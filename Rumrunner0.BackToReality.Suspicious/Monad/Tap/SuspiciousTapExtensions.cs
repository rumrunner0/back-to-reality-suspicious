using System;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Tap extensions for the unit <see cref="Suspicious" /> — chain-preserving side effects.</summary>
public static class SuspiciousTapExtensions
{
	/// <summary>Runs an <paramref name="effect" /> against a success; this <see cref="Suspicious" /> flows through unchanged.</summary>
	/// <param name="source">The source.</param>
	/// <param name="effect">The effect.</param>
	/// <returns>This <see cref="Suspicious" />.</returns>
	/// <remarks>The <paramref name="effect" /> runs on ANY success.</remarks>
	public static Suspicious Tap
	(
		this Suspicious source,
		Action effect
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(effect);
		if (source.IsSuccess) effect();

		return source;
	}

	/// <summary>Runs a result-returning <paramref name="effect" /> against a success; its failure REPLACES this result, its success is discarded.</summary>
	/// <param name="source">The source.</param>
	/// <param name="effect">The effect; must not produce <c>null</c>.</param>
	/// <returns>The failed result of the <paramref name="effect" />, or this <see cref="Suspicious" /> unchanged.</returns>
	/// <remarks>The <paramref name="effect" /> runs on ANY success; only its failure rail matters — the kind of its success is discarded and this instance flows through, so the original success kind is PRESERVED.</remarks>
	/// <exception cref="ArgumentNullException">Thrown if the <paramref name="effect" /> is <c>null</c>, or if it produces <c>null</c>.</exception>
	public static Suspicious Tap
	(
		this Suspicious source,
		Func<Suspicious> effect
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(effect);
		if (source.IsFailure) return source;

		var result = effect();
		if (result is null) throw new ArgumentNullException(nameof(effect), "The effect produced null");

		return result.IsFailure ? result : source;
	}

	/// <summary>Runs an <paramref name="effect" /> against the error of a failure; this <see cref="Suspicious" /> flows through unchanged.</summary>
	/// <param name="source">The source.</param>
	/// <param name="effect">The effect.</param>
	/// <returns>This <see cref="Suspicious" />.</returns>
	public static Suspicious TapError
	(
		this Suspicious source,
		Action<Error> effect
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(effect);
		if (source.Error is { } error) effect(error);

		return source;
	}
}