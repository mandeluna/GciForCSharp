using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CCKInf2U.Interop;

internal static unsafe partial class RT
{
	/// <summary>
	/// Full name of the "RT" wrapper DLL, abstracting GemBuilder For C.
	/// </summary>
	private const string RTWrapper = "rt.dll";

	/*
	 * NOTE(AB):
	 * 
	 * As we're running on windows for the forseeable future, LLP64 is assumed.
	 * (Reference: https://en.cppreference.com/w/c/language/arithmetic_types?fbclid=IwAR37OYY-uTsQMd5Y_utQD6xKJ9WAOx7yvSf3oCjV1M1qos0FWYbf2IKzZ1A)
	 * 
	 * Effectively, C++ `long`'s are narrowed to C# ints (32 bit)
	 */

	// TODO(AB): Move all string interop to char[] / Span<char> (after confirming blittability)

	#region Unused

	[LibraryImport(libraryName: RTWrapper)]
	[UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvSuppressGCTransition) })]
	public static partial Oop RTGetOPS_NIL();

	[LibraryImport(libraryName: RTWrapper)]
	[UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvSuppressGCTransition) })]
	public static partial Oop RTGetOPS_TRUE();

	[LibraryImport(libraryName: RTWrapper)]
	[UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvSuppressGCTransition) })]
	public static partial Oop RTGetOPS_FALSE();

	#endregion Unused

	[LibraryImport(libraryName: RTWrapper)]
	public static partial int RTLogin(
		ReadOnlySpan<byte> host,
		ReadOnlySpan<byte> gemServer,
		ReadOnlySpan<byte> netldi,
		ReadOnlySpan<byte> userId,
		ReadOnlySpan<byte> passw,
		ReadOnlySpan<byte> hostUserId,
		ReadOnlySpan<byte> hostPassw);

	[LibraryImport(libraryName: RTWrapper)]
	public static partial void RTLogout();

	[LibraryImport(libraryName: RTWrapper)]
	public static partial int RTInit();

	[LibraryImport(libraryName: RTWrapper)]
	public static partial Oop RTExecute(ReadOnlySpan<byte> expr);

	[LibraryImport(libraryName: RTWrapper)]
	public static partial Oop RTGlobalNamed(ReadOnlySpan<byte> id);

	[LibraryImport(libraryName: RTWrapper)]
	public static partial Oop RTContinue(Oop process);

	[LibraryImport(libraryName: RTWrapper)]
	public static partial Oop RTNewSYSDate(int day, int month, int year);

	[LibraryImport(libraryName: RTWrapper)]
	public static partial int RTCommit();

	[LibraryImport(libraryName: RTWrapper)]
	public static partial void RTBegin();

	[LibraryImport(libraryName: RTWrapper)]
	public static partial void RTAbort();

	[LibraryImport(libraryName: RTWrapper)]
	public static partial void RTEnableSignaledErrors();

	[LibraryImport(libraryName: RTWrapper)]
	public static partial void RTDisableSignaledErrors();

	[LibraryImport(libraryName: RTWrapper)]
	public static partial int RTPollForSignal();

	// NOTE(AB): Return value C++ `long` in RT 3.6.5
	[LibraryImport(libraryName: RTWrapper)]
	public static partial int RTGetObjectSize(Oop obj);

	[LibraryImport(libraryName: RTWrapper)]
	public static partial Oop RTForeignPerformWithArguments(Oop obj, ReadOnlySpan<byte> selector, ReadOnlySpan<Oop> args, int num_args);

	[LibraryImport(libraryName: RTWrapper)]
	public static partial Oop RTForeignPerform0(Oop obj, ReadOnlySpan<byte> selector);

	[LibraryImport(libraryName: RTWrapper)]
	public static partial Oop RTForeignPerform1(Oop obj, ReadOnlySpan<byte> selector, Oop arg1);

	[LibraryImport(libraryName: RTWrapper)]
	public static partial Oop RTForeignPerform2(Oop obj, ReadOnlySpan<byte> selector, Oop arg1, Oop arg2);

	[LibraryImport(libraryName: RTWrapper)]
	public static partial Oop RTForeignPerform3(Oop obj, ReadOnlySpan<byte> selector, Oop arg1, Oop arg2, Oop arg3);

	[LibraryImport(libraryName: RTWrapper)]
	public static partial Oop RTForeignPerform4(Oop obj, ReadOnlySpan<byte> selector, Oop arg1, Oop arg2, Oop arg3, Oop arg4);

	[LibraryImport(libraryName: RTWrapper)]
	public static partial Oop RTForeignPerform5(Oop obj, ReadOnlySpan<byte> selector, Oop arg1, Oop arg2, Oop arg3, Oop arg4, Oop arg5);

	[LibraryImport(libraryName: RTWrapper)]
	public static partial Oop RTForeignPerform6(Oop obj, ReadOnlySpan<byte> selector, Oop arg1, Oop arg2, Oop arg3, Oop arg4, Oop arg5, Oop arg6);

	[LibraryImport(libraryName: RTWrapper)]
	public static partial Oop RTForeignPerform7(Oop obj, ReadOnlySpan<byte> selector, Oop arg1, Oop arg2, Oop arg3, Oop arg4, Oop arg5, Oop arg6, Oop arg7);

	[LibraryImport(libraryName: RTWrapper)]
	public static partial Oop RTForeignPerform8(Oop obj, ReadOnlySpan<byte> selector, Oop arg1, Oop arg2, Oop arg3, Oop arg4, Oop arg5, Oop arg6, Oop arg7, Oop arg8);

	[LibraryImport(libraryName: RTWrapper)]
	public static partial Oop RTNewString(ReadOnlySpan<byte> s);

	[LibraryImport(libraryName: RTWrapper)]
	public static partial Oop RTNewSymbol(ReadOnlySpan<byte> s);

	[LibraryImport(libraryName: RTWrapper)]
	public static partial Oop RTNewFloat(double d);

	[LibraryImport(libraryName: RTWrapper)]
	public static partial Oop RTNewInteger(long i);

	[LibraryImport(libraryName: RTWrapper)]
	public static partial long RTCollectionSize(Oop coll);

	// NOTE(AB): Param1 index C++ `long` in RT 3.6.5
	[LibraryImport(libraryName: RTWrapper)]
	public static partial Oop RTFetchIndexedVariable(Oop coll, int index);

	[LibraryImport(libraryName: RTWrapper)]
	public static partial int RTIsKernelObject(Oop obj);

	[LibraryImport(libraryName: RTWrapper)]
	public static partial int RTIsKindOfCollection(Oop obj);

	[LibraryImport(libraryName: RTWrapper)]
	public static partial double RTFromForeignFloat(Oop obj);

	// NOTE(AB): Return value C++ `long` in RT 3.6.5
	[LibraryImport(libraryName: RTWrapper)]
	public static partial long RTFromForeignInteger(Oop obj);

	[LibraryImport(libraryName: RTWrapper)]
	public static partial int RTIsSystemClass(Oop obj);

	[LibraryImport(libraryName: RTWrapper)]
	public static partial void RTReleaseObject(Oop obj);

	// NOTE(AB): Return value C++ `long` in RT 3.6.5
	// NOTE(AB): Param2 C++ `long` in RT 3.6.5
	[LibraryImport(libraryName: RTWrapper)]
	public static partial int RTFromForeignStringW(Oop obj, Span<byte> result, int bufsize);

	// NOTE(AB): Param1 C++ `long` in RT 3.6.5
	[LibraryImport(libraryName: RTWrapper)]
	public static partial Oop RTNewStringW(ReadOnlySpan<byte> s, long bufsize);

	// NOTE(AB): Param1 C++ `long` in RT 3.6.5
	// NOTE(AB): Param5 C++ `long` in RT 3.6.5
	[LibraryImport(libraryName: RTWrapper)]
	public static partial int RTError(
		ref Oop category,
		ref int number,
		ref Oop context,
		Span<byte> message,
		Span<Oop> args,
		ref int argCount);

	[LibraryImport(libraryName: RTWrapper)]
	public static partial int RTForeignClassName(Oop obj, Span<byte> result);

	// NOTE(AB): Return value C++ `long` in RT 3.6.5
	// NOTE(AB): Param2 C++ `long` in RT 3.6.5
	[LibraryImport(libraryName: RTWrapper)]
	public static partial int RTFromForeignString(Oop obj, Span<byte> result, int bufsize);
}