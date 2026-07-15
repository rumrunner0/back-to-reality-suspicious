using System;
using System.Diagnostics.CodeAnalysis;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Access extensions for <see cref="Suspicious{TValue}" /> — outcome queries and safe value paths.</summary>
public static class SuspiciousOfTValueAccessExtensions
{
	/// <summary>Determines whether the outcome equals the provided <paramref name="kind" />.</summary>
	/// <param name="source">The source.</param>
	/// <param name="kind">The kind.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns><c>true</c>, if the outcomes are equal; <c>false</c>, otherwise.</returns>
	public static bool Is<TValue>
	(
		this Suspicious<TValue> source,
		OutcomeKind kind
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(kind);

		return source.Outcome == kind;
	}

	/// <summary>Tries to get the value.</summary>
	/// <param name="source">The source.</param>
	/// <param name="value">The value, if present.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns><c>true</c>, if a value is present; <c>false</c>, otherwise.</returns>
	/// <remarks>The imperative access path — for loops and early returns. At boundaries where every rail must be handled, prefer the three-way <c>Match</c> or <c>Switch</c>.</remarks>
	public static bool TryGetValue<TValue>
	(
		this Suspicious<TValue> source,
		[MaybeNullWhen(false)] out TValue value
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);

		value = source.HasValue ? source.Value : default;
		return source.HasValue;
	}

	/// <summary>Gets the value, or the provided <paramref name="fallback" /> if no value is present.</summary>
	/// <param name="source">The source.</param>
	/// <param name="fallback">The fallback value.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>The value or the <paramref name="fallback" />.</returns>
	/// <remarks>For flows with a genuine fallback — the <see cref="Suspicious{TValue}.Error" /> is deliberately discarded. At boundaries where every rail must be handled, prefer the three-way <c>Match</c> or <c>Switch</c>.</remarks>
	public static TValue GetValueOr<TValue>
	(
		this Suspicious<TValue> source,
		TValue fallback
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		if (default(TValue) is null && fallback is null) throw new ArgumentNullException(nameof(fallback));

		return source.HasValue ? source.Value : fallback;
	}

	/// <summary>Gets the value, or the result of the provided <paramref name="fallbackFactory" /> if no value is present.</summary>
	/// <param name="source">The source.</param>
	/// <param name="fallbackFactory">The fallback factory; invoked only when no value is present, and must not produce <c>null</c>.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>The value or the result of the <paramref name="fallbackFactory" />.</returns>
	/// <remarks>For flows with a genuine fallback — the <see cref="Suspicious{TValue}.Error" /> is deliberately discarded. At boundaries where every rail must be handled, prefer the three-way <c>Match</c> or <c>Switch</c>.</remarks>
	/// <exception cref="ArgumentNullException">Thrown if the <paramref name="fallbackFactory" /> is <c>null</c>, or if it produces <c>null</c>.</exception>
	public static TValue GetValueOr<TValue>
	(
		this Suspicious<TValue> source,
		Func<TValue> fallbackFactory
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(fallbackFactory);
		if (source.HasValue) return source.Value;

		var fallback = fallbackFactory();
		if (default(TValue) is null && fallback is null) throw new ArgumentNullException(nameof(fallbackFactory), "The fallback factory produced null");

		return fallback;
	}
}