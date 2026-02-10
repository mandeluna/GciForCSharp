using CCKInf2U.Extensions;
using CCKInf2U.Interop;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace CCKInf2U;

public sealed class GemStoneObject
{
	public Oop Oop { get; private set; }

	public object Value { get; private set; }

	private VariantType? _vbType;
	private readonly GemStoneSession _session;

	private GemStoneObject()
	{
		// Hide default constructor
	}

	private GemStoneObject(GemStoneSession session)
	{
		_session = session;
	}

	internal GemStoneObject(GemStoneSession session, Oop oop)
	{
		_session = session;
		SetOopAndClearType(oop);
	}

	public bool IsNil()
	{
		return Oop == KnownOops.OPS_NIL;
	}

	public bool IsTrue()
	{
		return Oop == KnownOops.OPS_TRUE;
	}

	public bool IsFalse()
	{
		return Oop == KnownOops.OPS_FALSE;
	}

	public bool IsCollection()
	{
		return RT.RTIsKindOfCollection(Oop) != 0;
	}

	public long Size()
	{
		return IsCollection() ? RT.RTCollectionSize(Oop) : 0L;
	}

	#region To refactor out

	public void SetOopAndClearType(Oop oop)
	{
		Oop = oop;
		_vbType = null;
	}

	internal void SetValueAndType(object value, VariantType type)
	{
		Value = value;
		_vbType = type;
	}

	#endregion To refactor out

	#region As...

	public string AsString(ref bool isWrongType)
	{
		var valueWrapper = FetchValue(Oop);

		if (valueWrapper._vbType != VariantType.String)
		{
			isWrongType = true;
			return string.Empty;
		}

		isWrongType = false;
		return (string)valueWrapper.Value;
	}

	public object AsLocal(ref bool isNotPresent)
	{
		var valueWrapper = FetchValue(Oop);

		if (!valueWrapper._vbType.HasValue)
		{
			isNotPresent = true;
			return null;
		}

		isNotPresent = false;
		return valueWrapper.Value;
	}

	public int AsInteger(ref bool isWrongType)
	{
		var valueWrapper = FetchValue(Oop);

		if (valueWrapper._vbType != VariantType.Integer)
		{
			isWrongType = true;
			return default;
		}

		isWrongType = false;
		return (int)valueWrapper.Value;
	}

	public DateTime AsDate(ref bool isWrongType)
	{
		var valueWrapper = FetchValue(Oop);

		if (valueWrapper._vbType != VariantType.Date)
		{
			isWrongType = true;
			return default;
		}

		isWrongType = false;
		return (DateTime)valueWrapper.Value;
	}

	public object[] AsArray(ref bool isWrongType)
	{
		var valueWrapper = FetchValue(Oop);

		if (valueWrapper._vbType != VariantType.Array)
		{
			isWrongType = true;
			return null;
		}

		isWrongType = false;
		return (object[])valueWrapper.Value;
	}

	public object[] AsCompressedArray(ref bool isWrongType)
	{
		var valueWrapper = FetchCompressedValue(Oop);

		if (valueWrapper._vbType != VariantType.Array)
		{
			isWrongType = true;
			return null;
		}

		isWrongType = false;
		return (object[])valueWrapper.Value;
	}

	#endregion #region As...

	#region Fetch...

	private GemStoneObject FetchCompressedValue(Oop oop)
	{
		if (RT.RTIsSystemClass(oop) != 0)
		{
			GemStoneObject retVal = new(_session);
			retVal.SetOopAndClearType(oop);
			return retVal;
		}

		Span<byte> classBuffer = stackalloc byte[255];
		var classBytesWritten = RT.RTForeignClassName(oop, classBuffer);
		ReadOnlySpan<byte> className = classBuffer[..classBytesWritten];

		if (className.SequenceEqual("String"u8)
			|| className.SequenceEqual("DoubleByteString"u8)
			|| className.SequenceEqual("DoubleByteSymbol"u8))
		{
			var stringBufferSize = RT.RTGetObjectSize(oop);

			var stringBuffer = new byte[stringBufferSize];

			var stringBytesWritten = RT.RTFromForeignStringW(oop, stringBuffer.AsSpan(), stringBufferSize);
			if (stringBytesWritten == 0)
			{
				stringBuffer = Array.Empty<byte>();
			}

			GemStoneObject retVal = new(_session);
			retVal.SetValueAndType(stringBuffer, VariantType.Array);
			return retVal;
		}

		return FetchValue(oop);
	}

	private void FetchCollectionValue(Oop oop, out GemStoneObject[] objectCollection)
	{
		var isError = default(bool);

		var collectionOop = RT.RTForeignPerform0(oop, "asOrderedCollection"u8);
		_ = _session.CheckError(ref isError);

		var size = RT.RTCollectionSize(collectionOop);
		_ = _session.CheckError(ref isError);

		objectCollection = new GemStoneObject[(int)(size + 1)];
		var collectionSpan = objectCollection.AsSpan();

		for (var ii = 1 ; ii < collectionSpan.Length ; ii++)
		{
			collectionSpan[ii] = FetchValue(RT.RTFetchIndexedVariable(collectionOop, ii));
		}
	}


	private GemStoneObject FetchValue(Oop obj)
	{

		// Opp inspection
		var checkSpecial = (ulong)obj & 0x7UL;
		GemStoneObject gemObject = new GemStoneObject(_session, obj);
		switch (checkSpecial)
		{
			case 02:
				gemObject.SetSmallIntegerFromOop();
				break;
			case 06:
				gemObject.SetSmallDoubleFromOop();
				break;
			default:
				return FetchValueFromGS(obj);
		}

		return gemObject;
	}

	private GemStoneObject FetchValueFromGS(Oop oop)
	{
		if (RT.RTIsSystemClass(oop) != 0)
		{
			GemStoneObject retVal = new(_session);
			retVal.SetOopAndClearType(oop);
			return retVal;
		}
		else
		{
			var classBuffer = ArrayPool<byte>.Shared.Rent(minimumLength: 255);
			var classBufferSpan = classBuffer.AsSpan();
			var classNameSize = RT.RTForeignClassName(oop, classBufferSpan);
			Span<byte> persistedBytes = stackalloc byte[classNameSize];
			classBufferSpan[..classNameSize].CopyTo(persistedBytes);
			ReadOnlySpan<byte> className = persistedBytes;
			ArrayPool<byte>.Shared.Return(classBuffer);

			// TODO(AB): Prove order by query frequency more granuarly.
			if (className.SequenceEqual("String"u8))
			{
				// +1 to account for null term
				var bufferSize = 1 + RT.RTGetObjectSize(oop);

				var buffer = ArrayPool<byte>.Shared.Rent(minimumLength: bufferSize);
				var bufferSpan = buffer.AsSpan();
				var size = RT.RTFromForeignString(oop, bufferSpan, bufferSize);
				var utf8String = bufferSpan[..size].DecodeUTF8();
				ArrayPool<byte>.Shared.Return(buffer);

				GemStoneObject retVal = new(_session);
				retVal.SetValueAndType(utf8String, VariantType.String);
				return retVal;
			}
			else if (className.SequenceEqual("LargeInteger"u8))
			{
				// TODO(CCK-3328): Check if Integer,LargePositiveInteger,LargeNegativeInteger are ever seen.
				GemStoneObject retVal = new(_session);
				retVal.SetValueAndType(RT.RTFromForeignInteger(oop), VariantType.Integer);
				return retVal;
			}
			else if (className.SequenceEqual("Symbol"u8))
			{
				var buffer = ArrayPool<byte>.Shared.Rent(minimumLength: 255);
				var bufferSpan = buffer.AsSpan();
				var size = RT.RTFromForeignString(oop, bufferSpan, 255);
				var utf8Symbol = bufferSpan[..size].DecodeUTF8();
				ArrayPool<byte>.Shared.Return(buffer);

				GemStoneObject retVal = new(_session);
				retVal.SetValueAndType(utf8Symbol, VariantType.String);
				return retVal;
			}
			else if (className.SequenceEqual("SYSDate"u8))
			{
				GemStoneObject retVal = new(_session);
				retVal.SetValueAndType(FetchLocalDate(), VariantType.Date);
				return retVal;
			}
			else if (className.SequenceEqual("Float"u8)
				|| className.SequenceEqual("BinaryFloat"u8)
				|| className.SequenceEqual("SmallFloat"u8)
				)
			{
				// TODO(CCK-3328): Check Float,BinaryFloat,SmallFloat are even seen.
				GemStoneObject retVal = new(_session);
				retVal.SetValueAndType(RT.RTFromForeignFloat(oop), VariantType.Double);
				return retVal;
			}
			else if (className.SequenceEqual("DoubleByteString"u8)
				|| className.SequenceEqual("DoubleByteSymbol"u8)
				|| className.SequenceEqual("Unicode7"u8)
				|| className.SequenceEqual("Unicode16"u8))
			{
				// TODO(CCK-3328): Check if DoubleByteSymbol,Unicode7,Unicode16 are ever seen.
				var bufferSize = RT.RTGetObjectSize(oop);

				byte[] rentedBuffer = null;
				scoped Span<byte> bytes;
				if (bufferSize > 256)
				{
					rentedBuffer = ArrayPool<byte>.Shared.Rent(bufferSize);
					bytes = rentedBuffer.AsSpan();
				}
				else
				{
					bytes = stackalloc byte[bufferSize];
				}

				var bytesWritten = RT.RTFromForeignStringW(oop, bytes, bufferSize);
				string decodedString;
				if (bytesWritten != 0)
				{
					if (className.SequenceEqual("Unicode7"u8))
					{
#pragma warning disable SYSLIB0001 // Type or member is obsolete - UTF-7 bad, we know.
						decodedString = UTF7Encoding.UTF7.GetString(bytes);
#pragma warning restore SYSLIB0001 // Type or member is obsolete - UTF-7 bad, we know.
					}
					else
					{
						decodedString = UnicodeEncoding.Unicode.GetString(bytes);
					}
				}
				else
				{
					decodedString = string.Empty;
				}

				if (rentedBuffer is not null)
				{
					ArrayPool<byte>.Shared.Return(rentedBuffer);
				}

				GemStoneObject retVal = new(_session);
				retVal.SetValueAndType(decodedString, VariantType.String);
				return retVal;
			}
			else if (className.SequenceEqual("Boolean"u8))
			{
				// TODO(TG for AB): This should be here, doesn't handle bool currently
				// TODO(AB): ^that, use the new checking for True/False.
				var isTrue = oop == KnownOops.OPS_TRUE;
				GemStoneObject retVal = new(_session);
				retVal.SetValueAndType(isTrue, VariantType.Boolean);
				return retVal;
			}
			else if (className.SequenceEqual("FixedPoint"u8)
				|| className.SequenceEqual("ScaledDecimal"u8)
				|| className.SequenceEqual("ScaledDecimalAndUnroundedValue"u8))
			{
				// TODO(CCK-3328): Check if ScaledDecimal,ScaledDecimalAndUnroundedValue are ever seen.
				var numberAsStringOop = RT.RTForeignPerform0(oop, "asString"u8);

				// +1 to account for null term
				var bufferSize = 1 + RT.RTGetObjectSize(numberAsStringOop);

				var buffer = ArrayPool<byte>.Shared.Rent(minimumLength: bufferSize);
				var bufferSpan = buffer.AsSpan();
				var size = RT.RTFromForeignString(numberAsStringOop, bufferSpan, bufferSize);
				var decodedNumberString = bufferSpan[..size].DecodeUTF8();
				ArrayPool<byte>.Shared.Return(buffer);

				// NOTE(AB): Mimicing the behaviour of Information.IsNumeric check + Conversions.ToDouble parsing.
				// Should probably be revised to drop some options (EG. exponent handling) - but keeping it consistent
				// for now.
				if (!double.TryParse(
					decodedNumberString,
					NumberStyles.AllowDecimalPoint
					| NumberStyles.AllowExponent
					| NumberStyles.AllowLeadingSign
					| NumberStyles.AllowLeadingWhite
					| NumberStyles.AllowThousands
					| NumberStyles.AllowTrailingSign
					| NumberStyles.AllowParentheses
					| NumberStyles.AllowTrailingWhite
					| NumberStyles.AllowCurrencySymbol,
					null,
					out var value))
				{
					value = 0D;
				}

				GemStoneObject retVal = new(_session);
				retVal.SetValueAndType(value, VariantType.Double);

				var isError = false;
				_ = _session.CheckError(ref isError);
				if (Debugger.IsAttached && isError)
				{
					Debugger.Break();
				}

				return retVal;
			}
			else if (className.SequenceEqual("Fraction"u8))
			{
				// TODO(CCK-3328): Check if Fraction is ever seen.
				var numberAsFloatOop = RT.RTForeignPerform0(oop, "asFloat"u8);

				GemStoneObject retVal = new(_session);
				retVal.SetValueAndType(RT.RTFromForeignFloat(numberAsFloatOop), VariantType.Double);

				var isError = false;
				_ = _session.CheckError(ref isError);
				if (Debugger.IsAttached && isError)
				{
					Debugger.Break();
				}

				return retVal;
			}
			else
			{
				GemStoneObject retVal = new(_session);

				if (RT.RTIsKindOfCollection(oop) != 0)
				{
					FetchCollectionValue(oop, out var collection);
					retVal.SetValueAndType(collection, VariantType.Array);
				}
				else
				{
					retVal.SetOopAndClearType(oop);
				}

				return retVal;
			}
		}
	}

	private DateTime FetchLocalDate()
	{
		var typeMismatch = default(bool);

		var day = ForeignPerform("day"u8).AsInteger(ref typeMismatch);
		var month = ForeignPerform("month"u8).AsInteger(ref typeMismatch);
		var year = ForeignPerform("year"u8).AsInteger(ref typeMismatch);

		return new(year, month, day);
	}

	#endregion Fetch...

	#region Foreign Perform

	public GemStoneObject ForeignPerform(byte[] selector)
	{
		return ForeignPerform(selector.AsSpan());
	}

	public GemStoneObject ForeignPerform(ReadOnlySpan<byte> selector)
	{
		var oop = RT.RTForeignPerform0(Oop, selector);

		var isError = default(bool);
		_ = _session.CheckError(ref isError);

		return oop != Oop ? new(_session, oop) : this;
	}

	public GemStoneObject ForeignPerformWithArgs(byte[] selector, params object[] arguments)
	{
		return ForeignPerformWithArgs(selector.AsSpan(), arguments);
	}

	public GemStoneObject ForeignPerformWithArgs(ReadOnlySpan<byte> selector, params object[] arguments)
	{
		const int MaximumNumberOfArguments = 12;

		if (arguments.Length > MaximumNumberOfArguments)
		{
			return null;
		}

		Span<Oop> args = stackalloc Oop[arguments.Length + 1];

		for (var ii = 0 ; ii < arguments.Length ; ++ii)
		{
			args[ii] = ConvertArgument(arguments[ii]);
		}

		var oop = arguments.Length switch
		{
			1 => RT.RTForeignPerform1(Oop, selector, args[0]),
			2 => RT.RTForeignPerform2(Oop, selector, args[0], args[1]),
			3 => RT.RTForeignPerform3(Oop, selector, args[0], args[1], args[2]),
			4 => RT.RTForeignPerform4(Oop, selector, args[0], args[1], args[2], args[3]),
			5 => RT.RTForeignPerform5(Oop, selector, args[0], args[1], args[2], args[3], args[4]),
			6 => RT.RTForeignPerform6(Oop, selector, args[0], args[1], args[2], args[3], args[4], args[5]),
			7 => RT.RTForeignPerform7(Oop, selector, args[0], args[1], args[2], args[3], args[4], args[5], args[6]),
			8 => RT.RTForeignPerform8(Oop, selector, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]),
			_ => RT.RTForeignPerformWithArguments(Oop, selector, args, args.Length - 1),
		};

		return oop != Oop ? new(_session, oop) : this;
	}

	private Oop ConvertArgument(object argument)
	{
		try
		{
			Oop argumentOop;

			switch (Information.VarType(argument))
			{
				case VariantType.Object:
					if (argument is null)
					{
						argumentOop = KnownOops.OPS_NIL;
					}
					else
					{
						var @object = Unsafe.As<object, GemStoneObject>(ref argument);
						argumentOop = @object.Oop;
					}
					break;

				case VariantType.Boolean:
					argumentOop = Unsafe.Unbox<bool>(argument) ? KnownOops.OPS_TRUE : KnownOops.OPS_FALSE;
					break;

				case VariantType.Short:
					argumentOop = Unsafe.Unbox<short>(argument).ToGemStoneOop();
					break;

				case VariantType.Integer:
					argumentOop = Unsafe.Unbox<int>(argument).ToGemStoneOop();
					break;

				case VariantType.Long:
					long @long = Unsafe.Unbox<long>(argument);
					var longOop = (@long).ToGemStoneOop();

					if (longOop is null)
					{
						argumentOop = RT.RTNewInteger(@long);
					}
					else
					{
						argumentOop = (Oop)longOop;
					}
					break;

				case VariantType.UserDefinedType:
					// Only UDT in the system is Oop
					argumentOop = RT.RTNewInteger(Unsafe.As<Oop, long>(ref Unsafe.Unbox<Oop>(argument)));
					break;

				case VariantType.Double:
				case VariantType.Single:
					if (5E-38 <= Math.Abs((double)(object)argument) && Math.Abs((double)(object)argument) <= 6E38)
					{
						argumentOop = ((double)(object)argument).ToGemStoneOop();
					}
					else
					{
						argumentOop = RT.RTNewFloat(Conversions.ToDouble(argument));
					}

					break;

				case VariantType.String:
					scoped Span<byte> unicodeBytes;

					if (argument is not null)
					{
						var stringArgument = Unsafe.As<object, string>(ref argument).AsSpan();
						var trimmedArgument = stringArgument.TrimEnd();
						var unicodeByteCount = UnicodeEncoding.Unicode.GetByteCount(trimmedArgument);
						// TODO(AB): Should be testing size here, make sure we're not stackallocing a massive string
						unicodeBytes = stackalloc byte[unicodeByteCount];
						UnicodeEncoding.Unicode.GetBytes(trimmedArgument, unicodeBytes);
					}
					else
					{
						unicodeBytes = Span<byte>.Empty;
					}

					argumentOop = RT.RTNewStringW((ReadOnlySpan<byte>)unicodeBytes, unicodeBytes.Length);
					break;

				case VariantType.Date:
					var dateArgument = Unsafe.Unbox<DateTime>(argument);
					argumentOop = RT.RTNewSYSDate(dateArgument.Day, dateArgument.Month, dateArgument.Year);
					break;

				case (VariantType.Array | VariantType.Char):

					// TODO(AB): Likely not necessary.
					// this is to support single byte strings, which are required for the user name and password for example
					// To activate this code, the method making the call should pass the parameter obj as a CHAR ARRAY
					// poUser.sUserName.Trim.ToCharArray

					var charArrayArgument = Unsafe.As<object, char[]>(ref argument);
					ReadOnlySpan<char> trimmedCharArrayArgument = charArrayArgument.AsSpan().TrimEnd();
					var utf8ByteCount = UTF8Encoding.UTF8.GetByteCount(trimmedCharArrayArgument);
					// Add null terminator
					Span<byte> terminatedCharArrayArgument = stackalloc byte[utf8ByteCount + 1];
					UTF8Encoding.UTF8.GetBytes(trimmedCharArrayArgument, terminatedCharArrayArgument);

					argumentOop = RT.RTNewString(terminatedCharArrayArgument);
					break;

				default:
					Debugger.Break();
					argumentOop = 0UL;
					break;
			}

			var isError = default(bool);
			_ = _session.CheckError(ref isError);
			return argumentOop;
		}
		catch
		{
			return 0UL;
		}
	}

	#endregion Foreign Perform

	#region Foreign Perform Plus

	public string PrintString()
	{
		var valueWrapper = FetchValue(Oop);

		if (!valueWrapper._vbType.HasValue)
		{
			var typeMismatch = default(bool);
			return valueWrapper.ForeignPerform("printString"u8).AsString(ref typeMismatch);
		}
		else if (valueWrapper._vbType == VariantType.Array)
		{
			if (Debugger.IsAttached)
			{
				// TODO(AB): See if this is ever hit and what side effects it has..... Because wtf.
				Debugger.Break();
			}
			return "a Collection";
		}
		else
		{
			return Conversions.ToString(valueWrapper.Value);
		}
	}

	public GemStoneObject Item(int index)
	{
		return FetchValue(RT.RTFetchIndexedVariable(Oop, index));
	}

	public string DealerRiskName()
	{
		return ForeignPerform("dealerRiskName"u8).PrintString();
	}

	#endregion Foreign Perform Plus
}