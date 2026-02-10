using System.Diagnostics;

namespace Util.Shims;

public static class DebuggerShim
{
	[Conditional("DEBUG")]
	public static void Break()
	{
		if (Debugger.IsAttached)
		{
			Debugger.Break();
		}
	}

}
