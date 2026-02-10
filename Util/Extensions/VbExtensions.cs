using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Util.Extensions;

//TODO Renamed and namespace changes
public static class VbExtensions
{
	private readonly static MethodInfo _friendlyVbMethod = PopulateFriendlyVbMethod();

	/// <summary>
	/// Exists for replacing calls to Information.TypeName() since this behaves predicable between C# and VB
	/// This currently emulates the old VB behaviour, however it should be replaced with more normal usages.
	/// </summary>
	/// <param name="obj"></param>
	/// <returns>The objects type, or "Nothing"</returns>
	[Obsolete("Prefer usage of Pattern matching or .GetType().Name")]
	[SkipLocalsInit]
	public static string VbTypeName(this object? obj)
	{
		if (obj is null)
		{
			return "Nothing";
		}

		var oldBehaviour = (string)_friendlyVbMethod.Invoke(null, new[] { obj.GetType().Name })!;
		var newBehaviour = obj.GetType().Name;

		// This fires when the old behaviour does not match the normal C# behaviour
		// This is a big deal, since they return strings, it's not easy to find the bugs this introduces

		// In the event that this triggers, it would be best to manually find the offending call sites,
		// and fix them so they work with the new behaviour, and then replace their calls to this funciton
		// instead with the new behaviour directly. We wish to remove the old behaviour from the code.

		// Alternatively replacing the magic string logic with something else, would be even better.
		if (newBehaviour != oldBehaviour)
		{
			LogBehaviourMismatch(newBehaviour, oldBehaviour);
		}

		if (oldBehaviour == "CCKNotifyingCollection")
		{
			// Event invocation was moved from the base CCKCollection class to CCKNotifyingCollection.
			// As the system hasn't been adjusted to respect the change of name, we should pretend it hasn't
			// changed for type comparison purposes.
			// This *will* break in situation when a direct cast is made to CCKCollection currently as
			// CCKCollection<string, object> isn't a parent of CCKNotifyingCollection<TValue> - but we'll deal
			// with any circumstances on a case-by-case basis.
			oldBehaviour = "CCKCollection";
		}

		return oldBehaviour;
	}

	public static T ReadPrivate<T>(this object source, string fieldName)
	{
		var field = source.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

		if (field is null)
		{
			throw new InvalidOperationException();
		}

		var value = field.GetValue(source);

		if (value is T castValue)
		{
			return castValue;
		}

		throw new InvalidOperationException();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void LogBehaviourMismatch(string newBehaviour, string oldBehaviour)
	{
		Debug.WriteLine($"Types did not match! NEW:{{{newBehaviour}}} ; OLD:{{{oldBehaviour}}}");
	}

	private static MethodInfo PopulateFriendlyVbMethod()
	{
		var method = typeof(Information)
				.GetMethod("OldVBFriendlyNameOfTypeName", BindingFlags.NonPublic | BindingFlags.Static);

		return method ?? throw new InvalidOperationException("Could not find VB compatability function");
	}
}