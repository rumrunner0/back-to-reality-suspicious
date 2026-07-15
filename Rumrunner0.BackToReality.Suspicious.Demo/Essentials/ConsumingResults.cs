namespace Rumrunner0.BackToReality.Suspicious.Demo.Essentials;

using System;
using Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Consuming results — <c>Match</c>, <c>Switch</c> and the safe value paths.</summary>
internal static class ConsumingResults
{
	/// <summary>Runs the example.</summary>
	internal static void Run()
	{
		var found = Suspicious.Ok("Roman");
		var failed = Suspicious.Fail<string>(Error.Invalid("Name is required"));

		// Match folds both rails into one value — the boundary tool.
		Console.WriteLine(Describe(found));
		Console.WriteLine(Describe(failed));

		// Switch is its side effect twin.
		failed.Switch
		(
			onValue: static name => Console.WriteLine($"Hello, {name}!"),
			onError: static e => Console.WriteLine($"Rejected: {e.Description}")
		);

		// The unit result folds the same way — there's just no value lane.
		Suspicious.Ok().Switch
		(
			onSuccess: static () => Console.WriteLine("Deleted"),
			onError: static e => Console.WriteLine($"Delete failed: {e.Description}")
		);

		// TryGetValue — the imperative escape hatch for loops and early returns.
		if (found.TryGetValue(out var name)) Console.WriteLine($"Extracted: {name}");

		// GetValueOr — when a genuine fallback exists and the error can be discarded.
		Console.WriteLine($"With fallback: {failed.GetValueOr("anonymous")}");

		// `Value` also exists but THROWS on a valueless result — a contract guard,
		// not control flow; the paths above never throw.
	}

	/// <summary>Folds a result into a display line.</summary>
	/// <param name="result">The result.</param>
	/// <returns>A display line.</returns>
	private static string Describe(Suspicious<string> result)
	{
		return result.Match
		(
			onValue: static name => $"Found: {name}",
			onError: static e => $"Failed: {e.Description}"
		);
	}
}