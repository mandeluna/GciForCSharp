using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using BucketState = byte;

namespace CCKUTIL2;

#nullable enable
#pragma warning disable IDE0034 // Simplify 'default' expression

#if NET5_0_OR_GREATER
[SkipLocalsInit]
#endif
public sealed class CCKNotifyingCollection<TValue> : CCKCollection<TValue> where TValue : class
{
	public event OnChangeEvent? OnChange;

	public delegate void OnChangeEvent(object sender);

	public override void Add(TValue value)
	{
		base.Add(value);
		OnChange?.Invoke(this);
	}

	public override void Add(TValue value, string? key)
	{
		base.Add(value, key);
		OnChange?.Invoke(this);
	}

	public override void Clear()
	{
		base.Clear();
		OnChange?.Invoke(this);
	}

	public override void Remove(object someObject)
	{
		//! This overload only exists due to time constraints - New calls should prefer the strongly typed overloads
		if (someObject is null)
		{
			return;
		}
		else if (someObject is string key)
		{
			Remove(key);
		}
		else if (someObject is ValueType and IConvertible index)
		{
			Remove(index.ToInt32(null));
		}
#if DEBUG
		else
		{
			Debug.Fail($"Removal attempted with invalid type {someObject.GetType().Name}");
		}
#endif
	}

	public override void Remove(string? key)
	{
		if (key is null)
		{
			return;
		}

		var shouldRaiseEvent = KeyedValues.ContainsKey(key);

		base.Remove(key);

		if (shouldRaiseEvent)
		{
			OnChange?.Invoke(this);
		}
	}

	public override void Remove(int oneBasedIndex)
	{
		if (IsIndexInRange(oneBasedIndex - 1))
		{
			base.Remove(oneBasedIndex);
			OnChange?.Invoke(this);
		}
	}
}

#if NET5_0_OR_GREATER
[SkipLocalsInit]
#endif
public class CCKCollection<TValue> : CCKCollection<string, TValue> where TValue : class
{
	public CCKCollection() : base(skipInternalCollectionInitialisation: true)
	{
		OrderedBuckets = new();
		KeyedValues = new(comparer: StringComparer.CurrentCultureIgnoreCase);
	}

	public CCKCollection(int capacity) : base(skipInternalCollectionInitialisation: true)
	{
		Guard.IsGreaterThanOrEqualTo(capacity, 0);

		OrderedBuckets = new(capacity);
		KeyedValues = new(capacity, StringComparer.CurrentCultureIgnoreCase);
	}

	public override void Add(TValue value, string? key)
	{
		if (IsStringKeyLegal(key))
		{
			base.Add(value, key!);
		}
		else
		{
			base.Add(value);
		}
	}

	public override void Remove(string? key)
	{
		if (key is not null)
		{
			base.Remove(key);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected bool IsStringKeyLegal(
#if NETCOREAPP3_0_OR_GREATER
		[NotNullWhen(true)]
#endif
		string? key) => !string.IsNullOrEmpty(key);
}

#if NET5_0_OR_GREATER
[SkipLocalsInit]
#endif
public class CCKCollection<TKey, TValue> : IEnumerable<TValue>
	where TKey : notnull
	where TValue : class
{
	[StructLayout(LayoutKind.Sequential)]
#if NET7_0_OR_GREATER
	protected readonly record struct Bucket(TValue Value, [ConstantExpected] BucketState IsKeyed, TKey? Key);
#else
	protected readonly record struct Bucket(TValue Value, BucketState IsKeyed, TKey? Key);
#endif

	public int Count => OrderedBuckets.Count;

	protected List<Bucket> OrderedBuckets { get; init; }
	protected Dictionary<TKey, TValue> KeyedValues { get; init; }

	private const BucketState BucketWithKey = 0xFF;
	private const BucketState BucketNoKey = 0x00;

	public CCKCollection()
	{
#if DEBUG
		// Key is able to be massaged to an int.
		// We don't support this for legacy reasons (single method for accessing by index/key)
		Guard.IsTrue(KeyTypeIsCompatible());
#endif

		OrderedBuckets = new();
		KeyedValues = new();
	}

	public CCKCollection(int capacity)
	{
#if DEBUG
		// Key is able to be massaged to an int.
		// We don't support this for legacy reasons (single method for accessing by index/key)
		Guard.IsTrue(KeyTypeIsCompatible());
#endif

		Guard.IsGreaterThanOrEqualTo(capacity, 0);

		OrderedBuckets = new(capacity);
		KeyedValues = new(capacity);
	}

	/// <summary>
	/// Skips the base class initialising the internal collections.
	/// </summary>
	/// <remarks>
	/// This should <b>only</b> be called when the caller guarantees the internal collections are initialised.
	/// </remarks>
	/// <param name="skipInternalCollectionInitialisation">Irrelevant.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable IDE0060, CS8618, RCS1163 // Deliberate protected overload that doesn't initialise the object
	protected CCKCollection(bool skipInternalCollectionInitialisation)
#pragma warning restore IDE0060, CS8618, RCS1163 // Deliberate protected overload that doesn't initialise the object
	{
		// NOP.
	}

	public IEnumerable<TKey> GetKeys()
	{
		return KeyedValues.Keys;
	}
	
	public virtual void Add(TValue value)
	{
		OrderedBuckets.Add(new(value, BucketNoKey, default(TKey?)));
	}

	public virtual void Add(TValue value, TKey key)
	{
		Remove(key);
		KeyedValues.Add(key, value);
		OrderedBuckets.Add(new(value, BucketWithKey, key));
	}

	public void AddRange(IEnumerable<TValue>? values)
	{
		if (values is not null)
		{
			foreach (var value in values)
			{
				Add(value);
			}
		}
	}

	public virtual void Clear()
	{
		KeyedValues.Clear();
		OrderedBuckets.Clear();
	}

	public bool Contains(TKey key)
	{
		return KeyedValues.ContainsKey(key);
	}

	public TValue? Item(object someObject)
	{
		//! This overload only exists due to time constraints - New calls should prefer the strongly typed overloads
		if (someObject is null)
		{
			return default(TValue?);
		}
		else if (someObject is TKey key)
		{
			return Item(key);
		}
		else if (someObject is ValueType and IConvertible index)
		{
			return Item(index.ToInt32(null));
		}
		else
		{
#if DEBUG
			Debug.Fail($"Indexing attempted with invalid type {someObject.GetType().Name}");
#endif
			return default(TValue?);
		}
	}

	public TValue? Item(TKey? key)
	{
		return key is not null && KeyedValues.TryGetValue(key, out var value)
			? value
			: default(TValue?);
	}

	public TValue? Item(int oneBasedIndex)
	{
		return IsIndexInRange(oneBasedIndex - 1)
			? OrderedBuckets[(oneBasedIndex - 1)].Value
			: default(TValue?);
	}

	public virtual void Remove(object someObject)
	{
		//! This overload only exists due to time constraints - New calls should prefer the strongly typed overloads
		if (someObject is null)
		{
			return;
		}
		else if (someObject is TKey key)
		{
			Remove(key);
		}
		else if (someObject is ValueType and IConvertible index)
		{
			Remove(index.ToInt32(null));
		}
#if DEBUG
		else
		{
			Debug.Fail($"Removal attempted with invalid type {someObject.GetType().Name}");
		}
#endif
	}

	public virtual void Remove(TKey key)
	{
		//! This will throw if key is null because ***it's not meant to be***

#if NETCOREAPP3_0_OR_GREATER
		if (KeyedValues.Remove(key, out var oldValue))
		{
			OrderedBuckets.Remove(new(oldValue, BucketWithKey, key));
		}
#else
		if (KeyedValues.TryGetValue(key, out var oldValue))
		{
			KeyedValues.Remove(key);
			OrderedBuckets.Remove(new(oldValue, BucketWithKey, key));
		}
#endif
	}

	public virtual void Remove(int oneBasedIndex)
	{
		if (!IsIndexInRange(oneBasedIndex - 1))
		{
			return;
		}

		var oldBucket = OrderedBuckets[oneBasedIndex - 1];
		OrderedBuckets.RemoveAt(oneBasedIndex - 1);

		if (oldBucket.Key is not null)
		{
			_ = KeyedValues.Remove(oldBucket.Key);
		}
	}

	public void ReplaceContents(CCKCollection<TKey, TValue> replacement)
	{
		Clear();
		foreach (var bucket in replacement.OrderedBuckets)
		{
			if (bucket.IsKeyed == BucketWithKey)
			{
				Add(bucket.Value, bucket.Key!);
			}
			else
			{
				Add(bucket.Value);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected bool IsIndexInRange(int zeroBasedIndex) => (uint)zeroBasedIndex < (uint)OrderedBuckets.Count;

	#region Enumeration

	public IEnumerator<TValue> GetEnumerator()
	{
		foreach (var bucket in OrderedBuckets)
		{
			yield return bucket.Value;
		}
	}

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	#endregion Enumeration

	#region Visual Basic Holdovers

	[Obsolete($"Avoid using {nameof(Microsoft.VisualBasic.Collection)}.")]
	public Microsoft.VisualBasic.Collection AsCollection()
	{
		Microsoft.VisualBasic.Collection collection = new();
		foreach (var bucket in OrderedBuckets)
		{
			collection.Add(bucket.Value);
		}
		return collection;
	}

	#endregion Visual Basic Holdovers

	#region Key Type Checking (Debug only)
#if DEBUG

	private static bool KeyTypeIsCompatible()
	{
		var keyType = typeof(TKey);

		return !(keyType == typeof(int)
			|| keyType == typeof(float)
			|| keyType == typeof(double)
			|| keyType == typeof(short)
			|| keyType == typeof(long)
			|| keyType == typeof(ulong)
			|| keyType == typeof(byte)
			|| keyType == typeof(sbyte)
			|| keyType == typeof(ushort)
			|| keyType == typeof(uint)
			|| keyType == typeof(nint)
			|| keyType == typeof(nuint)
#if NET5_0_OR_GREATER
			|| keyType == typeof(Half)
#endif
#if NET7_0_OR_GREATER
			|| keyType == typeof(Int128)
			|| keyType == typeof(UInt128)
#endif
			);
	}

#endif
	#endregion Key Type Checking (Debug only)
}

#pragma warning restore IDE0034 // Simplify 'default' expression
#nullable restore