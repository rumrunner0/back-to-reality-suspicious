namespace Rumrunner0.BackToReality.Suspicious.Demo.Essentials;

using System;
using Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Creating results — factories and implicit conversions.</summary>
internal static class CreatingResults
{
	/// <summary>Runs the example.</summary>
	internal static void Run()
	{
		// The unit result — the outcome of a void-like operation.
		Console.WriteLine(Suspicious.Ok());

		// A value result — the outcome is `ok`.
		Console.WriteLine(Suspicious.Ok(42));

		// The implicit conversion from a value does the same.
		Suspicious<int> converted = 42;
		Console.WriteLine(converted);

		// A failure carries exactly one error: `Error.*` mints the error, `Fail` lifts it into a result...
		Console.WriteLine(Suspicious.Fail<int>(Error.Invalid("Age must be positive")));

		// ...and the implicit conversion from an error mirrors the value one.
		Suspicious<int> fromError = Error.Conflict("Age is already locked");
		Console.WriteLine(fromError);

		// Together, the conversions keep producers linear: value out, error out.
		Console.WriteLine(ParseAge("30"));
		Console.WriteLine(ParseAge("-3"));
	}

	/// <summary>Parses an age.</summary>
	/// <param name="text">The text to parse.</param>
	/// <returns>The age, or an <see cref="OutcomeKind.Invalid" /> failure.</returns>
	private static Suspicious<int> ParseAge(string text)
	{
		if (!int.TryParse(text, out var age)) return Error.Invalid($"'{text}' is not a number");
		if (age is < 0 or > 150) return Error.Invalid($"Age {age} is out of range");

		return age;
	}
}