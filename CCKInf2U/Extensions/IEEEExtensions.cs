using CCKInf2U.Interop;
using System;
using System.Runtime.CompilerServices;

namespace CCKInf2U.Extensions;

[SkipLocalsInit]
internal static class IEEEExtensions
{
	/*
	 * Below examples use the number `+7.07`
	 * 
	 * SmallDouble spec in GemStone
	 * [    E   ] [                         M                          ]  [S] [ T ]
	 *  10000001   1100010001111010111000010100011110101110000101001000    0   110
	 * 
	 * Part				; Position (inclusive)	; Values
	 * Exponent			; 56-63					; Any
	 * Mantissa			; 4-55					; Any
	 * Sign				; 3						; 0 +ve, 1 -ve
	 * Type				; 0-2					; Always 110
	 * Padding			; (Used below)			; 0
	 *
	 * SmallDouble spec mapped to IEEE double spec
	 * SEPPPEEE EEEEMMMM MMMMMMMM MMMMMMMM MMMMMMMM MMMMMMMM MMMMMMMM MMMMMMMM
	 * 01000000 00011100 01000111 10101110 00010100 01111010 11100001 01001000
	 */

	public static Oop? ToGemStoneOop(this double @double)
	{
		if (!double.IsNormal(@double) || @double is < (double)float.MinValue or > (double)float.MaxValue)
		{
			// Double is (roughly) outside the range representable by GemStone's SmallDouble.

			/*
			 * In more detail, below is a quote from the doccomment on `GciFltToOop`
			 * in 'gci.hf' from GemBuilder for C 3.6.
			 * 
			 * ---
			 * Thus SmallDouble's can represent  C double that have value zero or that have exponent bits
			 * in range 0x381 to 0x3ff, which corresponds to about 5.0e-39 to 6.0e+38,
			 * which is also the range of  C 4 - byte float.
			 * ---
			 * 
			 * float.h specifies a range of at least 1.0e[+/-]37, so the C float part checks out.
			 * 
			 * All this said, we'll use the bounds of a C# float instead of extracting and testing the exponent.
			 * Finance doesn't deal with the extremes of the range anyway.
			 */

			return null;
		}

		return DoubleToGemStoneOop(@double);
	}

	private static Oop DoubleToGemStoneOop(double @double)
	{
		// SEPPPEEEEEEEMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM - BEFORE
		// EEEEEEEEMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMSTTT - AFTER

		var bits = BitConverter.DoubleToUInt64Bits(@double);

		const ulong SignMask = 0x80_00_00_00_00_00_00_00UL;
		const ulong MantissaMask = 0x00_0F_FF_FF_FF_FF_FF_FFUL;
		const ulong ExponentReducer = 896UL << 52;
		const ulong ExponentMask = 0x7F_F0_00_00_00_00_00_00UL;

		// Type
		return GciOop.OOP_TAG_SMALLDOUBLE
			// Sign
			| ((bits & SignMask) >> 60)
			// Mantissa
			| ((bits & MantissaMask) << 4)
			// Exponent
			| (((bits - ExponentReducer) & ExponentMask) << 4);
	}

	public static Oop ToGemStoneOop(this int @int)
	{
		return (((Oop)@int) << 3) | GciOop.OOP_TAG_SMALLINT;
	}

	public static Oop ToGemStoneOop(this short @short)
	{
		return (((Oop)@short) << 3) | GciOop.OOP_TAG_SMALLINT;
	}

	public static Oop? ToGemStoneOop(this long @long)
	{
		if (@long is < 0x8F_FF_FF_FFL or > 1_152_921_504_606_846_975L)
		{
			// Numbers smaller than -2^60 cannot be directly translated to a
			// SmallInteger Oop, therefore we need to ask Gci to make it for us

			// Numbers large than 2^60-1 cannot be directly translated to a
			// SmallInteger Oop, therefore we need to ask Gci to make it for us

			return null;
		}

		return (((Oop)@long) << 3) | GciOop.OOP_TAG_SMALLINT;
	}

	public static double SmallDoubleOopToDouble(this Oop oop)
	{
		// EEEEEEEEMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMSTTT - BEFORE
		// SEPPPEEEEEEEMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM - AFTER

		const ulong SignMask = 0x00_00_00_00_00_00_00_08UL;
		const ulong MantissaMask = 0x00_FF_FF_FF_FF_FF_FF_F0UL;
		const ulong ExponentExtender = 896UL << 52;
		const ulong ExponentMask = 0x7F_F0_00_00_00_00_00_00UL; // Post manipulation mask

		// Mantissa
		var ieee = ((oop & MantissaMask) >> 4)
			// Exponent
			| (((oop >> 4) + ExponentExtender) & ExponentMask)
			// Sign
			| ((oop & SignMask) << 60);

		return BitConverter.UInt64BitsToDouble(ieee);
	}

	public static GemStoneValue<object> SmallIntegerOopToBinaryNumberValue(this Oop oop)
	{
		var rawVal = ((long)oop) >> 3;

		// 15   14   13   12   11   10   9    8    7    6    5    4    3    2    1    0
		// S000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0TTT
		// T.F. We have the range of 2^-60 to 2^60-1 - 1

		if (rawVal is <= (long)int.MaxValue and >= (long)int.MinValue)
		{
			return new(DotNetValueType.Integer, (int)rawVal);
		}
		else
		{
			return new(DotNetValueType.Long, rawVal);
		}
	}
}
