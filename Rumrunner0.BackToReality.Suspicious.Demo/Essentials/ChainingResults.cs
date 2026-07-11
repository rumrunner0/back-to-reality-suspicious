namespace Rumrunner0.BackToReality.Suspicious.Demo.Essentials;

using System;
using Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Chaining — <c>Map</c> transforms the value, <c>Then</c> chains the next step, both short-circuit.</summary>
internal static class ChainingResults
{
	/// <summary>Runs the example.</summary>
	internal static void Run()
	{
		// The happy path runs every step.
		var greeting = ParseAge("30")
			.Then(CheckAdult)
			.Map(static age => $"Welcome, {age}!");

		Console.WriteLine(greeting);

		// A failure short-circuits — note that CheckAdult never announces itself below:
		// the parse error is carried through both steps untouched.
		var rejected = ParseAge("three")
			.Then(CheckAdult)
			.Map(static age => $"Welcome, {age}!");

		Console.WriteLine(rejected);

		// MapError rewrites the failure side; a success passes through untouched.
		Console.WriteLine(rejected.MapError(static e => Error.Failure("Signup failed", cause: e)));

		// AsUnit drops the value axis when only the outcome matters.
		Console.WriteLine(greeting.AsUnit());
	}

	/// <summary>Parses an age.</summary>
	/// <param name="text">The text to parse.</param>
	/// <returns>The age, or an <see cref="OutcomeKind.Invalid" /> failure.</returns>
	private static Suspicious<int> ParseAge(string text)
	{
		return int.TryParse(text, out var age) ? Suspicious.Ok(age) : Suspicious.Invalid<int>($"'{text}' is not a number");
	}

	/// <summary>Checks that an age is adult.</summary>
	/// <param name="age">The age.</param>
	/// <returns>The age, or an <see cref="OutcomeKind.Invalid" /> failure.</returns>
	private static Suspicious<int> CheckAdult(int age)
	{
		Console.WriteLine($"(CheckAdult runs for {age})");
		return age >= 18 ? Suspicious.Ok(age) : Suspicious.Invalid<int>("Must be at least 18");
	}
}