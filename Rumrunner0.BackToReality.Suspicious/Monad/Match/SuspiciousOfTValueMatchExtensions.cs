using System;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Match extensions for <see cref="Suspicious{TValue}" /> — folding and switching on the rails.</summary>
public static class SuspiciousOfTValueMatchExtensions
{
	/// <summary>Matches this <see cref="Suspicious{TValue}" /> into a <typeparamref name="TResult" />.</summary>
	/// <param name="source">The source.</param>
	/// <param name="onValue">The handler for a success with a value.</param>
	/// <param name="onError">The handler for the failure rail.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <typeparam name="TResult">The result type.</typeparam>
	/// <returns>The result of the invoked handler.</returns>
	/// <remarks>Use this overload only in flows where a success without a value can't occur; otherwise use the overload with an <c>onNoValue</c> handler.</remarks>
	/// <exception cref="InvalidOperationException">Thrown if this <see cref="Suspicious{TValue}" /> is a success without a value — a contract violation of this overload.</exception>
	public static TResult Match<TValue, TResult>
	(
		this Suspicious<TValue> source,
		Func<TValue, TResult> onValue,
		Func<Error, TResult> onError
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		if (source.HasValue) return onValue(source.Value);
		if (source.Error is { } error) return onError(error);

		throw new InvalidOperationException($"The {nameof(Suspicious<TValue>)} is a success without a value; use the {nameof(Match)} overload with an 'onNoValue' handler");
	}

	/// <summary>Matches this <see cref="Suspicious{TValue}" /> into a <typeparamref name="TResult" />.</summary>
	/// <param name="source">The source.</param>
	/// <param name="onValue">The handler for a success with a value.</param>
	/// <param name="onNoValue">The handler for a success without a value.</param>
	/// <param name="onError">The handler for the failure rail.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <typeparam name="TResult">The result type.</typeparam>
	/// <returns>The result of the invoked handler.</returns>
	public static TResult Match<TValue, TResult>
	(
		this Suspicious<TValue> source,
		Func<TValue, TResult> onValue,
		Func<TResult> onNoValue,
		Func<Error, TResult> onError
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onNoValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		if (source.HasValue) return onValue(source.Value);
		if (source.Error is { } error) return onError(error);
		return onNoValue();
	}

	/// <summary>Switches on this <see cref="Suspicious{TValue}" />.</summary>
	/// <param name="source">The source.</param>
	/// <param name="onValue">The handler for a success with a value.</param>
	/// <param name="onError">The handler for the failure rail.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <remarks>Use this overload only in flows where a success without a value can't occur; otherwise use the overload with an <c>onNoValue</c> handler.</remarks>
	/// <exception cref="InvalidOperationException">Thrown if this <see cref="Suspicious{TValue}" /> is a success without a value — a contract violation of this overload.</exception>
	public static void Switch<TValue>
	(
		this Suspicious<TValue> source,
		Action<TValue> onValue,
		Action<Error> onError
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		if (source.HasValue) onValue(source.Value);
		else if (source.Error is { } error) onError(error);
		else throw new InvalidOperationException($"The {nameof(Suspicious<TValue>)} is a success without a value; use the {nameof(Switch)} overload with an 'onNoValue' handler");
	}

	/// <summary>Switches on this <see cref="Suspicious{TValue}" />.</summary>
	/// <param name="source">The source.</param>
	/// <param name="onValue">The handler for a success with a value.</param>
	/// <param name="onNoValue">The handler for a success without a value.</param>
	/// <param name="onError">The handler for the failure rail.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	public static void Switch<TValue>
	(
		this Suspicious<TValue> source,
		Action<TValue> onValue,
		Action onNoValue,
		Action<Error> onError
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onNoValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		if (source.HasValue) onValue(source.Value);
		else if (source.Error is { } error) onError(error);
		else onNoValue();
	}
}