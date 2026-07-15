using System;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Then extensions for the unit <see cref="Suspicious" /> — the monadic bind.</summary>
public static class SuspiciousThenExtensions
{
	/// <summary>Chains a <paramref name="binder" /> that itself returns a <see cref="Suspicious" />; a failure short-circuits.</summary>
	/// <param name="source">The source.</param>
	/// <param name="binder">The binder.</param>
	/// <returns>The result of the <paramref name="binder" />, or this failed <see cref="Suspicious" /> unchanged.</returns>
	/// <remarks>The <paramref name="binder" /> runs on ANY success; a non-<c>ok</c> success kind is consumed (the binder's outcome wins).</remarks>
	public static Suspicious Then
	(
		this Suspicious source,
		Func<Suspicious> binder
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(binder);

		return source.IsFailure ? source : binder();
	}

	/// <summary>Chains a <paramref name="binder" /> that returns a <see cref="Suspicious{TValue}" />; a failure short-circuits.</summary>
	/// <param name="source">The source.</param>
	/// <param name="binder">The binder.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>The result of the <paramref name="binder" />, or a failed <see cref="Suspicious{TValue}" /> carrying this error.</returns>
	/// <remarks>The <paramref name="binder" /> runs on ANY success; a non-<c>ok</c> success kind is consumed (the binder's outcome wins).</remarks>
	public static Suspicious<TValue> Then<TValue>
	(
		this Suspicious source,
		Func<Suspicious<TValue>> binder
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(binder);

		return source.Error is { } error ? Suspicious<TValue>.CreateFailure(error) : binder();
	}
}