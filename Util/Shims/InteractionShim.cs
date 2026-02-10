using System;
using Microsoft.VisualBasic;

namespace Util.Shims;

/// <summary>
/// Replacement for calls to <see cref="Interaction"/> regsitry calls.
/// </summary>
public static class InteractionShim
{
	[Obsolete("Ideally we don't save to the registry.")]
	public static void SaveSetting(string AppName, string Section, string Key, string Setting)
	{
		if (ContextInformation.IsWeb)
		{
			// Do nothing for now!
		}
		else
		{
#pragma warning disable CA1416 // Validate platform compatibility - We are running on windows!
			Interaction.SaveSetting(AppName, Section, Key, Setting);
#pragma warning restore CA1416 // Validate platform compatibility - We are running on windows!
		}
	}
}
