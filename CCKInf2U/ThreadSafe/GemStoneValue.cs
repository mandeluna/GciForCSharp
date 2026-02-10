#nullable enable

namespace CCKInf2U;

internal readonly ref struct GemStoneValue<T>(DotNetValueType type, T value)
{
	public DotNetValueType Type { get; init; } = type;
	public T Value { get; init; } = value;
}

internal enum DotNetValueType
{
	/// <summary>
	/// Associated value is some form of unusable oop (ILLEGAL, NIL, etc.)
	/// </summary>
	None = 0,

	Oop,
	Boolean,
	Integer,
	Long,
	Double,
	Date,
	String,
	Array,
	ReadOnlyMemory,
}

#nullable restore