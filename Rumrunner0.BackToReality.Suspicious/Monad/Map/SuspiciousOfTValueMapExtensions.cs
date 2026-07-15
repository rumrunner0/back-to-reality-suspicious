using System;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Map extensions for <see cref="Suspicious{TValue}" /> — transforming the value and the error axes.</summary>
public static class SuspiciousOfTValueMapExtensions
{
	/// <summary>Maps the value into a <typeparamref name="TResult" />; valueless results are propagated unchanged.</summary>
	/// <param name="source">The source.</param>
	/// <param name="mapper">The mapper.</param>
	/// <typeparam name="TValue">The source value type.</typeparam>
	/// <typeparam name="TResult">The result value type.</typeparam>
	/// <returns>A new <see cref="Suspicious{TResult}" />.</returns>
	/// <remarks>A success with a value keeps its outcome (custom success kinds are preserved); a success without a value and a failure short-circuit.</remarks>
	public static Suspicious<TResult> Map<TValue, TResult>
	(
		this Suspicious<TValue> source,
		Func<TValue, TResult> mapper
	)
	where TValue : notnull
	where TResult : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(mapper);

		if (source.HasValue) return Suspicious<TResult>.CreateSuccess(source.Outcome, mapper(source.Value));
		if (source.Error is { } error) return Suspicious<TResult>.CreateFailure(error);
		return Suspicious<TResult>.CreateSuccess(source.Outcome);
	}

	/// <summary>Maps the <see cref="Suspicious{TValue}.Error" /> of a failure; a success is returned unchanged.</summary>
	/// <param name="source">The source.</param>
	/// <param name="mapper">The mapper.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A new <see cref="Suspicious{TValue}" /> with the mapped error, or this instance if it is a success.</returns>
	public static Suspicious<TValue> MapError<TValue>
	(
		this Suspicious<TValue> source,
		Func<Error, Error> mapper
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(mapper);

		return source.Error is { } error ? Suspicious<TValue>.CreateFailure(mapper(error)) : source;
	}
}