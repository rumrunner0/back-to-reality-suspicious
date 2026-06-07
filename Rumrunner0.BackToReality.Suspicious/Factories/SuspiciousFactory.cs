using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

	/// <summary>Creates a <see cref="Suspicious{TValue}" /> from <see cref="ErrorSet" /> parameters.</summary>
	/// <param name="member">The member.</param>
	/// <param name="filePath">The file path.</param>
	/// <param name="line">The line.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A new <see cref="Suspicious{TValue}" /> created from an <see cref="ErrorSet" />.</returns>
	public static Suspicious<TValue> Not<TValue>
	(
		[CallerMemberName] string member = "",
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int line = 0
	)
	where TValue : notnull
	{
		return Not<TValue>(ErrorSet.Empty(member, filePath, line));
	}

	/// <summary>Creates a <see cref="Suspicious{TValue}" /> from <see cref="ErrorSet" /> parameters.</summary>
	/// <param name="error">The error.</param>
	/// <param name="member">The member.</param>
	/// <param name="filePath">The file path.</param>
	/// <param name="line">The line.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A new <see cref="Suspicious{TValue}" /> created from an <see cref="ErrorSet" />.</returns>
	public static Suspicious<TValue> Not<TValue>
	(
		Error error,
		[CallerMemberName] string member = "",
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int line = 0
	)
	where TValue : notnull
	{
		return Not<TValue>(ErrorSet.New([error], member, filePath, line));
	}

	/// <summary>Creates a <see cref="Suspicious{TValue}" /> from <see cref="ErrorSet" /> parameters.</summary>
	/// <param name="errors">The errors.</param>
	/// <param name="member">The member.</param>
	/// <param name="filePath">The file path.</param>
	/// <param name="line">The line.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A new <see cref="Suspicious{TValue}" /> created from an <see cref="ErrorSet" />.</returns>
	public static Suspicious<TValue> Not<TValue>
	(
		IEnumerable<Error> errors,
		[CallerMemberName] string member = "",
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int line = 0
	)
	where TValue : notnull
	{
		return Not<TValue>(ErrorSet.New(errors, member, filePath, line));
	}

	/// <summary>Creates a <see cref="Suspicious{TValue}" /> from <see cref="ErrorSet" /> parameters.</summary>
	/// <param name="errorSet">The <see cref="ErrorSet" />.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A new <see cref="Suspicious{TValue}" /> created from the <see cref="ErrorSet" />.</returns>
	public static Suspicious<TValue> Not<TValue>(ErrorSet errorSet) where TValue : notnull
	{
		return Suspicious<TValue>.From(errorSet);
	}
}