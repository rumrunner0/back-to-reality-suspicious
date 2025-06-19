using System.Collections.Generic;
using Rumrunner0.BackToReality.Suspicious.Results;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>
/// Factory for <see cref="Suspicious{TResult}" />.
/// </summary>
public static class Suspicious
{
	/// <summary><see cref="Success" /> wrapped in <see cref="Suspicious{TResult}" />.</summary>
	public static Suspicious<Success> Success { get; } = Suspicious<Success>.From(new Success());

	/// <summary><see cref="Ok" /> wrapped in <see cref="Suspicious{TResult}" />.</summary>
	public static Suspicious<Ok> Ok { get; } = Suspicious<Ok>.From(new Ok());

	/// <summary>Creates a new <see cref="Suspicious{TResult}" /> from a <paramref name="result" />.</summary>
	public static Suspicious<TResult> Result<TResult>(TResult result) =>  Suspicious<TResult>.From(result);

	/// <summary>
	/// Factory for an error <see cref="Suspicious{TResult}" />.
	/// </summary>
	public static class Not<TResult>
	{
		/// <summary>Creates a new <see cref="Suspicious{TResult}" /> from an <see cref="ErrorSet" /> parameters.</summary>
		public static Suspicious<TResult> But(string category, string header, IEnumerable<Error> errors) => But(new (ErrorSetCategory.Custom(category), header, errors));

		/// <summary>Creates a new <see cref="Suspicious{TResult}" /> from an <see cref="errorSet" />.</summary>
		public static Suspicious<TResult> But(ErrorSet errorSet) => Suspicious<TResult>.From(errorSet);
	}
}