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
	public static Suspicious<TValue> Value<TValue>(TValue value) =>  Suspicious<TValue>.From(value);

	/// <summary>Factory for an error <see cref="Suspicious{TValue}" />.</summary>
	public static class Not<TValue>
	{
		/// <summary>Creates a <see cref="Suspicious{TValue}" /> from <see cref="ErrorCollection" /> parameters.</summary>
		/// <param name="category">The category.</param>
		/// <param name="header">The header.</param>
		/// <param name="errors">The <see cref="Error" />s.</param>
		/// <returns>A new <see cref="Suspicious{TValue}" /> created from an <see cref="ErrorCollection" />.</returns>
		public static Suspicious<TValue> But(ErrorCollectionCategory category, string header, params IEnumerable<Error> errors) => But(ErrorCollection.New(category, header, errors));

		/// <summary>Creates a <see cref="Suspicious{TValue}" /> from an <see cref="ErrorCollection" />.</summary>
		/// <param name="errorCollection">The <see cref="ErrorCollection" />.</param>
		/// <returns>A new <see cref="Suspicious{TValue}" /> created from the <see cref="ErrorCollection" />.</returns>
		public static Suspicious<TValue> But(ErrorCollection errorCollection) => Suspicious<TValue>.From(errorCollection);
	}
}