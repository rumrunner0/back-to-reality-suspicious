namespace Rumrunner0.BackToReality.Suspicious.Demo.Essentials;

using System;
using System.Collections.Generic;
using Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>The no_value kind — the same miss on either rail, producer's choice.</summary>
internal static class MissOnEitherRail
{
	/// <summary>Runs the example.</summary>
	internal static void Run()
	{
		var users = new Dictionary<Guid, string>();
		var missingId = Guid.NewGuid();

		// Absence is normal here — the success rail without a value.
		Suspicious<string> Find(Guid id)
		{
			return users.TryGetValue(id, out var name) ? Suspicious.Ok(name) : Suspicious.NoValue<string>();
		}

		// Absence is a failure here — the SAME outcome identity, failure rail; the explicit opt-in.
		Suspicious<string> Require(Guid id)
		{
			return users.TryGetValue(id, out var name) ? Suspicious.Ok(name) : Suspicious.Fail<string>(Error.NoValue($"User {id} is required"));
		}

		var successRail = Find(missingId);
		var failureRail = Require(missingId);

		Console.WriteLine(successRail);
		Console.WriteLine(failureRail);

		// Consumers ask about the miss uniformly — the rail doesn't matter for "was it a miss".
		Console.WriteLine($"Success rail: Is(NoValue) = {successRail.Is(OutcomeKind.NoValue)}, IsSuccess = {successRail.IsSuccess}");
		Console.WriteLine($"Failure rail: Is(NoValue) = {failureRail.Is(OutcomeKind.NoValue)}, IsSuccess = {failureRail.IsSuccess}");

		// The three-way Match is total — it has a lane for the valueless success.
		// (The two-way overload THROWS on it, by contract: use it only where a miss can't occur.)
		var text = successRail.Match
		(
			onValue: static name => $"Found {name}",
			onNoValue: static () => "No such user — and that's fine",
			onError: static e => $"Lookup failed: {e.Description}"
		);

		Console.WriteLine(text);
	}
}