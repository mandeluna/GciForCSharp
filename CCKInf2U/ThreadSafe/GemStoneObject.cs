using CCKInf2U.Extensions;
using CCKInf2U.Interop;
using CCKInf2U.ThreadSafe;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using CommunityToolkit.Diagnostics;

namespace CCKInf2U;

[SkipLocalsInit]
public sealed class GemStoneObject
{
	#region New API

	// TODO

	#endregion New API

	#region Legacy API

	public Oop Oop { get; }
	public object Value { get; }

	public bool HasActiveSession => _session.IsActiveSession;

	private readonly GemStoneSession _session;

	internal GemStoneObject(GemStoneSession session, Oop oop)
	{
		_session = session;
		Oop = oop;
	}

	public static GemStoneObject NullOop(GemStoneSession session)
	{
		return new GemStoneObject(session, GciOop.OOP_NIL);
	}

	private GemStoneObject(GemStoneSession session, object value)
	{
		// TODO(AB): This usage is an anti-pattern... Just don't have the time right now.
		// Getting values out of a GSO should be handled explicitly, not randomly returning ones
		// that already have the value retrieved.
		_session = session;
		Value = value;
	}

	public bool IsNil()
	{
		return Oop == GciOop.OOP_NIL;
	}

	public bool IsTrue()
	{
		return Oop == GciOop.OOP_TRUE;
	}

	public bool IsFalse()
	{
		return Oop == GciOop.OOP_FALSE;
	}

	public bool IsCollection()
	{
		return FFI.IsKindOfClass(_session, Oop, GciOop.OOP_CLASS_COLLECTION);
	}

	public long Size()
	{
		if (!IsCollection())
		{
			return 0L;
		}

		return FFI.GetObjectSize(_session, Oop);
	}

	#region As...

	public string AsString(ref bool isWrongType)
	{
		var valueWrapper = FetchValue(Oop);

		if (valueWrapper.Type != DotNetValueType.String)
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

		if (valueWrapper.Type is DotNetValueType.None or DotNetValueType.Oop)
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

		if (valueWrapper.Type != DotNetValueType.Integer)
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

		if (valueWrapper.Type != DotNetValueType.Date)
		{
			isWrongType = true;
			return default;
		}

		isWrongType = false;
		return (DateTime)valueWrapper.Value;
	}

	public GemStoneObject[] AsArray(ref bool isWrongType)
	{
		var valueWrapper = FetchValue(Oop);

		if (valueWrapper.Type != DotNetValueType.Array)
		{
			isWrongType = true;
			return null;
		}

		isWrongType = false;
		return (GemStoneObject[])valueWrapper.Value;
	}

	public byte[] AsCompressedString(ref bool isWrongType)
	{
		var valueWrapper = FetchCompressedString(Oop);

		if (valueWrapper.Type != DotNetValueType.Array)
		{
			isWrongType = true;
			return null;
		}

		isWrongType = false;
		return valueWrapper.Value;
	}

	public ReadOnlyMemory<byte>? AsJsonMemory(ref bool isWrongType)
	{
		var valueWrapper = FetchJsonMemory(Oop);

		if (valueWrapper.Type != DotNetValueType.ReadOnlyMemory)
		{
			isWrongType = true;
			return null;
		}

		isWrongType = false;
		return valueWrapper.Value;
	}

	#endregion #region As...

	#region Fetch Value

	private GemStoneValue<object> FetchValue(Oop oop)
	{
		switch (oop & 0x7UL)
		{
			case GciOop.OOP_TAG_SMALLINT:
				return oop.SmallIntegerOopToBinaryNumberValue();

			case GciOop.OOP_TAG_SMALLDOUBLE:
				return oop == 6
					? new (DotNetValueType.Double, 0)
					: new (DotNetValueType.Double, oop.SmallDoubleOopToDouble());

			case GciOop.OOP_TAG_SPECIAL:
				return oop switch
				{
					GciOop.OOP_NIL => new (DotNetValueType.Oop, GciOop.OOP_NIL),
					GciOop.OOP_TRUE => new (DotNetValueType.Boolean, true),
					GciOop.OOP_FALSE => new (DotNetValueType.Boolean, false),
					// TODO(CCK-3328): Char / JIS Char handling
					_ => new (DotNetValueType.Oop, oop),
				};

			default:
				if (oop == GciOop.OOP_CLASS_SYSTEM)
				{
					return new (DotNetValueType.Oop, GciOop.OOP_CLASS_SYSTEM);
				}

				return FetchValueFromGemStoneWithShortStringOptimisation(oop);
		}
	}

	private GemStoneValue<object> FetchValueFromGemStoneWithShortStringOptimisation(Oop oop)
	{
		Oop classOop;

#pragma warning disable S1199 // Nested code blocks should not be used - Used to scope stackalloc
		{
			Span<byte> buffer = stackalloc byte[64];
			if (FFI.TryGetObjectInfoWithStringBuffer(
				_session,
				oop,
				out var fullObjectInfo,
				buffer,
				out var stringBuffer))
			{
				if (!stringBuffer.IsEmpty)
				{
					return new (DotNetValueType.String, stringBuffer.DecodeUTF8());
				}

				classOop = fullObjectInfo.objClass;
			}
			else
			{
				return new (DotNetValueType.None, GciOop.OOP_NIL);
			}
		}
#pragma warning restore S1199 // Nested code blocks should not be used - Used to scope stackalloc

		return FetchValueFromGemStoneClass(oop, classOop);
	}

	private GemStoneValue<object> FetchValueFromGemStoneClass(Oop oop, Oop classOop)
	{
		if (classOop == GciOop.OOP_CLASS_STRING)
		{
			var bufferSize = (int)FFI.GetObjectSize(_session, oop);
			var buffer = ArrayPool<byte>.Shared.Rent(minimumLength: bufferSize);
			var populatedBuffer = FFI.GetSingleByteString(_session, oop, buffer.AsSpan());
			var utf8String = populatedBuffer.DecodeUTF8();
			ArrayPool<byte>.Shared.Return(buffer);

			return new (DotNetValueType.String, utf8String);
		}
		else if (classOop == GciOop.OOP_CLASS_LargeInteger)
		{
			// NOTE(AB): Return variant changed from Integer to Long.... Hmm. May cause issues, BUT is actually correct.
			return new (DotNetValueType.Long, FFI.GetLargeInteger(_session, oop));
		}
		else if (classOop == GciOop.OOP_CLASS_SYMBOL)
		{
			// TODO(CCK-3328): Correct size for symbols.
			var buffer = ArrayPool<byte>.Shared.Rent(minimumLength: 255);
			var populatedBuffer = FFI.GetSingleByteString(_session, oop, buffer.AsSpan());
			var utf8Symbol = populatedBuffer.DecodeUTF8();
			ArrayPool<byte>.Shared.Return(buffer);

			return new (DotNetValueType.String, utf8Symbol);
		}
		else if (classOop is GciOop.OOP_CLASS_Float or GciOop.OOP_CLASS_BINARY_FLOAT or GciOop.OOP_CLASS_SmallFloat)
		{
			// TODO(CCK-3328): Check Float,BinaryFloat,SmallFloat are even seen.
			return new (DotNetValueType.Double, FFI.GetFloat(_session, oop));
		}
		else if (classOop is GciOop.OOP_CLASS_DoubleByteString
			or GciOop.OOP_CLASS_DoubleByteSymbol
			or GciOop.OOP_CLASS_Unicode7
			or GciOop.OOP_CLASS_Unicode16
			or GciOop.OOP_CLASS_Unicode32)
		{
			// TODO(CCK-3328): Check if DoubleByteSymbol,Unicode7,Unicode16 are ever seen.
			var bufferSize = 1 + ((int)FFI.GetObjectSize(_session, oop) * 2);

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

			var populatedBytes = FFI.GetString(_session, oop, bytes);

			var decodedString = !populatedBytes.IsEmpty ? Encoding.UTF8.GetString(populatedBytes) : string.Empty;

			if (rentedBuffer is not null)
			{
				ArrayPool<byte>.Shared.Return(rentedBuffer);
			}

			return new (DotNetValueType.String, decodedString);
		}
		else if (classOop is GciOop.OOP_CLASS_FixedPoint or GciOop.OOP_CLASS_ScaledDecimal)
		{
			// TODO(CCK-3328): Check if ScaledDecimal,ScaledDecimalAndUnroundedValue are ever seen.
			var numberAsStringOop = FFI.ForeignPerform(_session, oop, "asString"u8, ReadOnlySpan<Oop>.Empty);

			var bufferSize = (int)FFI.GetObjectSize(_session, numberAsStringOop);
			var buffer = ArrayPool<byte>.Shared.Rent(minimumLength: bufferSize);
			var populatedBuffer = FFI.GetSingleByteString(_session, numberAsStringOop, buffer.AsSpan());
			var decodedNumberString = populatedBuffer.DecodeUTF8();
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

			// TODO(CCK-3328): Is the error checking really necessary here?
			var isError = false;
			_ = _session.CheckError(ref isError);
			if (Debugger.IsAttached && isError)
			{
				Debugger.Break();
			}

			return new (DotNetValueType.Double, value);
		}
		else if (classOop == GciOop.OOP_CLASS_FRACTION)
		{
			// TODO(CCK-3328): Check if Fraction is ever seen.
			var numberAsFloatOop = FFI.ForeignPerform(_session, oop, "asFloat"u8, ReadOnlySpan<Oop>.Empty);

			// TODO(CCK-3328): Is the error checking really necessary here?
			var isError = false;
			_ = _session.CheckError(ref isError);
			if (Debugger.IsAttached && isError)
			{
				Debugger.Break();
			}

			return new (DotNetValueType.Double, FFI.GetFloat(_session, numberAsFloatOop));
		}
		else if (classOop is GciOop.OOP_CLASS_ARRAY or GciOop.OOP_CLASS_ORDERED_COLLECTION
				|| FFI.IsKindOfClass(_session, oop, GciOop.OOP_CLASS_COLLECTION))
		{
			FetchCollectionValue(oop, out var collection);
			return new (DotNetValueType.Array, collection);
		}

		return FetchValueFromOpsClass(oop, classOop);
	}

	private GemStoneValue<object> FetchValueFromOpsClass(Oop oop, Oop classOop)
	{
		// TODO(CCK-3328): Use the type dictionary here - Identify and handle rather than getting the name.
		var classNameOop = FFI.ForeignPerform(_session, classOop, "name"u8, ReadOnlySpan<Oop>.Empty);
		Span<byte> classNameBuffer = stackalloc byte[255];
		var className = FFI.GetString(_session, classNameOop, classNameBuffer);

		if (className.SequenceEqual("SYSDate"u8))
		{
			return new (DotNetValueType.Date, FetchLocalDate());
		}
		else if (className.SequenceEqual("ScaledDecimalAndUnroundedValue"u8))
		{
			// TODO(CCK-3328): Copy-pasta development - un-copy pasta it.
			// TODO(CCK-3328): Check if ScaledDecimal,ScaledDecimalAndUnroundedValue are ever seen.
			var numberAsStringOop = FFI.ForeignPerform(_session, oop, "asString"u8, ReadOnlySpan<Oop>.Empty);

			var bufferSize = (int)FFI.GetObjectSize(_session, numberAsStringOop);
			var buffer = ArrayPool<byte>.Shared.Rent(minimumLength: bufferSize);
			var populatedBuffer = FFI.GetSingleByteString(_session, numberAsStringOop, buffer.AsSpan());
			var decodedNumberString = populatedBuffer.DecodeUTF8();
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

			var isError = false;
			_ = _session.CheckError(ref isError);
			if (Debugger.IsAttached && isError)
			{
				Debugger.Break();
			}

			return new (DotNetValueType.Double, value);
		}

		return new (DotNetValueType.Oop, oop);
	}

	#endregion Fetch Value

	#region Retrieve T

	private DateTime FetchLocalDate()
	{
		var typeMismatch = default(bool);

		var day = ForeignPerform("day"u8).AsInteger(ref typeMismatch);
		var month = ForeignPerform("month"u8).AsInteger(ref typeMismatch);
		var year = ForeignPerform("year"u8).AsInteger(ref typeMismatch);

		return new (year, month, day);
	}

	#endregion Retrieve T

	#region Fetch Value Special

	private GemStoneValue<byte[]> FetchCompressedString(Oop oop)
	{
		if (FFI.TryGetObjectInfo(_session, oop, out var objectInfo))
		{
			byte[] buffer = null;
			var populatedBytes = ReadOnlySpan<byte>.Empty;

			if (objectInfo.objClass == GciOop.OOP_CLASS_STRING)
			{
				var bufferSize = (int)FFI.GetObjectSize(_session, oop);
				buffer = new byte[bufferSize];
				populatedBytes = FFI.GetSingleByteString(_session, oop, buffer.AsSpan());
			}
			else if (objectInfo.objClass is GciOop.OOP_CLASS_DoubleByteString or GciOop.OOP_CLASS_Unicode16)
			{
				var bufferSize = 1 + ((int)FFI.GetObjectSize(_session, oop) * 2);
				buffer = new byte[bufferSize];
				populatedBytes = FFI.GetString(_session, oop, buffer.AsSpan());
			}

			if (!populatedBytes.IsEmpty)
			{
				if (populatedBytes.Length != buffer!.Length)
				{
					// Make sure we didn't write less than expected by GetObjectSize
					buffer.AsSpan(populatedBytes.Length).Clear();
				}

				return new (DotNetValueType.Array, buffer);
			}
		}

		return new (DotNetValueType.None, default);
	}

	private void FetchCollectionValue(Oop oop, out GemStoneObject[] objectCollection)
	{
		var isError = default(bool);

		var collectionOop = FFI.ForeignPerform(_session, oop, "asOrderedCollection"u8, ReadOnlySpan<Oop>.Empty);
		_ = _session.CheckError(ref isError);

		var size = FFI.GetObjectSize(_session, collectionOop);

		objectCollection = new GemStoneObject[(int)(size + 1)];
		var collectionSpan = objectCollection.AsSpan();

		Span<Oop> oops = stackalloc Oop[1];

		for (var ii = 1; ii < collectionSpan.Length; ii++)
		{
			oops[0] = GciOop.OOP_NIL;
			var oopCount = FFI.GetCollectionObjects(_session, collectionOop, (long)ii, oops);
			if (oopCount == oops.Length)
			{
				_ = FFI.PersistObjects(_session, oops);
			}
			else
			{
				oops[0] = GciOop.OOP_NIL;
			}

			var value = FetchValue(oops[0]);

			collectionSpan[ii] =
				value.Type is DotNetValueType.None or DotNetValueType.Oop
					? new (_session, (Oop)value.Value)
					: new (_session, value.Value);
		}
	}

	private GemStoneValue<ReadOnlyMemory<byte>> FetchJsonMemory(Oop oop)
	{
		if (FFI.TryGetObjectInfo(_session, oop, out var objectInfo))
		{
			Memory<byte> buffer;
			ReadOnlySpan<byte> populatedBytes;

			if (objectInfo.objClass == GciOop.OOP_CLASS_STRING)
			{
				var bufferSize = objectInfo.objSize;
				buffer = new byte[bufferSize];
				populatedBytes = FFI.GetSingleByteString(_session, oop, buffer.Span);
			}
			else if (objectInfo.objClass is GciOop.OOP_CLASS_DoubleByteString or GciOop.OOP_CLASS_Unicode16)
			{
				var bufferSize = 1 + objectInfo.objSize * 2;
				buffer = new byte[bufferSize];
				populatedBytes = FFI.GetString(_session, oop, buffer.Span);
			}
			else
			{
				return new (DotNetValueType.None, default);
			}

			if (populatedBytes.Length != buffer.Length)
			{
				// Make sure we didn't write less than expected by GetObjectSize
				buffer.Span[populatedBytes.Length..].Clear();
			}

			return new (DotNetValueType.ReadOnlyMemory, (ReadOnlyMemory<byte>)buffer);
		}

		return new (DotNetValueType.None, default);
	}

	#endregion Fetch Value Special

	#region Foreign Perform

	public static readonly ActivitySource OpsTraceSource = new ActivitySource("Ops");

	internal void SetBasicActivityTags(Activity activity, ReadOnlySpan<byte> selector)
	{
		_session.OpsMeter?.GemstoneForeignFunctionCall(selector.DecodeUTF8());
		activity?.SetTag("oop", Oop);
		activity?.SetTag("selector", selector.DecodeUTF8());
		// TODO(TG) Fix this please
		if (Activity.Current is null)
		{
			return;
		}

		var traceId = Activity.Current.TraceId;

		if (activity is not null &&
			(_session.LastTraceIdToOps is null ||
				!_session.LastTraceIdToOps.Equals(traceId.ToString())))
		{
			activity.SetTag("reported span", activity.SpanId);
			var parentId = activity.SpanId;
			_session.LastTraceIdToOps = traceId.ToString();
			_session.Execute(
				Encoding
					.ASCII
					.GetBytes(
						$"""
						 |handledExeption|
						 handledExeption := Exception category: nil number: nil do: [ :ex :cat :num :args |
						 ex ifNotNil:[ex remove].
						 ^nil].
						 System beginNestedTransaction.
						 SparkOTLPRecord setTrace: '{traceId}' on:'{parentId}'.
						 System commitTransaction.
						 """)
					.AsSpan());

			var isError = false;
			_session.CheckError(ref isError);
		}
	}

	internal void SetActivityTags(Activity activity, ReadOnlySpan<byte> selector, Span<Oop> args)
	{
		SetBasicActivityTags(activity, selector);
		var i = 0;
		foreach (var x in args)
		{
			activity?.SetTag($"arg{i}", x);
			i++;
		}
	}

	public GemStoneObject ForeignPerform(ReadOnlySpan<byte> selector)
	{
		using (var activity = OpsTraceSource.StartActivity("Foreign perform {selector} on {oop}."))
		{
			SetBasicActivityTags(activity, selector);
			var oop = FFI.ForeignPerform(_session, Oop, selector, ReadOnlySpan<Oop>.Empty);
			var isError = default(bool);
			_ = _session.CheckError(ref isError);

			if (!isError)
			{
				activity?.AddEvent(new ActivityEvent("Success"));
			}

			return oop != Oop ? new (_session, oop) : this;
		}
	}

	public GemStoneObject ForeignPerform(ReadOnlySpan<byte> selector, CancellationToken ct)
	{
		if (selector.ToString() == "nextPage")
		{
			Debugger.Break();
		}

		using (var activity = OpsTraceSource.StartActivity("Cullable foreign perform {selector} on {oop}."))
		{
			SetBasicActivityTags(activity, selector);
			GemStoneCullableInteraction nbinteraction = new (_session, ct);
			var nbCallTask = nbinteraction.ForeignPerformAsync(Oop, selector);
			return nbCallTask.GetAwaiter().GetResult();
		}
	}

	public GemStoneObject ForeignPerformWithArgs<T0>(ReadOnlySpan<byte> selector, T0 arg0)
	{
		using (var activity = OpsTraceSource.StartActivity("Foreign perform {selector} on {oop} passsing {arg0}."))
		{
			Span<Oop> args = stackalloc Oop[1];
			args[0] = ConvertArgument(arg0);
			SetActivityTags(activity, selector, args);

			var oop = FFI.ForeignPerform(_session, Oop, selector, args);
			return oop != Oop ? new (_session, oop) : this;
		}
	}

	public GemStoneObject ForeignPerformWithArgs<T0>(ReadOnlySpan<byte> selector, T0 arg0, CancellationToken ct)
	{
		using (var activity =
			OpsTraceSource.StartActivity("Cullable foreign perform {selector} on {oop} passsing {arg0}."))
		{
			Span<Oop> args = stackalloc Oop[1];
			args[0] = ConvertArgument(arg0);
			SetActivityTags(activity, selector, args);

			GemStoneCullableInteraction nbinteraction = new (_session, ct);
			var nbCallTask = nbinteraction.ForeignPergormWithArgsAsync(Oop, selector, args);
			return nbCallTask.GetAwaiter().GetResult();
		}
	}

	public GemStoneObject ForeignPerformWithArgs<T0, T1>(ReadOnlySpan<byte> selector, T0 arg0, T1 arg1)
	{
		using (var activity =
			OpsTraceSource.StartActivity("Foreign perform {selector} on {oop} passsing {arg0} {arg1}."))
		{
			Span<Oop> args = stackalloc Oop[2];
			args[0] = ConvertArgument(arg0);
			args[1] = ConvertArgument(arg1);
			SetActivityTags(activity, selector, args);

			var oop = FFI.ForeignPerform(_session, Oop, selector, args);
			return oop != Oop ? new (_session, oop) : this;
		}
	}

	public GemStoneObject ForeignPerformWithArgs<T0, T1>(
		ReadOnlySpan<byte> selector,
		T0 arg0,
		T1 arg1,
		CancellationToken ct
	)
	{
		using (var activity =
			OpsTraceSource.StartActivity(
				"Cullable foreign perform {selector} " +
				"on {oop} passsing {arg0} {arg1}."))
		{
			Span<Oop> args = stackalloc Oop[2];
			args[0] = ConvertArgument(arg0);
			args[1] = ConvertArgument(arg1);
			SetActivityTags(activity, selector, args);

			GemStoneCullableInteraction nbinteraction = new (_session, ct);
			var nbCallTask = nbinteraction.ForeignPergormWithArgsAsync(Oop, selector, args);
			return nbCallTask.GetAwaiter().GetResult();
		}
	}

	public GemStoneObject ForeignPerformWithArgs<T0, T1, T2>(ReadOnlySpan<byte> selector, T0 arg0, T1 arg1, T2 arg2)
	{
		using (var activity =
			OpsTraceSource.StartActivity(
				"Foreign perform {selector} " +
				"on {oop} passsing {arg0} {arg1} {arg2}."))
		{
			Span<Oop> args = stackalloc Oop[3];
			args[0] = ConvertArgument(arg0);
			args[1] = ConvertArgument(arg1);
			args[2] = ConvertArgument(arg2);
			SetActivityTags(activity, selector, args);

			var oop = FFI.ForeignPerform(_session, Oop, selector, args);
			return oop != Oop ? new (_session, oop) : this;
		}
	}

	public GemStoneObject ForeignPerformWithArgs<T0, T1, T2, T3>(
		ReadOnlySpan<byte> selector,
		T0 arg0,
		T1 arg1,
		T2 arg2,
		T3 arg3
	)
	{
		using (var activity =
			OpsTraceSource.StartActivity(
				"Foreign perform {selector} " +
				"on {oop} passsing  {arg0} {arg1} {arg2} {arg3}."))
		{
			Span<Oop> args = stackalloc Oop[4];
			args[0] = ConvertArgument(arg0);
			args[1] = ConvertArgument(arg1);
			args[2] = ConvertArgument(arg2);
			args[3] = ConvertArgument(arg3);
			SetActivityTags(activity, selector, args);

			var oop = FFI.ForeignPerform(_session, Oop, selector, args);
			return oop != Oop ? new (_session, oop) : this;
		}
	}

	public GemStoneObject ForeignPerformWithArgs<T0, T1, T2, T3, T4>(
		ReadOnlySpan<byte> selector,
		T0 arg0,
		T1 arg1,
		T2 arg2,
		T3 arg3,
		T4 arg4
	)
	{
		using (var activity =
			OpsTraceSource.StartActivity(
				"Foreign perform {selector} " +
				"on {oop} passsing  {arg0} {arg1} {arg2} {arg3} {arg4}."))
		{
			Span<Oop> args = stackalloc Oop[5];
			args[0] = ConvertArgument(arg0);
			args[1] = ConvertArgument(arg1);
			args[2] = ConvertArgument(arg2);
			args[3] = ConvertArgument(arg3);
			args[4] = ConvertArgument(arg4);
			SetActivityTags(activity, selector, args);

			var oop = FFI.ForeignPerform(_session, Oop, selector, args);
			return oop != Oop ? new (_session, oop) : this;
		}
	}

	public GemStoneObject ForeignPerformWithArgs<T0, T1, T2, T3, T4, T5>(
		ReadOnlySpan<byte> selector,
		T0 arg0,
		T1 arg1,
		T2 arg2,
		T3 arg3,
		T4 arg4,
		T5 arg5
	)
	{
		using (var activity =
			OpsTraceSource.StartActivity(
				"Foreign perform {selector} " +
				"on {oop} passsing  {arg0} {arg1} {arg2} {arg3} {arg4} {arg5}."))
		{
			Span<Oop> args = stackalloc Oop[6];
			args[0] = ConvertArgument(arg0);
			args[1] = ConvertArgument(arg1);
			args[2] = ConvertArgument(arg2);
			args[3] = ConvertArgument(arg3);
			args[4] = ConvertArgument(arg4);
			args[5] = ConvertArgument(arg5);
			SetActivityTags(activity, selector, args);

			var oop = FFI.ForeignPerform(_session, Oop, selector, args);
			return oop != Oop ? new (_session, oop) : this;
		}
	}

	public GemStoneObject ForeignPerformWithArgs<T0, T1, T2, T3, T4, T5, T6>(
		ReadOnlySpan<byte> selector,
		T0 arg0,
		T1 arg1,
		T2 arg2,
		T3 arg3,
		T4 arg4,
		T5 arg5,
		T6 arg6
	)
	{
		using (var activity =
			OpsTraceSource.StartActivity(
				"Foreign perform {selector} " +
				"on {oop} passsing  {arg0} {arg1} {arg2} {arg3} {arg4} {arg5} {arg6}."))
		{
			Span<Oop> args = stackalloc Oop[7];
			args[0] = ConvertArgument(arg0);
			args[1] = ConvertArgument(arg1);
			args[2] = ConvertArgument(arg2);
			args[3] = ConvertArgument(arg3);
			args[4] = ConvertArgument(arg4);
			args[5] = ConvertArgument(arg5);
			args[6] = ConvertArgument(arg6);
			SetActivityTags(activity, selector, args);

			var oop = FFI.ForeignPerform(_session, Oop, selector, args);
			return oop != Oop ? new (_session, oop) : this;
		}
	}

	public GemStoneObject ForeignPerformWithArgs<T0, T1, T2, T3, T4, T5, T6, T7>(
		ReadOnlySpan<byte> selector,
		T0 arg0,
		T1 arg1,
		T2 arg2,
		T3 arg3,
		T4 arg4,
		T5 arg5,
		T6 arg6,
		T7 arg7
	)
	{
		using (var activity =
			OpsTraceSource.StartActivity(
				"Foreign perform {selector} " +
				"on {oop} passsing  {arg0} {arg1} {arg2} {arg3} {arg4} {arg5} {arg6} {arg7}."))
		{
			Span<Oop> args = stackalloc Oop[8];
			args[0] = ConvertArgument(arg0);
			args[1] = ConvertArgument(arg1);
			args[2] = ConvertArgument(arg2);
			args[3] = ConvertArgument(arg3);
			args[4] = ConvertArgument(arg4);
			args[5] = ConvertArgument(arg5);
			args[6] = ConvertArgument(arg6);
			args[7] = ConvertArgument(arg7);
			SetActivityTags(activity, selector, args);

			var oop = FFI.ForeignPerform(_session, Oop, selector, args);
			return oop != Oop ? new (_session, oop) : this;
		}
	}

	public GemStoneObject ForeignPerformWithArgs<T0, T1, T2, T3, T4, T5, T6, T7, T8>(
		ReadOnlySpan<byte> selector,
		T0 arg0,
		T1 arg1,
		T2 arg2,
		T3 arg3,
		T4 arg4,
		T5 arg5,
		T6 arg6,
		T7 arg7,
		T8 arg8
	)
	{
		using (var activity =
			OpsTraceSource.StartActivity(
				"Foreign perform {selector} " +
				"on {oop} passsing  {arg0} {arg1} {arg2} {arg3} {arg4} {arg5} {arg6} {arg7} {arg8}."))
		{
			Span<Oop> args = stackalloc Oop[9];
			args[0] = ConvertArgument(arg0);
			args[1] = ConvertArgument(arg1);
			args[2] = ConvertArgument(arg2);
			args[3] = ConvertArgument(arg3);
			args[4] = ConvertArgument(arg4);
			args[5] = ConvertArgument(arg5);
			args[6] = ConvertArgument(arg6);
			args[7] = ConvertArgument(arg7);
			args[8] = ConvertArgument(arg8);
			SetActivityTags(activity, selector, args);

			var oop = FFI.ForeignPerform(_session, Oop, selector, args);
			return oop != Oop ? new (_session, oop) : this;
		}
	}

	public GemStoneObject ForeignPerformWithArgs<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
		ReadOnlySpan<byte> selector,
		T0 arg0,
		T1 arg1,
		T2 arg2,
		T3 arg3,
		T4 arg4,
		T5 arg5,
		T6 arg6,
		T7 arg7,
		T8 arg8,
		T9 arg9
	)
	{
		using (var activity =
			OpsTraceSource.StartActivity(
				"Foreign perform {selector} " +
				"on {oop} passsing  {arg0} {arg1} {arg2} {arg3} {arg4} {arg5} {arg6} {arg7} {arg8} {arg9}."))
		{
			Span<Oop> args = stackalloc Oop[10];
			args[0] = ConvertArgument(arg0);
			args[1] = ConvertArgument(arg1);
			args[2] = ConvertArgument(arg2);
			args[3] = ConvertArgument(arg3);
			args[4] = ConvertArgument(arg4);
			args[5] = ConvertArgument(arg5);
			args[6] = ConvertArgument(arg6);
			args[7] = ConvertArgument(arg7);
			args[8] = ConvertArgument(arg8);
			args[9] = ConvertArgument(arg9);
			SetActivityTags(activity, selector, args);

			var oop = FFI.ForeignPerform(_session, Oop, selector, args);
			return oop != Oop ? new (_session, oop) : this;
		}
	}

	private Oop ConvertArgument<TArg>(TArg argument)
	{
		switch (argument)
		{
			case null:
				return GciOop.OOP_NIL;
			case Oop oop:
				long asLong = (long)oop;
				if (asLong < 0)
				{
					ThrowHelper.ThrowArgumentException("argument", "Oop was negative");
				}

				return asLong.ToGemStoneOop() ?? FFI.NewLargeInteger(_session, asLong);
			case GemStoneObject gsObject:
				return gsObject.Oop;
			case char[] charArray:
				Debugger.Break(); // You should just pass a string instead.
				return stringConversion(new string(charArray));
			case string str:
				return stringConversion(str);
			case int num:
				return num.ToGemStoneOop();
			case double dbl:
				return dbl.ToGemStoneOop() ?? FFI.NewFloat(_session, dbl);
			case DateTime dt:
				return FFI.RTNewSYSDate(
					_session,
					dt.Day.ToGemStoneOop(),
					dt.Month.ToGemStoneOop(),
					dt.Year.ToGemStoneOop());
			case long longNum:
				return longNum.ToGemStoneOop() ?? FFI.NewLargeInteger(_session, longNum);
			case bool boolean:
				return boolean ? GciOop.OOP_TRUE : GciOop.OOP_FALSE;
			case short shortNum:
				return shortNum.ToGemStoneOop();
			case float floatNum:
			{
				double asDouble = (double)floatNum;
				return asDouble.ToGemStoneOop() ?? FFI.NewFloat(_session, asDouble);
			}
			default:
				// Do *NOT* ignore this break, an argument has been supplied that isn't handled by the current methods.
				// Either write a conversion and add it in priority order or change it to a compatible type before calling.
				Debugger.Break();
				return 0UL; // Mimicing existing behaviour, effectively an undefined object.
		}

		Oop stringConversion(string str)
		{
			scoped Span<byte> unicodeBytes;

			if (str is not null)
			{
				// TODO(CCK-3328): Check if there was any good reason to always make double width strings...
				// If the string doesn't contain any non-ascii characters (*most* of ours) it'd be more efficient
				// for both sides to write as single byte instead.
				var trimmedString = str.AsSpan().TrimEnd();
				var unicodeByteCount = Encoding.Unicode.GetByteCount(trimmedString);
				// TODO(AB): Should be testing size here, make sure we're not stackallocing a massive string
				unicodeBytes = stackalloc byte[unicodeByteCount];
				Encoding.Unicode.GetBytes(trimmedString, unicodeBytes);
			}
			else
			{
				unicodeBytes = Span<byte>.Empty;
			}

			var unicodeSpan = MemoryMarshal.Cast<byte, ushort>(unicodeBytes);
			return FFI.NewString(_session, (ReadOnlySpan<ushort>)unicodeSpan);
		}
	}

	#endregion Foreign Perform

	#region Foreign Perform Plus

	public string PrintString()
	{
		if (IsNil())
		{
			// Short-circuit path - This should really reference a constant, but if the repr of nil
			// ever changed this wouldn't be the first place it blew up.
			return "nil";
		}

		var valueWrapper = FetchValue(Oop);
		if (valueWrapper.Type is DotNetValueType.None or DotNetValueType.Oop)
		{
			var typeMismatch = default(bool);
			return new GemStoneObject(_session, (Oop)valueWrapper.Value)
				.ForeignPerform("printString"u8)
				.AsString(ref typeMismatch);
		}
		else
		{
			return Conversions.ToString(valueWrapper.Value);
		}
	}

	public GemStoneObject Item(int index)
	{
		Span<Oop> oops = stackalloc Oop[1];

		var oopCount = FFI.GetCollectionObjects(_session, Oop, (long)index, oops);

		if (oopCount != oops.Length)
		{
			return new (_session, GciOop.OOP_NIL);
		}

		// TODO(CCK-3328): See if this persist is actually necessary.
		_ = FFI.PersistObjects(_session, oops);

		var value = FetchValue(oops[0]);

		return value.Type is DotNetValueType.None or DotNetValueType.Oop
			? new (_session, (Oop)value.Value)
			: new (_session, value.Value);
	}

	public string DealerRiskName()
	{
		return ForeignPerform("dealerRiskName"u8).PrintString();
	}

	#endregion Foreign Perform Plus

	#endregion Legacy API
}
