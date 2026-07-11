namespace Rumrunner0.BackToReality.Suspicious.Demo.Advanced;

using System;
using Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Error triage — severity ordering, tree queries and enrichment for operations.</summary>
internal static class ErrorTriage
{
	/// <summary>Runs the example.</summary>
	internal static void Run()
	{
		// A nightly job ran three independent steps; two failed in different ways.
		var run = Suspicious.Combine
		(
			Suspicious.Ok(),
			Suspicious.Unavailable("Reporting warehouse timed out", exception: new TimeoutException("No response in 30 s")),
			Suspicious.Invalid("Currency table has 3 malformed rows")
		);

		if (run.IsSuccess)
		{
			Console.WriteLine("Nothing to triage");
			return;
		}

		// The aggregate escalated to the most critical child; codes make the alert policy a one-liner.
		var alert = run.Error.Kind >= OutcomeKind.Unavailable ? "PAGE the on-call" : "log and continue";
		Console.WriteLine($"{run.Error.Kind}: {alert}");

		// Targeted reactions query the tree instead of parsing messages. Find searches the
		// details BEFORE self, so even the kind the aggregate escalated to (unavailable)
		// resolves to the concrete child that carries the real description.
		if (run.Error.Find(OutcomeKind.Unavailable) is { } outage) Console.WriteLine($"Schedule a retry: {outage.Description}");
		if (run.Error.Find(OutcomeKind.Invalid) is { } dataIssue) Console.WriteLine($"File a data-quality ticket: {dataIssue.Description}");

		// MapError enriches at a layer boundary — the rails stay untouched.
		Console.WriteLine(run.MapError(static e => Error.Failure("Nightly refresh failed", cause: e)));
	}
}