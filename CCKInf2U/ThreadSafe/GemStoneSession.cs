using CCKCOMPONENTS;
using CCKInf2U.ThreadSafe;
using CCKUTIL2;
using CCKXTIMERS;
using Microsoft.Extensions.Logging;
using SparkSupport;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using static CCKInf2U.Interop.GciConstants;

namespace CCKInf2U;

[SkipLocalsInit]
public sealed class GemStoneSession
{
	public CCKLogNameHolder LogNameHolder;
	private bool enableTylersLogChanges = true;

	#region New API

	[MaybeNull]
	internal GemStoneErrorData LastError { get; private set; } = null;

	public string CurrentUser { get; set; }

	public event OpsErrorEventHandler OpsErrorEvent;

	public delegate void OpsErrorEventHandler(string error, string user, bool isQuiet);

	public event OpsWarningEventHandler OpsWarningEvent;

	public delegate void OpsWarningEventHandler(string error, string user, bool isQuiet);

	public event OpsInterSessionSignalEventHandler OpsInterSessionSignalEvent;

	public delegate void OpsInterSessionSignalEventHandler(string error, string user, bool isQuiet);

	internal GemStoneErrorData LastErrorFromWaitForEvent { get; private set; } = null;

	internal GciSession SessionId { get; private set; } = GciSession.Zero;

	public bool ShouldLogGemStoneErrors { private get; set; }

	[MemberNotNullWhen(true, nameof(LastError))]
	private bool HasError => LastError is not null;

	internal bool IsActiveSession => SessionId != 0;

	public string LastTraceIdToOps { get; set; }

	private static unsafe GemStoneErrorData GetGemstoneError(GciErrSType error)
	{
		var local = error;

		var messageBuffer = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(local.message);
		var reasonBuffer = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(local.reason);

		Oop[]? argsBuffer = null;
		if (local.argCount > 0)
		{
			var rawArgs = new ReadOnlySpan<Oop>(local.args, local.argCount);
			argsBuffer = new Oop[local.argCount];
			rawArgs.CopyTo(argsBuffer.AsSpan());
		}

		GemStoneErrorData errorData =
			new (
				Category: local.category,
				Context: local.context,
				ExceptionObj: local.exceptionObj,
				Args: argsBuffer,
				Number: local.number,
				ArgCount: local.argCount,
				Fatal: local.fatal,
				Message: messageBuffer.Length != 0 ? messageBuffer.DecodeUTF8() : null,
				Reason: reasonBuffer.Length != 0 ? reasonBuffer.DecodeUTF8() : null,
				When: DateTime.Now);

		return errorData;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal void AddError(ref GciErrSType error, bool fromWaitForEvent = false)
	{
#nullable enable
		var errorData = GetGemstoneError(error);

		// System.Diagnostics.Debugger.Break();

		if (fromWaitForEvent)
		{
			LastErrorFromWaitForEvent = errorData;
		}
		else
		{
			LastError = errorData;
		}

		if (false)
		{
			ExecuteString($"System beginTransaction ");
			ExecuteString($"Published at:#SparkException put: (nil objectWithOop: {errorData.ExceptionObj}). ");
			ExecuteString($"System commitTransaction ");
		}

		if (ShouldLogGemStoneErrors)
		{
			// ! This code should only be used in debug builds, but we'll include it in release for now.
			_logger.LogEvent(
				LastError.When,
				CCKConstants.CCK_ENUM_ERROR_CATEGORY.CCK_ENUM_ERROR_CATEGORY_SYSTEM,
				CCKConstants.CCK_ENUM_ERROR_TYPE.CCK_ENUM_ERROR_TYPE_INFO,
				$"GemStone Error logged - {LastError.Number}: {LastError.Message}");
		}
#nullable restore
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal void AddErrorAcceptingBreaks(ref GciErrSType error, bool fromWaitForEvent = false)
	{
#nullable enable

		var errorData = GetGemstoneError(error);

		if (false)
		{
			ExecuteString($"System beginTransaction ");
			ExecuteString($"Published at:#SparkException put: (nil objectWithOop: {errorData.ExceptionObj}). ");
			ExecuteString($"System commitTransaction ");
		}

		if (errorData.Number is not 6003 and not 6004)
		{
			System.Diagnostics.Debugger.Break();
			if (fromWaitForEvent)
			{
				LastErrorFromWaitForEvent = errorData;
			}
			else
			{
				LastError = errorData;
			}

			if (ShouldLogGemStoneErrors)
			{
				// ! This code should only be used in debug builds, but we'll include it in release for now.
				_logger.LogEvent(
					LastError.When,
					CCKConstants.CCK_ENUM_ERROR_CATEGORY.CCK_ENUM_ERROR_CATEGORY_SYSTEM,
					CCKConstants.CCK_ENUM_ERROR_TYPE.CCK_ENUM_ERROR_TYPE_INFO,
					$"GemStone Error logged - {LastError.Number}: {LastError.Message}");
			}
		}

#nullable restore
	}

	public void CheckInterSessionSignal(int? initialSentInt = null, string? iniyialSentMessage = null)
	{
		var error = false;
		string finalMessage = null;
		int? signalType = initialSentInt;
		string? signalMessage = iniyialSentMessage;
		switch (signalType)
		{
			case 2:
				finalMessage =
				(
					$"""
					 Backoffice Credit Limit Breach Warning:

					 {signalMessage}
					 """);

				break;
			case 3:
				finalMessage =
				(
					$"""
					 Backoffice Short Position Warning:

					 {signalMessage}
					 """);

				break;
			default:
				break;
		}

		OpsInterSessionSignalEvent?.Invoke(finalMessage, CurrentUser, (IsQuiet || IsQuietACS));
	}

	#endregion New API

	#region Legacy API

	public event DisconnectedEventHandler Disconnected;

	public delegate void DisconnectedEventHandler(object sender);

	private CCKXTIMERS.IXTimer _disTimer;

	private CCKXTIMERS.IXTimer DisTimer
	{
		[MethodImpl(MethodImplOptions.Synchronized)]
		get { return _disTimer; }

		[MethodImpl(MethodImplOptions.Synchronized)]
		set
		{
			if (_disTimer != null)
			{
				_disTimer.Tick -= DisTimer_Tick;
			}

			_disTimer = value;
			if (_disTimer != null)
			{
				_disTimer.Tick += DisTimer_Tick;
			}

			void DisTimer_Tick()
			{
				_ = Interlocked.Exchange(ref _badSessionCounter, 0);
			}
		}
	}

	private bool _error = false;

	public bool Error
	{
		get
		{
			var wasError = _error;
			_error = false;
			return wasError;
		}
	}

	private GemStoneObject _trueI;

	private GemStoneObject TrueI
	{
		get
		{
			_trueI ??= GlobalNamed("true"u8);
			return _trueI;
		}
	}

	private GemStoneObject _falseI;

	private GemStoneObject FalseI
	{
		get
		{
			_falseI ??= GlobalNamed("false"u8);
			return _falseI;
		}
	}

	public bool IsQuiet { get; set; }

	public bool IsQuietACS { get; set; } // REFACTOR NOTE(AB): A specific quiet for action server... Why?

	public bool ProcessAborts
	{
		set { _processAborts = value; }
	}

	public ILogger<GemStoneSession>? Logger { get; set; }
	public OpsMetrics? OpsMeter { get; set; }

	private readonly CCKLog _logger;

	private int _badSessionCounter;
	private bool _processAborts = true;

	public static GemStoneSession CreateGemStoneConnection(CCKLogNameHolder lsLogName, [MaybeNull] AtomicAccessLock accessLock)
	{
		// TODO(AB): Revisit then when creating the switch between legacy and threadsafe
		return new (lsLogName, accessLock);
	}

	internal GemStoneSession( CCKLogNameHolder lsLogName,[MaybeNull] AtomicAccessLock accessLock)
	{
		LogNameHolder = lsLogName;
		_logger = 
			new (LogNameHolder)
			{
				sFileName = "Epinf2Error.log",
				bQuietMode = false,
			};
		if (accessLock is not null)
		{
			DisTimer =
				new SparkXTimer(accessLock, "DisTimer")
				{
					Interval = 64_000,
					Enabled = true,
				};
		}
		else
		{
			DisTimer =
				new XTimer()
				{
					Interval = 64_000,
					Enabled = true,
				};
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public GemStoneObject WrapOopInObjectContext(Oop oop)
	{
		return new (this, oop);
	}

	public GemStoneObject Execute(ReadOnlySpan<byte> command)
	{
		var oop = FFI.Execute(this, command);

		var isError = default(bool);
		_ = CheckError(ref isError);

		return WrapOopInObjectContext(oop);
	}

	public bool NbExecute(ReadOnlySpan<byte> command)
	{
		return FFI.NbExecute(this, command);
	}

	public GemStoneObject ExecuteString(string code)
	{
		return Execute(Encoding.ASCII.GetBytes(code).AsSpan());
	}

	public void ExecuteForLogs(string code)
	{
		/// TODO TG: need special execute for traces and logs?
	}

	public bool NbForeignPerform(Oop receiver, ReadOnlySpan<byte> selector, ReadOnlySpan<Oop> args)
	{
		return FFI.NbForeignPerform(this, receiver, selector, args);
	}

	public GemStoneObject NbResult()
	{
		var oop = FFI.NbResult(this);

		var isError = default(bool);
		_ = CheckError(ref isError);

		return WrapOopInObjectContext(oop);
	}

	public GemStoneObject NbResultAcceptingBreaks()
	{
		var oop = FFI.NbResultAcceptingBreaks(this);

		var isError = default(bool);
		_ = CheckError(ref isError);

		return WrapOopInObjectContext(oop);
	}

	public enum StatusOfNbCall
	{
		ResultNotReady,
		ResultOrErrorReady,
		Error
	}

	public StatusOfNbCall IsResultReady(int timeout)
	{
		var stateInt = FFI.NonBlockingPoll(this, timeout);
		return stateInt switch
		{
			0 => StatusOfNbCall.ResultNotReady,
			1 => StatusOfNbCall.ResultOrErrorReady,
			_ => StatusOfNbCall.Error
		};
	}

	public void SoftBreak()
	{
		FFI.SoftBreak(this);
	}

	public GemStoneObject GlobalNamed(ReadOnlySpan<byte> name)
	{
		var oop = FFI.ResolveSymbol(this, name);

		var isError = default(bool);
		_ = CheckError(ref isError);

		return WrapOopInObjectContext(oop);
	}

	public GemStoneObject ConvertStringToInfIISymbol(string str)
	{
		return GlobalNamed("Symbol"u8).ForeignPerformWithArgs("withAll:"u8, str);
	}

	public GemStoneObject ConvertBoolToInfII(bool @bool)
	{
		return @bool ? TrueI : FalseI;
	}

	public GemStoneObject NewInfIIOrderedCollection()
	{
		return GlobalNamed("OrderedCollection"u8).ForeignPerform("new"u8);
	}

	#region Login / Logout

	public bool LoginAsRootSubscriber(GemStoneLoginData loginData)
	{
		var rootSubscriberLoginData =
			loginData with
			{
				Username = CCKConstants.GetRootSubscriberUserName(),
				Password = CCKConstants.GetRootSubscriberPassword(),
			};

		return Login(rootSubscriberLoginData);
	}

	public bool LoginAsUser(GemStoneLoginData loginData)
	{
		var isLogedIn = Login(loginData);
		if (isLogedIn)
		{
			CurrentUser = loginData.Username;
		}

		return isLogedIn;
	}

	private bool Login(GemStoneLoginData loginData)
	{
		Logout();

		var username = loginData.Username.AsSpan().Trim();
		var usernameCount = Encoding.UTF8.GetByteCount(username);
		Span<byte> usernameBuffer = stackalloc byte[usernameCount + 1];
		Encoding.UTF8.GetBytes(username, usernameBuffer);
		usernameBuffer[^1] = 0;

		var password = loginData.Password.AsSpan().Trim();
		var passwordCount = Encoding.UTF8.GetByteCount(password);
		Span<byte> passwordBuffer = stackalloc byte[passwordCount + 1];
		Encoding.UTF8.GetBytes(password, passwordBuffer);
		passwordBuffer[^1] = 0;

		var hostUsername = loginData.HostUserName.AsSpan().Trim();
		var hostUsernameCount = Encoding.UTF8.GetByteCount(hostUsername);
		Span<byte> hostUsernameBuffer = stackalloc byte[hostUsernameCount + 1];
		Encoding.UTF8.GetBytes(hostUsername, hostUsernameBuffer);
		hostUsernameBuffer[^1] = 0;

		var hostPassword = loginData.HostPassword.AsSpan().Trim();
		var hostPasswordCount = Encoding.UTF8.GetByteCount(hostPassword);
		Span<byte> hostPasswordBuffer = stackalloc byte[hostPasswordCount + 1];
		Encoding.UTF8.GetBytes(hostPassword, hostPasswordBuffer);
		hostPasswordBuffer[^1] = 0;

		// TODO(AB): 0 alloc these two.
		ReadOnlySpan<byte> stoneName =
			Encoding
				.UTF8
				.GetBytes($"!@{loginData.Host}!{loginData.GemServer}\0")
				.AsSpan();

		ReadOnlySpan<byte> gemService =
			Encoding
				.UTF8
				.GetBytes($"!tcp@{loginData.Host}#netldi:{loginData.NetLDI}#task!gemnetobject\0")
				.AsSpan();

		var sessionId =
			FFI.Login(
				this,
				stoneName,
				hostUsernameBuffer,
				hostPasswordBuffer,
				gemService,
				usernameBuffer,
				passwordBuffer);

		SessionId = sessionId;
		Interlocked.Exchange(ref _badSessionCounter, 0);

		if (sessionId != GciSession.Zero)
		{
			Execute("System enableSignaledGemStoneSessionError"u8);
			FFI.CallSocket(this);
			// RegisterEventHandler(); // This was for WaitForEvent... Fuck off.
			return true;
		}

		return false;
	}

	public void Logout()
	{
		if (IsActiveSession)
		{
			// DeregisterEventHandler();
			// Execute("System disableSignaledGemStoneSessionError"u8); // Not needed!
			FFI.Logout(this);
		}

		LastError = null;
		SessionId = GciSession.Zero;
	}

	#endregion Login / Logout

	#region Transaction

	internal readonly ActivitySource TransactionLevelActivitySource = new ActivitySource("Ops");
	internal Activity? _activity;
	private int _transactionCount = 0;

	public int TransactionCount()
	{
		return _transactionCount;
	}

	public void BeginTransaction()
	{
		_activity?.Stop();
		_activity?.Dispose();
		_activity =
			TransactionLevelActivitySource
				.StartActivity("<<<<< Begining Gemstone Transaction on {sessionid} >>>>>");

		_activity?.AddTag("sessionid", SessionId);
		Logger?.LogInformation("Begining Gemstone transaction.");

		_ = Execute("Treasury prepareForBegin"u8); // TODO(CCK-3228): Is this necessary?

		_ = FFI.BeginTransaction(this);

		var isError = default(bool);
		_ = CheckError(ref isError);
		_transactionCount++;
	}

	public void AbortTransaction()
	{
		using (Activity AbortActivity =
			TransactionLevelActivitySource
				.StartActivity("<<<<< Aborting Gemstone Transaction on {sessionid} >>>>>"))
		{
			AbortActivity?.AddTag("sessionid", SessionId);
			if (_processAborts)
			{
				_ = Execute("Treasury prepareForAbort"u8); // TODO(CCK-3228): Is this necessary?

				BasicAbortTransaction();

				if (_transactionCount > 0)
				{
					_transactionCount--;
				}
			}
		}

		_activity?.Dispose();
		_activity = null;
		if (!enableTylersLogChanges)
		{
			_activity =
				TransactionLevelActivitySource
					.StartActivity("<<<<< Begining AUTOMATIC Gemstone Transaction on {sessionid} >>>>>");

			_activity?.AddTag("sessionid", SessionId);
		}

		Logger?.LogInformation("Auto begining Gemstone transaction.");
	}

	public void BasicAbortTransaction()
	{
		_ = FFI.AbortTransaction(this);
		var isError = default(bool);
		_ = CheckError(ref isError);
	}

	public bool CommitTransaction()
	{
		bool commitSuccessful;
		using (Activity CommitActivity =
			TransactionLevelActivitySource
				.StartActivity("<<<<< Committing Gemstone Transaction on {sessionid} >>>>>"))
		{
			_ = Execute("Treasury prepareForCommit"u8); // TODO(CCK-3228): Is this necessary?

			commitSuccessful = FFI.CommitTransaction(this);

			if (!commitSuccessful && Debugger.IsAttached)
			{
				Execute(
					"""
								| oldTransactions |
								oldTransactions := System transactionConflicts.
								System beginTransaction.
								Published at: #SparkLastCommitConflicts ifAbsentPut: (OrderedCollection new).
								(Published at: #SparkLastCommitConflicts) add: oldTransactions. 
								System commitTransaction.
					"""u8);


				Debugger.Break();
			}

			var isError = default(bool);
			_ = CheckError(ref isError);

			if (!commitSuccessful)
			{
				_ = Execute("Treasury logGuavaDealerCommitFailure"u8);
				_activity?.SetStatus(ActivityStatusCode.Error);
			}

			if (_transactionCount > 0)
			{
				_transactionCount--;
			}
		}

		_activity?.Dispose();
		_activity = null;
		if (!enableTylersLogChanges)
		{
			_activity =
				TransactionLevelActivitySource
					.StartActivity("<<<<< Begining AUTOMATIC Gemstone Transaction on {sessionid} >>>>>");

			_activity?.AddTag("sessionid", SessionId);
		}

		Logger?.LogInformation("Auto begining Gemstone transaction.");
		return commitSuccessful;
	}

	#endregion Transaction

	#region System Flags

	public void EnableAutomaticTransactionMode()
	{
		Execute("System transactionMode: #autoBegin"u8);
	}

	public bool IsCurrentTransactionModeAutoBegin()
	{
		var isError = false;
		var mode = (bool)Execute("GsSession currentSession transactionMode == #autoBegin"u8).AsLocal(ref isError);
		return mode;
	}

	public void EnableManualTransactionMode()
	{
		Execute("System transactionMode: #manualBegin"u8);
	}

	public void EnableSignaledObjectsError()
	{
		Execute("System enableSignaledObjectsError"u8);
	}

	public void EnableSignaledAbortError()
	{
		Execute("System enableSignaledAbortError"u8);
	}

	public void EnableSignals()
	{
		//Execute("InterSessionSignal enableSignalling"u8);
	}

	public void DisableSignals()
	{
		//Execute("InterSessionSignal disableSignalling"u8);
	}
	
	public void EnableSignalsNew()
	{
		Execute("InterSessionSignal enableSignalling"u8);
	}

	public void DisableSignalsNew()
	{
		Execute("InterSessionSignal disableSignalling"u8);
	}

	#endregion System Flags

	public string CheckError(
		ref bool isError,
		bool isQuiet = false,
		CCKLog inLogger = null,
		bool showLowLevelErrors = false
	)
	{
		if (HasError)
		{
			var anError = ProcessError(ref isError, isQuiet, inLogger, showLowLevelErrors);
			return anError;
		}

		isError = false;
		return null;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private unsafe string ProcessError(ref bool isError, bool isQuiet, CCKLog inLogger, bool showLowLevelErrors)
	{
		// TODO(AB): Return value of this is only ever used in user management - clear that up and return void instead.
		// TODO(AB): Replace the existing error handling process with something more robust.

		isError = false;

		if (!HasError)
		{
			return null;
		}

		// Mimicing the existing pattern - Get the latest error to process and clear it.
		// Same as "Check signal, take top error, ignore the rest."
		// We have extra information in scope - but we only use what was available in the original system.

		var currentError = LastError;
		LastError = null;

		var errorNumber = currentError.Number;
		var gemStoneErrorMessage = currentError.Message ?? string.Empty;

		#if DEBUG
		Debug.Print($"CheckError= {errorNumber}  Msg= {gemStoneErrorMessage}");
		#endif

		var logger = inLogger ?? _logger;

		if (errorNumber is > 4000 and < 5000)
		{
			string OnSessionDisconection()
			{
				string message;
				message =
					$"""
					 Connection with the backoffice has been lost. The server may be down or the session may have been terminated forcibly!
					 {gemStoneErrorMessage}
					 Please restart the application!
					 """;

				logger.LogEvent(
					DateTime.Now,
					CCKConstants.CCK_ENUM_ERROR_CATEGORY.CCK_ENUM_ERROR_CATEGORY_SYSTEM,
					CCKConstants.CCK_ENUM_ERROR_TYPE.CCK_ENUM_ERROR_TYPE_ERROR,
					message);

				Disconnected?.Invoke(this);
				return message;
			}
			// The 4xxx range of errors represent fatal errors.
			// TODO(AB): Highly doubt the handling below is sufficient as the majority of the error descriptions
			// seem very fatal. To address later.

			_ = Interlocked.Add(ref _badSessionCounter, 1);
			isError = true;
			if (errorNumber is 4059 or 4052)
			{
				return OnSessionDisconection();
			}

			if (_badSessionCounter == 100)
			{
				return OnSessionDisconection();
			}

			return null;
		}

		// REFACTOR NOTE(AB): Inverting show low level feels off.
		if (!showLowLevelErrors && errorNumber is 2010 or 2101 or 2103 or 2106 or 2404)
		{
			var errorMessage = $"Guava Ops exception {errorNumber} received: {gemStoneErrorMessage}";
			logger?.LogEvent(
				DateTime.Now,
				CCKConstants.CCK_ENUM_ERROR_CATEGORY.CCK_ENUM_ERROR_CATEGORY_SYSTEM,
				CCKConstants.CCK_ENUM_ERROR_TYPE.CCK_ENUM_ERROR_TYPE_INFO,
				errorMessage);

			isError = true;
			return errorMessage;
		}
		else if (errorNumber == 6008)
		{
			EnableSignaledObjectsError();
			return null;
		}
		else if (errorNumber == 6009)
		{
			FFI.AbortTransaction(this);
			EnableSignaledAbortError();
			return null;
		}
		else if (errorNumber == 6010)
		{
			// TO DO for Thushar to handle
			// we need to handle 6010
			// we need to pass the limit breach message
			// to the UI of the app and SPARK
			FFI.AbortTransaction(this);
			var intOOp = currentError.Args[1];
			var msgOop = currentError.Args[2];

			var integer = new GemStoneObject(this, intOOp).AsInteger(ref isError);
			var megssage = new GemStoneObject(this, msgOop).AsString(ref isError);

			if (!isError)
			{
				CheckInterSessionSignal(integer, megssage);
			}

			EnableSignaledObjectsError();
			EnableSignaledAbortError();
			EnableSignals();
			return null;
		}
		else if (errorNumber == 3031)
		{
			// This is a signal to advise that we were running outside a transaction
			// and didn't respond to a sig abort. Do an abort here
			FFI.AbortTransaction(this);
			return null;
		}

		var eventMessage = $"Guava Ops Event No:{errorNumber}\r\n";

		var args = currentError.Args ?? ReadOnlySpan<Oop>.Empty;
		Oop exceptionOop;
		exceptionOop = currentError.ExceptionObj;
		var exceptionI = WrapOopInObjectContext(exceptionOop);
		if (exceptionI.IsNil())
		{
			// Investigate if anything more needs to be done here
			return null;
		}

		if (!exceptionI.ForeignPerform("isException"u8).IsTrue())
		{
			// Gemstone exception
			return GemstoneErrorOccured(ref isError);
		}
		else
		{
			var argCount = currentError.ArgCount;
			if (argCount <= 0)
			{
				return GemstoneErrorOccured(ref isError);
			}

			// Ops Exception
			// ops error
			if (!args.IsEmpty)
			{
				exceptionOop = args[argCount - 1];
			}
			else
			{
				exceptionOop = currentError.ExceptionObj;
			}

			exceptionI = WrapOopInObjectContext(exceptionOop);

			if (exceptionI.IsNil())
			{
				// Investigate if anything more needs to be done here
				return null;
			}
		}
		// exceptionI should be an ops error at this point.
		return ProcessErrorException(
			exceptionI,
			currentError.Context,
			eventMessage,
			logger,
			isQuiet,
			ref isError);

		string GemstoneErrorOccured(ref bool errorFlag)
		{
			eventMessage = $"Gemstone error occured: {eventMessage}{gemStoneErrorMessage}";

			logger.LogEvent(
				DateTime.Now,
				CCKConstants.CCK_ENUM_ERROR_CATEGORY.CCK_ENUM_ERROR_CATEGORY_SYSTEM,
				CCKConstants.CCK_ENUM_ERROR_TYPE.CCK_ENUM_ERROR_TYPE_ERROR,
				eventMessage);

			errorFlag = true;
			return eventMessage;
		}


		string StandardNonExceptionErrorMessage(ref bool errorFlag)
		{ 
			// TODO: TG, TS At which point should we decide to log and reconnect the session -- investigate according to VB app behavior
			FFI.AbortTransaction(this);
			EnableSignaledAbortError();

			eventMessage = $" Network/session disconnection error.{eventMessage}{gemStoneErrorMessage}";

			logger.LogEvent(
				DateTime.Now,
				CCKConstants.CCK_ENUM_ERROR_CATEGORY.CCK_ENUM_ERROR_CATEGORY_SYSTEM,
				CCKConstants.CCK_ENUM_ERROR_TYPE.CCK_ENUM_ERROR_TYPE_ERROR,
				eventMessage);

			errorFlag = true;
			Disconnected?.Invoke(this);
			return eventMessage;
		}
	}

	private string ProcessErrorException(
		GemStoneObject exceptionI,
		Oop process,
		string baseEventMessage,
		CCKLog logger,
		bool isQuiet,
		ref bool isError
	)
	{
		var eventMessage = $"{baseEventMessage}\r\n{exceptionI.ForeignPerform("displayString"u8).PrintString()}";

		if (exceptionI.ForeignPerform("isError"u8).IsTrue())
		{
			FFI.AbortTransaction(this);
			EnableSignaledAbortError();

			if (IsQuiet || IsQuietACS)
			{
				logger.LogEvent(
					DateTime.Now,
					CCKConstants.CCK_ENUM_ERROR_CATEGORY.CCK_ENUM_ERROR_CATEGORY_SYSTEM,
					CCKConstants.CCK_ENUM_ERROR_TYPE.CCK_ENUM_ERROR_TYPE_ERROR,
					eventMessage);
			}
			else
			{
				_ = _logger.CCKMsgBox.WrappedMsgBox(eventMessage, CCKMsgBox.WrappedMsgBoxStyle.Critical, "Guava Ops Error");
			}

			isError = true;
			_error = true;
			OpsErrorEvent?.Invoke(eventMessage, CurrentUser, (IsQuiet || IsQuietACS));
			return eventMessage;
		}
		else if (exceptionI.ForeignPerform("isWarning"u8).IsTrue())
		{
			FFI.ContinueProcessAfterException(this, process);

			if (isQuiet || IsQuietACS)
			{
				logger.LogEvent(
					DateTime.Now,
					CCKConstants.CCK_ENUM_ERROR_CATEGORY.CCK_ENUM_ERROR_CATEGORY_DATA,
					CCKConstants.CCK_ENUM_ERROR_TYPE.CCK_ENUM_ERROR_TYPE_WARNING,
					eventMessage);
			}
			else
			{
				_ = _logger.CCKMsgBox.WrappedMsgBox(eventMessage, CCKMsgBox.WrappedMsgBoxStyle.Exclamation);
			}

			OpsWarningEvent?.Invoke(eventMessage, CurrentUser, (IsQuiet || IsQuietACS));
			return eventMessage;
		}
		else if (exceptionI.ForeignPerform("isInformation"u8).IsTrue())
		{
			FFI.ContinueProcessAfterException(this, process);

			if (isQuiet || IsQuietACS)
			{
				logger.LogEvent(
					DateTime.Now,
					CCKConstants.CCK_ENUM_ERROR_CATEGORY.CCK_ENUM_ERROR_CATEGORY_SYSTEM,
					CCKConstants.CCK_ENUM_ERROR_TYPE.CCK_ENUM_ERROR_TYPE_INFO,
					eventMessage);
			}
			else
			{
				_ = _logger.CCKMsgBox.WrappedMsgBox(eventMessage, CCKMsgBox.WrappedMsgBoxStyle.Exclamation);
			}

			return eventMessage;
		}

		// REFACTOR NOTE(AB): VB had this path implicitly returning null (code path wasn't set) - this feels wrong.
		return null;
	}

	#endregion Legacy API

	~GemStoneSession()
	{
		if (_activity?.IsStopped == false)
		{
			_activity.Dispose();
		}
	}
}
