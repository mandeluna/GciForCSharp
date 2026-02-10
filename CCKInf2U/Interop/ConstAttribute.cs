using System;

namespace CCKInf2U.Interop;

/// <summary>
/// Explicit marker that FFI had pointer marked as <c>const</c>. Doesn't affect code generation at all.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
internal class ConstAttribute : Attribute
{
}
