using System;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Then extensions for <see cref="Suspicious{TValue}" /> — the monadic bind.</summary>
public static class SuspiciousOfTValueThenExtensions
{
	/// <summary>Chains a <paramref name="binder" /> that itself returns a <see cref="Suspicious{TResult}" />; valueless results short-circuit.</summary>
	/// <param name="source">The source.</param>
	/// <param name="binder">The binder.</param>
	/// <typeparam name="TValue">The source value type.</typeparam>
	/// <typeparam name="TResult">The result value type.</typeparam>
	/// <returns>The result of the <paramref name="binder" />, or a propagated valueless result.</returns>
	/// <remarks>The <paramref name="binder" /> runs ONLY when a value is present; both a success without a value and a failure are propagated unchanged (fail-fast).</remarks>
	public static Suspicious<TResult> Then<TValue, TResult>
	(
		this Suspicious<TValue> source,
		Func<TValue, Suspicious<TResult>> binder
	)
	where TValue : notnull
	where TResult : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(binder);

		if (source.HasValue) return binder(source.Value);
		if (source.Error is { } error) return Suspicious<TResult>.CreateFailure(error);
		return Suspicious<TResult>.CreateSuccess(source.Outcome);
	}

	/// <summary>Chains a <paramref name="binder" /> that returns a unit <see cref="Suspicious" />; valueless results short-circuit.</summary>
	/// <param name="source">The source.</param>
	/// <param name="binder">The binder.</param>
	/// <typeparam name="TValue">The source value type.</typeparam>
	/// <returns>The result of the <paramref name="binder" />, or a propagated valueless result.</returns>
	/// <remarks>The <paramref name="binder" /> runs ONLY when a value is present; both a success without a value and a failure are propagated unchanged (fail-fast).</remarks>
	public static Suspicious Then<TValue>
	(
		this Suspicious<TValue> source,
		Func<TValue, Suspicious> binder
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(binder);

		if (source.HasValue) return binder(source.Value);
		if (source.Error is { } error) return Suspicious.Fail(error);
		return Suspicious.Success(source.Outcome);
	}
}