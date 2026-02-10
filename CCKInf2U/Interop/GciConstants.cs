using System.Runtime.CompilerServices;

namespace CCKInf2U.Interop;

/// <summary>
/// gci.ht
/// </summary>
internal unsafe static class GciConstants
{
#pragma warning disable S1144 // Unused private types or members should be removed - They are used!
	private const int GCI_MAX_ERR_ARGS = 10;
	private const int GCI_ERR_STR_SIZE = 1024;
	private const int GCI_ERR_reasonSize = 1024;
#pragma warning restore S1144 // Unused private types or members should be removed - They are used!

	#region Classes

	/// <summary>
	/// GCI error message.
	/// </summary>
	[SkipLocalsInit]
	public ref struct GciErrSType
	{
		/// <summary>
		/// Error dictionary.
		/// </summary>
		public readonly OopType category;

		/// <summary>
		/// GemStone Smalltalk execution state, a GsProcess.
		/// </summary>
		public readonly OopType context;

		/// <summary>
		/// An instance of AbstractException, or nil; may be nil if error was not signaled from Smalltalk execution.
		/// </summary>
		public readonly OopType exceptionObj;

		/// <summary>
		/// Arguments to error text.
		/// </summary>
		public fixed OopType args[GCI_MAX_ERR_ARGS];

		/// <summary>
		/// GemStone error number.
		/// </summary>
		public readonly int number;

		/// <summary>
		/// Num of arg in the args[].
		/// </summary>
		public readonly int argCount;

		/// <summary>
		/// Nonzero if err is fatal.
		/// </summary>
		public readonly byte fatal;

		/// <summary>
		/// Null-terminated Utf8 error text.
		/// </summary>
		public fixed byte message[GCI_ERR_STR_SIZE + 1];

		/// <summary>
		/// Null-terminated Utf8.
		/// </summary>
		public fixed byte reason[GCI_ERR_reasonSize + 1];

		public GciErrSType()
		{
			// TODO(AB): InlineArray, .NET8, soon(TM)

			category = GciOop.OOP_NIL;
			context = GciOop.OOP_NIL;
			exceptionObj = GciOop.OOP_NIL;

			fixed (OopType* argsBase = args)
			{
				*argsBase = GciOop.OOP_ILLEGAL;
			}

			number = 0;
			argCount = 0;
			fatal = 0;

			fixed (byte* messageBase = message)
			{
				*messageBase = 0;
			}

			fixed (byte* reasonBase = reason)
			{
				*reasonBase = 0;
			}
		}
	}

	#endregion Classes

	#region Enums

	[System.Flags]
#pragma warning disable S1939, S2344 // Flags suffix matches header definition, underlying type for clarity
	public enum GciFetchUtf8Flags : int
#pragma warning restore S1939, S2344 // Flags suffix matches header definition, underlying type for clarity
	{
#pragma warning disable S2346 // Flags enumerations zero-value members should be named "None" - Matching interop.
		GCI_UTF8_FetchNormal = 0,
#pragma warning restore S2346 // Flags enumerations zero-value members should be named "None" - Matching interop.

		/// <summary>
		/// Substitute '.' for illegal codepoints.
		/// </summary>
		GCI_UTF8_FilterIllegalCodePoints = 0x1,

		/// <summary>
		/// Return message instead of signalling Exception for illegal codepoints.
		/// </summary>
		GCI_UTF8_NoError = 0x2,
	}

	#endregion Enums
}
