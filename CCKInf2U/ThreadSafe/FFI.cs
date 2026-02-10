#nullable enable

using CCKInf2U.Interop;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using static CCKInf2U.Interop.GciConstants;

namespace CCKInf2U.ThreadSafe;

[SkipLocalsInit]
#pragma warning disable S101 // Types should be named in PascalCase - Acronym, Foreign Function Interface
internal static unsafe class FFI
#pragma warning restore S101 // Types should be named in PascalCase - Acronym, Foreign Function Interface
{
	/*
	 * FOREWORD(AB):
	 * 
	 * The majority of the return values from the functions in this class don't follow the TS standard - This is (hopefully)
	 * temporary and has been done to keep like-for-like compatibility with the existing code that used the legacy version
	 * of GemBuilder for C + RT wrapper that (almost) always defaulted to returning NIL.
	 */

	#region RT-Style Methods - To Replace

	public static Oop RTNewSYSDate(GemStoneSession session, Oop day, Oop month, Oop year)
	{
		var sysDateClass = ResolveSymbol(session, "SYSDate"u8);
		if (sysDateClass == GciOop.OOP_ILLEGAL)
		{
			return GciOop.OOP_NIL;
		}

		ReadOnlySpan<Oop> args = stackalloc Oop[3]
		{
			day,
			month,
			year,
		};

		return ForeignPerform(session, sysDateClass, "newDay:monthNumber:year:"u8, args);
	}

	#endregion RT-Style Methods - To Replace

	public static bool AbortTransaction(GemStoneSession session)
	{
		GciErrSType error = new();

		var abortSuccessful = GciThreadSafe.GciTsAbort(session.SessionId, ref error);
		if (abortSuccessful == 0)
		{
			session.AddError(ref error);
			return false;
		}

		return true;
	}

	public static bool BeginTransaction(GemStoneSession session)
	{
		GciErrSType error = new();

		var openedTransaction = GciThreadSafe.GciTsBegin(session.SessionId, ref error);
		if (openedTransaction == 0)
		{
			session.AddError(ref error);
			return false;
		}

		return true;
	}

	public static bool CommitTransaction(GemStoneSession session)
	{
		GciErrSType error = new();

		var committedTransaction = GciThreadSafe.GciTsCommit(session.SessionId, ref error);
		if (committedTransaction == 0)
		{
			session.AddError(ref error);
			return false;
		}

		return true;
	}

	public static bool ContinueProcessAfterException(GemStoneSession session, Oop process)
	{
		GciErrSType error = new();

		var continueResult = GciThreadSafe.GciTsContinueWith(
			session.SessionId,
			process,
			GciOop.OOP_ILLEGAL,
			null,
			0,
			ref error);

		if (continueResult == GciOop.OOP_ILLEGAL)
		{
			session.AddError(ref error);
			return false;
		}

		return true;
	}

	public static Oop Execute(GemStoneSession session, ReadOnlySpan<byte> command)
	{
		GciErrSType error = new();
		
		var oop = GciThreadSafe.GciTsExecute(
			session.SessionId,
			command,
			GciOop.OOP_CLASS_Utf8,
			GciOop.OOP_ILLEGAL,
			GciOop.OOP_NIL,
			0,
			0,
			ref error);

		if (oop == GciOop.OOP_ILLEGAL)
		{
			session.AddError(ref error);
			return GciOop.OOP_NIL;
		}
		

		return oop;
	}

	public static bool NbExecute(GemStoneSession session, ReadOnlySpan<byte> command)
	{
		GciErrSType error = new();

		var oop = GciThreadSafe.GciTsNbExecute(
			session.SessionId,
			command,
			GciOop.OOP_CLASS_Utf8,
			GciOop.OOP_ILLEGAL,
			GciOop.OOP_NIL,
			0,
			0,
			ref error);

		if (oop == 0)
		{
			session.AddError(ref error);
			return false;
		}

		return true;
	}

	public static bool NbForeignPerform(
	GemStoneSession session,
	Oop root,
	ReadOnlySpan<byte> selector,
	ReadOnlySpan<Oop> args)
	{
		GciErrSType error = new();

		var oop = GciThreadSafe.GciTsNbPerform(
			session.SessionId,
			root,
			GciOop.OOP_ILLEGAL,
			selector,
			args,
			args.Length,
			0,
			0,
			ref error);

		if (oop == 0)
		{
			session.AddError(ref error);
			return false;
		}

		return true;
	}

	public static Oop NbResult(GemStoneSession session)
	{
		GciErrSType error = new();

		var oop = GciThreadSafe.GciTsNbResult(
			session.SessionId,
			ref error);

		if (oop == GciOop.OOP_ILLEGAL)
		{
			session.AddError(ref error);

		}

		return oop;
	}

	public static Oop NbResultAcceptingBreaks(GemStoneSession session)
	{
		GciErrSType error = new();

		var oop = GciThreadSafe.GciTsNbResult(
			session.SessionId,
			ref error);

		if (oop == GciOop.OOP_ILLEGAL)
		{
			session.AddErrorAcceptingBreaks(ref error);

		}

		return oop;
	}


	public static int CallInProgress(GemStoneSession session)
	{
		GciErrSType error = new();

		var result = GciThreadSafe.GciTsCallInProgress(session.SessionId, ref error);

		if (result == -1)
		{
			session.AddError(ref error);

		}
		return result;
	}

	public static int NonBlockingPoll(GemStoneSession session, int timeout)
	{
		GciErrSType error = new();

		var result = GciThreadSafe.GciTsNbPoll(session.SessionId, timeout, ref error);

		if (result == -1)
		{
			session.AddError(ref error);

		}
		return result;
	}

	public static void SoftBreak(GemStoneSession session)
	{
		GciErrSType error = new();

		var result = GciThreadSafe.GciTsBreak(session.SessionId, 0, ref error);

		if (result == 0)
		{
			session.AddError(ref error);
		}
	}

	public static Oop ForeignPerform(
		GemStoneSession session,
		Oop root,
		ReadOnlySpan<byte> selector,
		ReadOnlySpan<Oop> args)
	{
		GciErrSType error = new();

		var oop = GciThreadSafe.GciTsPerform(
			session.SessionId,
			root,
			GciOop.OOP_ILLEGAL,
			selector,
			args,
			args.Length,
			0,
			0,
			ref error);

		if (oop == GciOop.OOP_ILLEGAL)
		{
			session.AddError(ref error);
			return GciOop.OOP_NIL;
		}

		return oop;
	}

	public static int GetCollectionObjects(GemStoneSession session, Oop root, long startIndex, Span<Oop> oops)
	{
		GciErrSType error = new();

		var oopCount = GciThreadSafe
			.GciTsFetchOops(session.SessionId, root, startIndex, oops, oops.Length, ref error);
		if (oopCount == -1)
		{
			session.AddError(ref error);
			return 0;
		}

		return oopCount;
	}

	public static double GetFloat(GemStoneSession session, Oop root)
	{
		GciErrSType error = new();
		Unsafe.SkipInit(out double @double);

		// Should only return false if `root` isn't a SmallDouble/Float
		var readSuccessful = GciThreadSafe.GciTsOopToDouble(session.SessionId, root, ref @double, ref error);
		if (readSuccessful == 0)
		{
			session.AddError(ref error);
			return 0D;
		}

		return @double;
	}

	public static long GetLargeInteger(GemStoneSession session, Oop root)
	{
		GciErrSType error = new();
		Unsafe.SkipInit(out long @long);

		// Should only return false if the number exceeds the bounds of i64  (2^63-1, 2^-63)
		var readSuccessful = GciThreadSafe.GciTsOopToI64(session.SessionId, root, ref @long, ref error);
		if (readSuccessful == 0)
		{
			session.AddError(ref error);
			return 0L;
		}

		return @long;
	}

	public static Oop GetObjectClass(GemStoneSession session, Oop root)
	{
		GciErrSType error = new();

		var classOop = GciThreadSafe.GciTsFetchClass(session.SessionId, root, ref error);
		if (classOop == GciOop.OOP_ILLEGAL)
		{
			session.AddError(ref error);
			return GciOop.OOP_NIL;
		}

		return classOop;
	}

	public static long GetObjectSize(GemStoneSession session, Oop root)
	{
		GciErrSType error = new();

		var size = GciThreadSafe.GciTsFetchSize(session.SessionId, root, ref error);
		if (size == -1)
		{
			session.AddError(ref error);
			return 0L;
		}

		return size;
	}

	public static ReadOnlySpan<byte> GetString(GemStoneSession session, Oop root, Span<byte> buffer)
	{
		// NOTE -- As this is using GciTsFetchUtf8 instead of GciTsFetchUtf8Bytes, the string is null terminated
		// and the caller needs to add 1 to the buffer before passing it in.

		GciErrSType error = new();
		Unsafe.SkipInit(out long requiredSize);

		var bytesWritten = GciThreadSafe
			.GciTsFetchUtf8(session.SessionId, root, buffer, (long)buffer.Length, ref requiredSize, ref error);

		if (bytesWritten == -1)
		{
			session.AddError(ref error);
			return ReadOnlySpan<byte>.Empty;
		}

		return buffer[..(int)bytesWritten];
	}

	public static ReadOnlySpan<byte> GetSingleByteString(GemStoneSession session, Oop root, Span<byte> buffer)
	{
		GciErrSType error = new();

		var bytesWritten = GciThreadSafe
			.GciTsFetchBytes(session.SessionId, root, 1L, buffer, (long)buffer.Length, ref error);

		if (bytesWritten == -1)
		{
			session.AddError(ref error);
			return ReadOnlySpan<byte>.Empty;
		}

		return buffer[..(int)bytesWritten];
	}

	public static bool IsKindOfClass(GemStoneSession session, Oop root, Oop @class)
	{
		GciErrSType error = new();

		var isClass = GciThreadSafe.GciTsIsKindOfClass(session.SessionId, root, @class, ref error);
		if (isClass == -1)
		{
			session.AddError(ref error);
			return false;
		}

		return isClass == 1;
	}

	public static GciSession Login(
		GemStoneSession session,
		ReadOnlySpan<byte> stoneName,
		ReadOnlySpan<byte> hostUsername,
		ReadOnlySpan<byte> hostPassword,
		ReadOnlySpan<byte> gemService,
		ReadOnlySpan<byte> username,
		ReadOnlySpan<byte> password)
	{
		var sessionStarted = 0;
		GciErrSType error = new();

		var sessionId = GciThreadSafe.GciTsLogin(
			StoneNameNrs: stoneName,
			HostUserId: hostUsername,
			HostPassword: hostPassword,
			hostPwIsEncrypted: 0,
			GemServiceNrs: gemService,
			gemstoneUsername: username,
			gemstonePassword: password,
			loginFlags: 0,
			haltOnErrNum: 0,
			executedSessionInit: ref sessionStarted,
			err: ref error);

		if (sessionId == 0 || sessionStarted == 0)
		{
			session.AddError(ref error);
			return GciSession.Zero;
		}

		return sessionId;
	}

	public static void Logout(GemStoneSession session)
	{
		GciErrSType error = new();

		if (GciThreadSafe.GciTsLogout(session.SessionId, ref error) == 0)
		{
			session.AddError(ref error);
		}
	}

	public static Oop NewFloat(GemStoneSession session, double @double)
	{
		GciErrSType error = new();

		var floatOop = GciThreadSafe.GciTsDoubleToOop(session.SessionId, @double, ref error);
		if (floatOop == GciOop.OOP_ILLEGAL)
		{
			session.AddError(ref error);
			return GciOop.OOP_NIL;
		}

		return floatOop;
	}

	public static Oop NewLargeInteger(GemStoneSession session, long @long)
	{
		GciErrSType error = new();

		var longOop = GciThreadSafe.GciTsI64ToOop(session.SessionId, @long, ref error);
		if (longOop == GciOop.OOP_ILLEGAL)
		{
			session.AddError(ref error);
			return GciOop.OOP_NIL;
		}

		return longOop;
	}

	public static Oop NewSingleByteString(GemStoneSession session, ReadOnlySpan<byte> bytes)
	{
		GciErrSType error = new();

		var stringOop = GciThreadSafe.GciTsNewString_(session.SessionId, bytes, (ulong)bytes.Length, ref error);
		if (stringOop == GciOop.OOP_ILLEGAL)
		{
			session.AddError(ref error);
			return GciOop.OOP_NIL;
		}

		return stringOop;
	}

	public static Oop NewString(GemStoneSession session, ReadOnlySpan<ushort> bytes)
	{
		GciErrSType error = new();

		var stringOop = GciThreadSafe.GciTsNewUnicodeString_(session.SessionId, bytes, (ulong)bytes.Length, ref error);
		if (stringOop == GciOop.OOP_ILLEGAL)
		{
			session.AddError(ref error);
			return GciOop.OOP_NIL;
		}

		return stringOop;
	}

	public static bool PersistObjects(GemStoneSession session, ReadOnlySpan<Oop> oops)
	{
		GciErrSType error = new();

		var bufferEntirelyProcessed = GciThreadSafe.GciTsSaveObjs(session.SessionId, oops, oops.Length, ref error);
		if (bufferEntirelyProcessed == 0)
		{
			session.AddError(ref error);
			return false;
		}

		return true;
	}

	public static Oop ResolveSymbol(GemStoneSession session, ReadOnlySpan<byte> name)
	{
		GciErrSType error = new();

		var oop = GciThreadSafe.GciTsResolveSymbol(session.SessionId, name, GciOop.OOP_NIL, ref error);
		if (oop == GciOop.OOP_ILLEGAL)
		{
			session.AddError(ref error);
		}

		return oop;
	}

	public static bool TryGetObjectInfo(GemStoneSession session, Oop root, out GciThreadSafe.GciTsObjInfo objectInfo)
	{
		GciErrSType error = new();
		GciThreadSafe.GciTsObjInfo localObjInfo = new();

		var fetchSuccessful = GciThreadSafe
			.GciTsFetchObjInfo(session.SessionId, root, 0, ref localObjInfo, null, (size_t)0, ref error);

		if (fetchSuccessful == -1)
		{
			session.AddError(ref error);
			objectInfo = default;
			return false;
		}

		objectInfo = localObjInfo;
		return true;
	}

	public static bool TryGetObjectInfoWithStringBuffer(
		GemStoneSession session,
		Oop root,
		out GciThreadSafe.GciTsObjInfo objectInfo,
		Span<byte> buffer,
		out ReadOnlySpan<byte> stringBuffer)
	{
		GciErrSType error = new();
		GciThreadSafe.GciTsObjInfo localObjInfo = new();

		int64 bytesWritten;

		fixed (byte* bufferPtr = &buffer.GetPinnableReference())
		{
			bytesWritten = GciThreadSafe
				.GciTsFetchObjInfo(session.SessionId, root, 1, ref localObjInfo, bufferPtr, (size_t)buffer.Length, ref error);
		}

		if (bytesWritten == -1)
		{
			session.AddError(ref error);
			objectInfo = default;
			stringBuffer = ReadOnlySpan<byte>.Empty;
			return false;
		}

		objectInfo = localObjInfo;

		// If we have a string-like object that returns the representation we're looking to retrieve in `buffer`
		// AND it was written in full, make it available for the caller via `stringBuffer`.
		// TODO(CCK-3328): This only seems (?) to work with single byte string types... Double check though.
		if (localObjInfo.objClass is GciOop.OOP_CLASS_STRING or GciOop.OOP_CLASS_SYMBOL)
		{
			stringBuffer = bytesWritten < buffer.Length
				? (ReadOnlySpan<byte>)buffer[..(int)bytesWritten]
				: ReadOnlySpan<byte>.Empty;
		}
		else
		{
			stringBuffer = ReadOnlySpan<byte>.Empty;
		}

		return true;
	}

	#region Abe's hacky shit

	public static bool CancelWaitForEvent(GemStoneSession session)
	{
		GciErrSType error = new();
		var result = GciThreadSafe.GciTsCancelWaitForEvent(session.SessionId, ref error);

		if (result == 0)
		{
			session.AddError(ref error, fromWaitForEvent: true);
			return false;
		}

		return true;
	}

	public static void CallSocket(GemStoneSession session)
	{
		GciErrSType error = new();

		var result = GciThreadSafe.GciTsSocket(session.SessionId, ref error);
		// Right now we don't care about the result, but for debugging we'll put it into a var
	}

	public static bool WaitForEvent(GemStoneSession session, [NotNullWhen(true)] out GciThreadSafe.GciEventType? eventType)
	{
		GciErrSType error = new();


		GciThreadSafe.GciEventType gsEventType = default;

		var result = GciThreadSafe.GciTsWaitForEvent(
			sess: session.SessionId,
			latencyMs: 10_000,
			evout: ref gsEventType,
			err: ref error);

		if (result == -1)
		{
			session.AddError(ref error, fromWaitForEvent: true);
			eventType = null;
			return false;
		}

		eventType = gsEventType;
		return true;
	}

	#endregion Abe's hacky shit
}

#nullable restore