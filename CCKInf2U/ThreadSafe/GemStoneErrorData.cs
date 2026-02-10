#nullable enable

using CCKInf2U.Interop;
using System;

namespace CCKInf2U.ThreadSafe;

/// <summary>
/// Storage for information contained in a <see cref="GciConstants.GciErrSType"/> and any worthwhile supplementary
/// information.
/// </summary>
/// <param name="Category"><see cref="GciConstants.GciErrSType.category"/></param>
/// <param name="Context"><see cref="GciConstants.GciErrSType.context"/></param>
/// <param name="ExceptionObj"><see cref="GciConstants.GciErrSType.exceptionObj"/></param>
/// <param name="Args"><see cref="GciConstants.GciErrSType.args"/></param>
/// <param name="Number"><see cref="GciConstants.GciErrSType.number"/></param>
/// <param name="ArgCount"><see cref="GciConstants.GciErrSType.argCount"/></param>
/// <param name="Fatal"><see cref="GciConstants.GciErrSType.fatal"/></param>
/// <param name="Message"><see cref="GciConstants.GciErrSType.message"/></param>
/// <param name="Reason"><see cref="GciConstants.GciErrSType.reason"/></param>
/// <param name="When">The time this error was logged.</param>
internal sealed record GemStoneErrorData(
	OopType Category,
	OopType Context,
	OopType ExceptionObj,
	OopType[]? Args,
	int Number,
	int ArgCount,
	byte Fatal,
	string? Message,
	string? Reason,
	DateTime When);

#nullable restore