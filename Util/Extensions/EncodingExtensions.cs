using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Util.Extensions;

public static class EncodingExtensions
{
	/// <summary>
	/// Decodes UTF-8 encoded bytes into a string.
	/// </summary>
	/// <param name="bytes">The UTF-8 encoded bytes.</param>
	/// <returns>A string that contains the decoded bytes.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string DecodeUTF8(this byte[] bytes)
	{
		return Encoding.UTF8.GetString(bytes.AsSpan());
	}

	/// <summary>
	/// Decodes UTF-8 encoded bytes into a string.
	/// </summary>
	/// <param name="bytes">The UTF-8 encoded bytes.</param>
	/// <returns>A string that contains the decoded bytes.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string DecodeUTF8(this Span<byte> bytes)
	{
		return Encoding.UTF8.GetString(bytes);
	}

	/// <summary>
	/// Decodes UTF-8 encoded bytes into a string.
	/// </summary>
	/// <param name="bytes">The UTF-8 encoded bytes.</param>
	/// <returns>A string that contains the decoded bytes.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string DecodeUTF8(this ReadOnlySpan<byte> bytes)
	{
		return Encoding.UTF8.GetString(bytes);
	}
}
