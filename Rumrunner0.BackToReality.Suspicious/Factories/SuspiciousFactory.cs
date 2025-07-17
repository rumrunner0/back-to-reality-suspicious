using System.Collections.Generic;
using Rumrunner0.BackToReality.Suspicious.Monad;
using Rumrunner0.BackToReality.Suspicious.Results;

namespace Rumrunner0.BackToReality.Suspicious.Factories;

/// <summary>Factory for a <see cref="Suspicious{TResult}" />.</summary>
public static class Suspicious
{
	/// <summary>A <see cref="Success" /> wrapped in a <see cref="Suspicious{TResult}" />.</summary>
	public static Suspicious<Success> Success { get; } = Suspicious<Success>.From(new Success());

	/// <summary>An <see cref="Ok" /> wrapped in a <see cref="Suspicious{TResult}" />.</summary>
	public static Suspicious<Ok> Ok { get; } = Suspicious<Ok>.From(new Ok());

	/// <summary>Creates a new <see cref="Suspicious{TResult}" /> from a <paramref name="result" />.</summary>
	/// <param name="result">The result.</param>
	/// <typeparam name="TResult">The result type.</typeparam>
	/// <returns>A new <see cref="Suspicious{TResult}" /> created from the  <paramref name="result" />.</returns>
	public static Suspicious<TResult> Result<TResult>(TResult result) =>  Suspicious<TResult>.From(result);

	/// <summary>Factory for an error <see cref="Suspicious{TResult}" />.</summary>
	public static class Not<TResult>
	{
		/// <summary>Creates a <see cref="Suspicious{TResult}" /> from <see cref="ErrorCollection" /> parameters.</summary>
		/// <param name="category">The category.</param>
		/// <param name="header">The header.</param>
		/// <param name="errors">The <see cref="Error" />s.</param>
		/// <returns>A new <see cref="Suspicious{TResult}" /> created from an <see cref="ErrorCollection" />.</returns>
		public static Suspicious<TResult> But(ErrorCollectionCategory category, string header, params IEnumerable<Error> errors) => But(ErrorCollection.New(category, header, errors));

		/// <summary>Creates a <see cref="Suspicious{TResult}" /> from an <see cref="ErrorCollection" />.</summary>
		/// <param name="errorCollection">The <see cref="ErrorCollection" />.</param>
		/// <returns>A new <see cref="Suspicious{TResult}" /> created from the <see cref="ErrorCollection" />.</returns>
		public static Suspicious<TResult> But(ErrorCollection errorCollection) => Suspicious<TResult>.From(errorCollection);
	}
}