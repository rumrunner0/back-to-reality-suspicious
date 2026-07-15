namespace Rumrunner0.BackToReality.Suspicious.Demo.Essentials;

using System;
using Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Combining — gathering independent checks into one result.</summary>
internal static class CombiningResults
{
	/// <summary>Runs the example.</summary>
	internal static void Run()
	{
		// All successes fold to Ok.
		Console.WriteLine(Suspicious.Combine(ValidateName("Roman"), ValidateAge(30)));

		// A single failure is carried as-is.
		Console.WriteLine(Suspicious.Combine(ValidateName("Roman"), ValidateAge(-5)));

		// Multiple failures aggregate into one error; the kind escalates to the most critical child.
		var validation = Suspicious.Combine
		(
			ValidateName(null),
			ValidateAge(-5),
			Error.Unexpected("Validator crashed")
		);

		Console.WriteLine($"Escalated to: {validation.Outcome}");

		// The error tree is queryable — Details (recursively) and the Cause chain.
		if (validation.IsFailure)
		{
			Console.WriteLine($"Contains invalid: {validation.Error.Contains(OutcomeKind.Invalid)}");
			if (validation.Error.Find(OutcomeKind.Invalid) is { } invalid) Console.WriteLine($"First invalid: {invalid.Description}");
		}

		// Find/Contains THROW on a kind that can never appear in an error (e.g. `ok`) —
		// the query would be always-false, so asking is API misuse.
	}

	/// <summary>Validates a name.</summary>
	/// <param name="name">The name.</param>
	/// <returns>An <c>ok</c> result, or an <see cref="OutcomeKind.Invalid" /> failure.</returns>
	private static Suspicious ValidateName(string? name)
	{
		if (string.IsNullOrWhiteSpace(name)) return Error.Invalid("Name is required");
		return Suspicious.Ok();
	}

	/// <summary>Validates an age.</summary>
	/// <param name="age">The age.</param>
	/// <returns>An <c>ok</c> result, or an <see cref="OutcomeKind.Invalid" /> failure.</returns>
	private static Suspicious ValidateAge(int age)
	{
		if (age is < 0 or > 150) return Error.Invalid($"Age {age} is out of range");
		return Suspicious.Ok();
	}
}