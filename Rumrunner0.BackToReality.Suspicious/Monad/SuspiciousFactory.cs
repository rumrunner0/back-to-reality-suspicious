using System.Collections.Generic;
using Rumrunner0.BackToReality.Suspicious.Results;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>
/// Factory for a <see cref="Suspicious{TResult}" />.
/// </summary>
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

	/// <summary>
	/// Factory for an error <see cref="Suspicious{TResult}" />.
	/// </summary>
	public static class Not<TResult>
	{
		/// <summary>Creates a <see cref="Suspicious{TResult}" /> from <see cref="ErrorSet" /> parameters.</summary>
		/// <param name="category">The category name.</param>
		/// <param name="header">The header.</param>
		/// <param name="errors">The <see cref="Error" />s.</param>
		/// <returns>A new <see cref="Suspicious{TResult}" /> created from an <see cref="ErrorSet" />.</returns>
		public static Suspicious<TResult> But(string category, string header, params IEnumerable<Error> errors) => But(new (ErrorSetCategory.Custom(category), header, errors));

		/// <summary>Creates a <see cref="Suspicious{TResult}" /> from an <see cref="ErrorSet" />.</summary>
		/// <param name="errorSet">The <see cref="ErrorSet" />.</param>
		/// <returns>A new <see cref="Suspicious{TResult}" /> created from the <see cref="ErrorSet" />.</returns>
		public static Suspicious<TResult> But(ErrorSet errorSet) => Suspicious<TResult>.From(errorSet);
	}
}