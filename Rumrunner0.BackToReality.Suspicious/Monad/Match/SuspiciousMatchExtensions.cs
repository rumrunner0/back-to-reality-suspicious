using System;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Match extensions for the unit <see cref="Suspicious" /> — folding and switching on the rails.</summary>
public static class SuspiciousMatchExtensions
{
	/// <summary>Matches this <see cref="Suspicious" /> into a <typeparamref name="TResult" />.</summary>
	/// <param name="source">The source.</param>
	/// <param name="onSuccess">The handler for the success rail.</param>
	/// <param name="onError">The handler for the failure rail.</param>
	/// <typeparam name="TResult">The result type.</typeparam>
	/// <returns>The result of the invoked handler.</returns>
	public static TResult Match<TResult>
	(
		this Suspicious source,
		Func<TResult> onSuccess,
		Func<Error, TResult> onError
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onSuccess);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		return source.Error is { } error ? onError(error) : onSuccess();
	}

	/// <summary>Switches on this <see cref="Suspicious" />.</summary>
	/// <param name="source">The source.</param>
	/// <param name="onSuccess">The handler for the success rail.</param>
	/// <param name="onError">The handler for the failure rail.</param>
	public static void Switch
	(
		this Suspicious source,
		Action onSuccess,
		Action<Error> onError
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onSuccess);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		if (source.Error is { } error) onError(error); else onSuccess();
	}
}