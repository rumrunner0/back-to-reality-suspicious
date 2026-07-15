namespace Rumrunner0.BackToReality.Suspicious.Demo.Essentials;

using System;
using Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Outcome kinds — the domain identity of a result, ordered by severity.</summary>
internal static class OutcomeKinds
{
	/// <summary>Runs the example.</summary>
	internal static void Run()
	{
		// The presets — "domain HTTP codes"; the code orders kinds by severity,
		// the side declares which rails a kind can be constructed on.
		OutcomeKind[] presets =
		[
			OutcomeKind.Ok,
			OutcomeKind.NoValue,
			OutcomeKind.Invalid,
			OutcomeKind.Conflict,
			OutcomeKind.Failure,
			OutcomeKind.Unavailable,
			OutcomeKind.Unexpected
		];

		foreach (var kind in presets) Console.WriteLine($"{kind,-19} side: {kind.Side}");

		// `Is` compares the outcome — one uniform question for either rail.
		var missing = Suspicious.NoValue<string>();
		Console.WriteLine($"Is no_value: {missing.Is(OutcomeKind.NoValue)}");

		// Severity comparisons make policies one-liners.
		Console.WriteLine($"unexpected outranks invalid: {OutcomeKind.Unexpected > OutcomeKind.Invalid}");

		// Failures print their kind too — `Error.*` mints them for either result type.
		Console.WriteLine(Suspicious.Fail(Error.Conflict("Entity already exists")));
		Console.WriteLine(Suspicious.Fail<string>(Error.Unavailable("Dependency timed out")));
	}
}