using CCKInf2U.ThreadSafe;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static CCKInf2U.Interop.GciConstants;

namespace CCKInf2U;

[SkipLocalsInit]
public sealed class GemStoneSession
{
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
#nullable restore
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal void AddErrorAcceptingBreaks(ref GciErrSType error, bool fromWaitForEvent = false)
	{
#nullable enable

		var errorData = GetGemstoneError(error);

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

	~GemStoneSession()
	{
		if (_activity?.IsStopped == false)
		{
			_activity.Dispose();
		}
	}
}
