namespace Rumrunner0.BackToReality.Suspicious.Demo.Advanced;

using System;
using System.Collections.Generic;
using System.Linq;
using Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>An any-side custom kind — one domain fact, two rails, policy per producer.</summary>
/// <remarks>Mirrors the built-in no_value: the kind is rail-agnostic, each producer picks the rail its policy dictates.</remarks>
internal static class PartialImport
{
	/// <summary>Partial outcome — some records imported, some rejected.</summary>
	private static readonly OutcomeKind _partial = OutcomeKind.Custom("partial", code: 150, OutcomeSide.Any);

	/// <summary>Runs the example.</summary>
	internal static void Run()
	{
		string[] records = ["alpha", "beta", "", "gamma", ""];

		// Lenient policy — a partial import is still useful: success rail, value attached.
		var lenient = ImportLenient(records);
		Console.WriteLine(lenient);
		Console.WriteLine(ToResponse(lenient));

		// Strict policy — the SAME domain fact rides the failure rail.
		var strict = ImportStrict(records);
		Console.WriteLine(strict);
		Console.WriteLine(ToResponse(strict));

		// A clean batch is plain Ok under either policy.
		Console.WriteLine(ToResponse(ImportStrict(["alpha", "beta"])));
	}

	/// <summary>Imports records; a partial import is acceptable.</summary>
	/// <param name="records">The records.</param>
	/// <returns>An import summary on the success rail, even when partial.</returns>
	private static Suspicious<ImportSummary> ImportLenient(IReadOnlyList<string> records)
	{
		var imported = records.Count(static r => !string.IsNullOrWhiteSpace(r));
		var rejected = records.Count - imported;

		if (imported == 0) return Suspicious.Failure<ImportSummary>("All records were rejected");
		if (rejected == 0) return new ImportSummary(imported, rejected);

		return Suspicious.Success(_partial, new ImportSummary(imported, rejected));
	}

	/// <summary>Imports records; anything less than 100% is a failure.</summary>
	/// <param name="records">The records.</param>
	/// <returns>An import summary, or a <c>partial</c> failure.</returns>
	private static Suspicious<ImportSummary> ImportStrict(IReadOnlyList<string> records)
	{
		var imported = records.Count(static r => !string.IsNullOrWhiteSpace(r));
		var rejected = records.Count - imported;

		if (rejected == 0) return new ImportSummary(imported, rejected);

		return Error.Custom(_partial, $"{rejected} of {records.Count} records were rejected");
	}

	/// <summary>Folds an import result into an HTTP-ish response line.</summary>
	/// <param name="result">The result.</param>
	/// <returns>A response line.</returns>
	private static string ToResponse(Suspicious<ImportSummary> result)
	{
		// Kind first — the policy is rail-agnostic; the rail only changes the detail available.
		if (result.Is(_partial))
		{
			return result.Match
			(
				onValue: static s => $"206 Partial Content — {s.Imported} imported, {s.Rejected} rejected",
				onError: static e => $"206 Partial Content — {e.Description}"
			);
		}

		// If a producer ever returns a VALUELESS partial success — Success<T>(kind) without a value —
		// the two-way Match here starts throwing by contract; move to the three-way overloads then.
		return result.Match
		(
			onValue: static s => $"200 OK — {s.Imported} imported",
			onError: static e => $"500 Internal Server Error — {e.Description}"
		);
	}
}

/// <summary>Summary of an import.</summary>
/// <param name="Imported">The count of imported records.</param>
/// <param name="Rejected">The count of rejected records.</param>
internal sealed record class ImportSummary(int Imported, int Rejected);