using System;

namespace Util;

public static class ContextInformation
{
	public static readonly bool IsWeb = System.Reflection.Assembly.GetEntryAssembly()?.FullName is var assemblyName
		&& assemblyName is not null
		&& !(assemblyName.StartsWith("CCK", StringComparison.OrdinalIgnoreCase)
			|| assemblyName.StartsWith("GDR", StringComparison.OrdinalIgnoreCase)
			|| assemblyName.StartsWith("RsTcpServ", StringComparison.OrdinalIgnoreCase));
}
