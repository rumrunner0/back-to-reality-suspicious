using System;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Tap extensions for <see cref="Suspicious{TValue}" /> — chain-preserving side effects.</summary>
public static class SuspiciousOfTValueTapExtensions
{
	/// <summary>Runs an <paramref name="effect" /> against the value; this <see cref="Suspicious{TValue}" /> flows through unchanged.</summary>
	/// <param name="source">The source.</param>
	/// <param name="effect">The effect.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>This <see cref="Suspicious{TValue}" />.</returns>
	/// <remarks>The <paramref name="effect" /> runs ONLY when a value is present; a valueless success and a failure skip it.</remarks>
	public static Suspicious<TValue> Tap<TValue>
	(
		this Suspicious<TValue> source,
		Action<TValue> effect
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(effect);
		if (source.HasValue) effect(source.Value);

		return source;
	}

	/// <summary>Runs a result-returning <paramref name="effect" /> against the value; its failure REPLACES this result, its success is discarded.</summary>
	/// <param name="source">The source.</param>
	/// <param name="effect">The effect; must not produce <c>null</c>.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A failed <see cref="Suspicious{TValue}" /> carrying the effect's error, or this <see cref="Suspicious{TValue}" /> unchanged.</returns>
	/// <remarks>The <paramref name="effect" /> runs ONLY when a value is present; only its failure rail matters — on its success this instance flows through, so the success kind is PRESERVED (unlike <c>Then</c>, which normalizes it).</remarks>
	/// <exception cref="ArgumentNullException">Thrown if the <paramref name="effect" /> is <c>null</c>, or if it produces <c>null</c>.</exception>
	public static Suspicious<TValue> Tap<TValue>
	(
		this Suspicious<TValue> source,
		Func<TValue, Suspicious> effect
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(effect);
		if (!source.HasValue) return source;

		var result = effect(source.Value);
		if (result is null) throw new ArgumentNullException(nameof(effect), "The effect produced null");

		return result.IsFailure ? result.AsFailure<TValue>() : source;
	}

	/// <summary>Runs an <paramref name="effect" /> against the <see cref="Suspicious{TValue}.Error" /> of a failure; this <see cref="Suspicious{TValue}" /> flows through unchanged.</summary>
	/// <param name="source">The source.</param>
	/// <param name="effect">The effect.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>This <see cref="Suspicious{TValue}" />.</returns>
	public static Suspicious<TValue> TapError<TValue>
	(
		this Suspicious<TValue> source,
		Action<Error> effect
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(effect);
		if (source.Error is { } error) effect(error);

		return source;
	}
}