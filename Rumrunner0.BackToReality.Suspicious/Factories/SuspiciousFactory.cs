using System.Collections.Generic;
using Rumrunner0.BackToReality.Suspicious.Monad;
using Rumrunner0.BackToReality.Suspicious.Results;

namespace Rumrunner0.BackToReality.Suspicious.Factories;

/// <summary>Factory for a <see cref="Suspicious{TValue}" />.</summary>
public static class Suspicious
{
	/// <summary>A <see cref="Success" /> wrapped in a <see cref="Suspicious{TValue}" />.</summary>
	public static Suspicious<Success> Success { get; } = Suspicious<Success>.From(new Success());

	/// <summary>An <see cref="Ok" /> wrapped in a <see cref="Suspicious{TValue}" />.</summary>
	public static Suspicious<Ok> Ok { get; } = Suspicious<Ok>.From(new Ok());

	/// <summary>Creates a new <see cref="Suspicious{TValue}" /> from a <paramref name="value" />.</summary>
	/// <param name="value">The value.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A new <see cref="Suspicious{TValue}" /> created from the <paramref name="value" />.</returns>
	public static Suspicious<TValue> Value<TValue>(TValue value) where TValue : notnull => Suspicious<TValue>.From(value);

	/// <summary>Factory for an error <see cref="Suspicious{TValue}" />.</summary>
	public static class Not<TValue> where TValue : notnull
	{
		/// <summary>Creates a <see cref="Suspicious{TValue}" /> from <see cref="ErrorSet" /> parameters.</summary>
		/// <param name="category">The category.</param>
		/// <param name="header">The header.</param>
		/// <param name="errors">The <see cref="Error" />s.</param>
		/// <returns>A new <see cref="Suspicious{TValue}" /> created from an <see cref="ErrorSet" />.</returns>
		public static Suspicious<TValue> But(ErrorSetCategory category, string header, params IEnumerable<Error> errors) => But(ErrorSet.New(category, header, errors));

		/// <summary>Creates a <see cref="Suspicious{TValue}" /> from an <see cref="ErrorSet" />.</summary>
		/// <param name="errorSet">The <see cref="ErrorSet" />.</param>
		/// <returns>A new <see cref="Suspicious{TValue}" /> created from the <see cref="ErrorSet" />.</returns>
		public static Suspicious<TValue> But(ErrorSet errorSet) => Suspicious<TValue>.From(errorSet);
	}
}