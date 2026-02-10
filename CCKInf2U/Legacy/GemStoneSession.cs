using CCKCOMPONENTS;
using CCKInf2U.Interop;
using CCKUTIL2;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace CCKInf2U;

public sealed class GemStoneSession
{
	public event DisconnectedEventHandler Disconnected;
	public delegate void DisconnectedEventHandler(object sender);

	public event DisconnectForDayEndEventHandler DisconnectForDayEnd;
	public delegate void DisconnectForDayEndEventHandler(object sender);

	public event LivePipeDirtyEventHandler LivePipeDirty;
	public delegate void LivePipeDirtyEventHandler(object sender);

	private CCKXTIMERS.XTimer _disTimer;
	private CCKXTIMERS.XTimer DisTimer
	{
		[MethodImpl(MethodImplOptions.Synchronized)]
		get
		{
			return _disTimer;
		}

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
				_badSessionCounter = 0;
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
		set
		{
			_processAborts = value;
		}
	}

	private readonly CCKLog _logger = new()
	{
		sFileName = "Epinf2Error.log",
		bQuietMode = false,
	};

	private SignalTimer _signalHandler;
	private int _badSessionCounter;
	private bool _processAborts = true;

	public static GemStoneSession CreateGemStoneConnection()
	{
		// TODO(AB): Revisit then when creating the switch between legacy and threadsafe
		return new();
	}

	internal GemStoneSession()
	{
		DisTimer = new()
		{
			Interval = 64_000,
			Enabled = true,
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public GemStoneObject WrapOopInObjectContext(Oop oop)
	{
		return new(this, oop);
	}

	public GemStoneObject Execute(ReadOnlySpan<byte> command)
	{
		var oop = RT.RTExecute(command);

		var isError = default(bool);
		_ = CheckError(ref isError);

		return WrapOopInObjectContext(oop);
	}

	public Boolean IsInTransaction()
	{
		var err = false;	
		return ((Boolean)Execute("GsSession currentSession inTransaction"u8).AsLocal(ref err));
	}

	public GemStoneObject GlobalNamed(ReadOnlySpan<byte> name)
	{
		var oop = RT.RTGlobalNamed(name);

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

	public bool PollForSignal()
	{
		return RT.RTPollForSignal() != 0;
	}

	#region Login / Logout

	public bool LoginAsRootSubscriber(GemStoneLoginData loginData)
	{
		var rootSubscriberLoginData = loginData with
		{
			UserName = CCKConstants.GetRootSubscriberUserName(),
			Password = CCKConstants.GetRootSubscriberPassword(),
		};

		return Login(rootSubscriberLoginData);
	}

	public bool LoginAsUser(GemStoneLoginData loginData)
	{
		Logout();
		return Login(loginData);
	}

	private bool Login(GemStoneLoginData loginData)
	{
		if (!Init())
		{
			return false;
		}
		else
		{
			Logout();
		}

		var host = UTF8Encoding.UTF8.GetBytes(loginData.Host.Trim()).AsSpan();
		Span<byte> hostBuffer = stackalloc byte[host.Length + 1];
		host.CopyTo(hostBuffer);

		var gemServer = UTF8Encoding.UTF8.GetBytes(loginData.GemServer.Trim()).AsSpan();
		Span<byte> gemServerBuffer = stackalloc byte[gemServer.Length + 1];
		gemServer.CopyTo(gemServerBuffer);

		var netLDI = UTF8Encoding.UTF8.GetBytes(loginData.NetLDI.Trim()).AsSpan();
		Span<byte> netLDIBuffer = stackalloc byte[netLDI.Length + 1];
		netLDI.CopyTo(netLDIBuffer);

		var username = UTF8Encoding.UTF8.GetBytes(loginData.UserName.Trim()).AsSpan();
		Span<byte> usernameBuffer = stackalloc byte[username.Length + 1];
		username.CopyTo(usernameBuffer);

		var password = UTF8Encoding.UTF8.GetBytes(loginData.Password.Trim()).AsSpan();
		Span<byte> passwordBuffer = stackalloc byte[password.Length + 1];
		password.CopyTo(passwordBuffer);

		var hostUsername = UTF8Encoding.UTF8.GetBytes(loginData.HostUserName.Trim()).AsSpan();
		Span<byte> hostUsernameBuffer = stackalloc byte[hostUsername.Length + 1];
		hostUsername.CopyTo(hostUsernameBuffer);

		var hostPassword = UTF8Encoding.UTF8.GetBytes(loginData.HostPassword.Trim()).AsSpan();
		Span<byte> hostPasswordBuffer = stackalloc byte[hostPassword.Length + 1];
		hostPassword.CopyTo(hostPasswordBuffer);

		var loginSuccessful = RT.RTLogin(
			host: hostBuffer,
			gemServer: gemServerBuffer,
			netldi: netLDIBuffer,
			userId: usernameBuffer,
			passw: passwordBuffer,
			hostUserId: hostUsernameBuffer,
			hostPassw: hostPasswordBuffer) == 0;

		_badSessionCounter = 0;
		return loginSuccessful;
	}

	private bool Init()
	{
		return RT.RTInit() != 0;
	}

	public void Logout()
	{
		RT.RTLogout();
	}

	#endregion Login / Logout

	#region Transaction

	public void BeginTransaction()
	{
		RT.RTBegin();
		var isError = default(bool);
		CheckError(ref isError);
	}

	public void AbortTransaction()
	{
		if (_processAborts)
		{
			RT.RTAbort();
			var isError = default(bool);
			CheckError(ref isError);
		}
	}

	public bool CommitTransaction()
	{
		var commitSuccessful = RT.RTCommit() != 0;
		if (!commitSuccessful && Debugger.IsAttached)
		{
			Debugger.Break();
		}

		var isError = default(bool);
		CheckError(ref isError);

		if (!commitSuccessful)
		{
			Execute("Treasury logGuavaDealerCommitFailure"u8);
		}

		return commitSuccessful;
	}

	#endregion Transaction

	#region System Flags

	public void EnableAutomaticTransactionMode()
	{
		Execute("System transactionMode: #autoBegin"u8);
	}

	public void EnableManualTransactionMode()
	{
		Execute("System transactionMode: #manualBegin"u8);
	}

	public void EnableSignaledErrors()
	{
		RT.RTEnableSignaledErrors();
	}

	public void EnableSignaledObjectsError()
	{
		Execute("System enableSignaledObjectsError"u8);
	}

	public void EnableSignaledAbortError()
	{
		Execute("System enableSignaledAbortError"u8);
	}

	public void EnableSignals(bool enable)
	{
		_signalHandler ??= new(this);
		_signalHandler.EnableSignals(enable);
	}

	#endregion System Flags

	public string CheckError(ref bool isError, bool isQuiet = false, CCKLog inLogger = null, bool showLowLevelErrors = false)
	{
		isError = false; // TODO(AB): This should be `out`

		Oop cat = default;
		int errorNumber = default;
		Oop process = default;
		var buffer = ArrayPool<byte>.Shared.Rent(minimumLength: 1_025);
		var bufferSpan = buffer.AsSpan();
		bufferSpan.Clear(); // NOTE(AB): Won't be necessary if using an always cleared pool
		Span<Oop> args = stackalloc Oop[11];
		int argCount = default;

		if (RT.RTError(ref cat, ref errorNumber, ref process, bufferSpan, args, ref argCount) == 0)
		{
			ArrayPool<byte>.Shared.Return(buffer);
			return null;
		}

		var nullTermIndex = bufferSpan.IndexOf((byte)0);
		var gemStoneErrorMessage = nullTermIndex != 0
			? bufferSpan[..nullTermIndex].DecodeUTF8()
			: string.Empty;
		ArrayPool<byte>.Shared.Return(buffer);

		Debug.Print($"CheckError= {errorNumber}  Msg= {gemStoneErrorMessage}");

		var logger = inLogger ?? _logger;

		if (errorNumber == 4100)
		{
			_badSessionCounter++;

			string maybeErrorMessage = null;

			if (_badSessionCounter == 100)
			{
				maybeErrorMessage = $"""
					Connection with the backoffice has been lost. The server may be down or the session may have been terminated forcibly!
					{gemStoneErrorMessage}
					Please restart the application!
					""";
				logger.LogEvent(
					DateTime.Now,
					CCKConstants.CCK_ENUM_ERROR_CATEGORY.CCK_ENUM_ERROR_CATEGORY_SYSTEM,
					CCKConstants.CCK_ENUM_ERROR_TYPE.CCK_ENUM_ERROR_TYPE_ERROR,
					maybeErrorMessage);
				Disconnected?.Invoke(this);
			}

			isError = true;
			return maybeErrorMessage;
		}

		// REFACTOR NOTE(AB): Inverting show low level feels off.
		if (!showLowLevelErrors && errorNumber is 2010 or 2101 or 2103 or 2106 or 2404)
		{
			var errorMessage = $"Guava Ops exception {errorNumber} received: {gemStoneErrorMessage}";
			inLogger?.LogEvent(
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
			RT.RTAbort();
			EnableSignaledAbortError();
			return null;
		}
		else if (errorNumber == 3031)
		{
			// This is a signal to advise that we were running outside a transaction
			// and didn't respond to a sig abort. Do an abort here
			RT.RTAbort();
			return null;
		}

		var eventMessage = $"Guava Ops Event No:{errorNumber}\r\n";

		if (argCount <= 0)
		{
			return StandardNonExceptionErrorMessage(ref isError);
		}

		var exceptionOop = args[argCount - 1];
		var exceptionI = WrapOopInObjectContext(exceptionOop);

		if (exceptionI.ForeignPerform("isException"u8).PrintString() != "true")
		{
			return StandardNonExceptionErrorMessage(ref isError);
		}

		eventMessage = $"{eventMessage}\r\n{exceptionI.ForeignPerform("displayString"u8).PrintString()}";

		if (exceptionI.ForeignPerform("isError"u8).PrintString() == "true")
		{
			RT.RTAbort();
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
				CCKMsgBox.WrappedMsgBox(eventMessage, CCKMsgBox.WrappedMsgBoxStyle.Critical, "Guava Ops Error");
			}

			isError = true;
			_error = true;
			return eventMessage;
		}
		else if (exceptionI.ForeignPerform("isWarning"u8).PrintString() == "true")
		{
			if (isQuiet || IsQuietACS)
			{
				logger.LogEvent(
					DateTime.Now,
					CCKConstants.CCK_ENUM_ERROR_CATEGORY.CCK_ENUM_ERROR_CATEGORY_DATA,
					CCKConstants.CCK_ENUM_ERROR_TYPE.CCK_ENUM_ERROR_TYPE_WARNING,
					eventMessage);
				RT.RTContinue(process);
			}
			else
			{
				eventMessage = $"{eventMessage}\r\nContinue?";

				using frmMsg messageForm = new()
				{
					lTimeOut = 30,
					sMsg = eventMessage,
					bShowYesNo = true,
				};

				messageForm.Display(true);

				if (messageForm.Result)
				{
					RT.RTContinue(process);
				}
				else
				{
					RT.RTAbort();
					EnableSignaledAbortError();
					isError = true;
				}
			}

			return eventMessage;
		}
		else if (exceptionI.ForeignPerform("isInformation"u8).PrintString() == "true")
		{
			RT.RTContinue(process);

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
				using frmMsg messageForm = new()
				{
					sMsg = eventMessage,
					lTimeOut = 300,
				};

				messageForm.Display();
			}

			return eventMessage;
		}

		// REFACTOR NOTE(AB): VB had this path implicitly returning null (code path wasn't set) - this feels wrong.
		return null;

		string StandardNonExceptionErrorMessage(ref bool errorFlag)
		{
			RT.RTAbort();
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

	public void CheckSignal()
	{
		while (true)
		{
			var signalI = Execute("InterSessionSignal poll"u8);
			if (signalI.IsNil())
			{
				// This means something went wrong with the call, likely not good as
				// signalFromGemStoneSession should never return nil. Bounce out.
				return;
			}

			var isError = false;

			var signalValue = signalI.ForeignPerform("sentInt"u8).AsInteger(ref isError);
			if (signalValue is 2 or 3)
			{
				// A signal with a message we care about.
				var signalMessage = signalI.ForeignPerform("sentMessage"u8).AsString(ref isError);

				var logMessage = signalValue == 2
					? "Backoffice Credit Limit Breach Warning:\r\n\r\n" + signalMessage
					// 3
					: "Backoffice Short Position Warning:\r\n\r\n" + signalMessage;

				if (!(IsQuiet || IsQuietACS))
				{
					frmMsg messageBox = new()
					{
						sMsg = logMessage,
						lTimeOut = 120,
					};
					messageBox.Display();
				}
				else
				{
					_logger.LogEvent(
						DateTime.Now,
						CCKConstants.CCK_ENUM_ERROR_CATEGORY.CCK_ENUM_ERROR_CATEGORY_SYSTEM,
						CCKConstants.CCK_ENUM_ERROR_TYPE.CCK_ENUM_ERROR_TYPE_INFO,
						logMessage);
				}
			}
			else if (signalValue == 4)
			{
				// End of the day, don't process any further signals
				DisconnectForDayEnd?.Invoke(this);
				return;
			}

			// Signals with other values are ignored.
		}
	}
}