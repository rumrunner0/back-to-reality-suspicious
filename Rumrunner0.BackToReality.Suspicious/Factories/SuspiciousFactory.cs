using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rumrunner0.BackToReality.Suspicious.Monad;
using Rumrunner0.BackToReality.Suspicious.Results;

namespace Rumrunner0.BackToReality.Suspicious.Factories;

/// <summary><see cref="Suspicious{TValue}" /> factory.</summary>
public static class Suspicious
{
	/// <summary>A <see cref="Success" /> wrapped in a <see cref="Suspicious{TValue}" />.</summary>
	public static Suspicious<Success> Success { get; } = Value(new Success());

	/// <summary>An <see cref="Ok" /> wrapped in a <see cref="Suspicious{TValue}" />.</summary>
	public static Suspicious<Ok> Ok { get; } = Value(new Ok());

	/// <summary>Creates a new <see cref="Suspicious{TValue}" /> from a <paramref name="value" />.</summary>
	/// <param name="value">The value.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A new <see cref="Suspicious{TValue}" /> created from the <paramref name="value" />.</returns>
	public static Suspicious<TValue> Value<TValue>(TValue value) where TValue : notnull => Suspicious<TValue>.From(value);

	/// <summary>Creates a <see cref="Suspicious{TValue}" /> from <see cref="ErrorSet" /> parameters.</summary>
	/// <param name="errors">The errors.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A new <see cref="Suspicious{TValue}" /> created from an <see cref="ErrorSet" />.</returns>
	public static Suspicious<TValue> Not<TValue>(params IEnumerable<Error> errors) where TValue : notnull => Not<TValue>(ErrorSet.New(errors));

	/// <summary>Creates a <see cref="Suspicious{TValue}" /> from <see cref="ErrorSet" /> parameters.</summary>
	/// <param name="errorSet">The <see cref="ErrorSet" />.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A new <see cref="Suspicious{TValue}" /> created from the <see cref="ErrorSet" />.</returns>
	public static Suspicious<TValue> Not<TValue>(ErrorSet errorSet) where TValue : notnull => Suspicious<TValue>.From(errorSet);
}