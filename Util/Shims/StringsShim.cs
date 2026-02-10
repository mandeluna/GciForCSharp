using System.Runtime.CompilerServices;

namespace Util.Shims;

/// <summary>
/// Replacement for calls to <see cref="Microsoft.VisualBasic.Strings"/>.
/// </summary>
[SkipLocalsInit]
public static class StringsShim
{
	private static readonly char[] _trimSpaces = [ ' ', '\u3000', ];

	/// <summary>
	/// Strips leading and trailing <b>spaces</b> (emphasis: not all whitespace) and coalesces
	/// <see langword="null"/> to empty string.
	/// </summary>
	/// <remarks>
	/// Replaces <see cref="Microsoft.VisualBasic.Strings.Trim(string?)"/>.
	/// </remarks>
	/// <param name="str">The string to trim.</param>
	/// <returns>The trimmed string if not <see langword="null"/>, otherwise empty string.</returns>
	public static string VbTrim(this string? str)
	{
		return !string.IsNullOrEmpty(str) ? str.Trim(_trimSpaces) : "";
	}
}
