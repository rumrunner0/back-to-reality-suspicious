using System;
using System.Threading;
using System.Threading.Tasks;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Async Tap extensions for <see cref="Suspicious{TValue}" /> and <c>Task</c>-wrapped generic results.</summary>
public static class SuspiciousOfTValueAsyncTapExtensions
{
	/// <summary>Runs an async <paramref name="effect" /> against the value; the source flows through unchanged.</summary>
	/// <param name="source">The source.</param>
	/// <param name="effect">The effect; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before the <paramref name="effect" /> runs.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with this <see cref="Suspicious{TValue}" />.</returns>
	/// <remarks>The <paramref name="effect" /> runs ONLY when a value is present; a valueless success and a failure skip it.</remarks>
	/// <exception cref="ArgumentNullException">Thrown if the <paramref name="effect" /> is <c>null</c> (synchronously), or if it produces <c>null</c> (in the returned task).</exception>
	public static Task<Suspicious<TValue>> Tap<TValue>
	(
		this Suspicious<TValue> source,
		Func<TValue, Task> effect,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(effect);

		if (!source.HasValue) return Task.FromResult(source);
		return Core(source, effect, cancellationToken);

		static async Task<Suspicious<TValue>> Core(Suspicious<TValue> source, Func<TValue, Task> effect, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var task = effect(source.Value);
			if (task is null) throw new ArgumentNullException(nameof(effect), "The effect produced null");

			await task.ConfigureAwait(false);
			return source;
		}
	}

	/// <summary>Runs an async token-receiving <paramref name="effect" /> against the value; the source flows through unchanged.</summary>
	/// <param name="source">The source.</param>
	/// <param name="effect">The effect; receives the <paramref name="cancellationToken" />; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before the <paramref name="effect" /> runs and passed into it.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with this <see cref="Suspicious{TValue}" />.</returns>
	public static Task<Suspicious<TValue>> Tap<TValue>
	(
		this Suspicious<TValue> source,
		Func<TValue, CancellationToken, Task> effect,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(effect);
		return source.Tap(value => effect(value, cancellationToken), cancellationToken);
	}

	/// <summary>Runs an async result-returning <paramref name="effect" /> against the value; its failure REPLACES the result, its success is discarded.</summary>
	/// <param name="source">The source.</param>
	/// <param name="effect">The effect; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before the <paramref name="effect" /> runs.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with a failed <see cref="Suspicious{TValue}" /> carrying the effect's error, or this <see cref="Suspicious{TValue}" /> unchanged.</returns>
	/// <remarks>The <paramref name="effect" /> runs ONLY when a value is present; on its success this instance flows through, so the success kind is PRESERVED.</remarks>
	/// <exception cref="ArgumentNullException">Thrown if the <paramref name="effect" /> is <c>null</c> (synchronously), or if it produces <c>null</c> (in the returned task).</exception>
	public static Task<Suspicious<TValue>> Tap<TValue>
	(
		this Suspicious<TValue> source,
		Func<TValue, Task<Suspicious>> effect,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(effect);

		if (!source.HasValue) return Task.FromResult(source);
		return Core(source, effect, cancellationToken);

		static async Task<Suspicious<TValue>> Core(Suspicious<TValue> source, Func<TValue, Task<Suspicious>> effect, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var task = effect(source.Value);
			if (task is null) throw new ArgumentNullException(nameof(effect), "The effect produced null");

			var result = await task.ConfigureAwait(false);
			if (result is null) throw new ArgumentNullException(nameof(effect), "The effect produced null");

			return result.IsFailure ? result.AsFailure<TValue>() : source;
		}
	}

	/// <summary>Runs an async token-receiving result-returning <paramref name="effect" /> against the value; its failure REPLACES the result, its success is discarded.</summary>
	/// <param name="source">The source.</param>
	/// <param name="effect">The effect; receives the <paramref name="cancellationToken" />; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before the <paramref name="effect" /> runs and passed into it.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with a failed <see cref="Suspicious{TValue}" /> carrying the effect's error, or this <see cref="Suspicious{TValue}" /> unchanged.</returns>
	public static Task<Suspicious<TValue>> Tap<TValue>
	(
		this Suspicious<TValue> source,
		Func<TValue, CancellationToken, Task<Suspicious>> effect,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(effect);
		return source.Tap(value => effect(value, cancellationToken), cancellationToken);
	}

	/// <summary>Runs an async <paramref name="effect" /> against the error of a failure; the source flows through unchanged.</summary>
	/// <param name="source">The source.</param>
	/// <param name="effect">The effect; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before the <paramref name="effect" /> runs.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with this <see cref="Suspicious{TValue}" />.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the <paramref name="effect" /> is <c>null</c> (synchronously), or if it produces <c>null</c> (in the returned task).</exception>
	public static Task<Suspicious<TValue>> TapError<TValue>
	(
		this Suspicious<TValue> source,
		Func<Error, Task> effect,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(effect);

		if (source.Error is not { } error) return Task.FromResult(source);
		return Core(source, error, effect, cancellationToken);

		static async Task<Suspicious<TValue>> Core(Suspicious<TValue> source, Error error, Func<Error, Task> effect, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var task = effect(error);
			if (task is null) throw new ArgumentNullException(nameof(effect), "The effect produced null");

			await task.ConfigureAwait(false);
			return source;
		}
	}

	/// <summary>Runs an async token-receiving <paramref name="effect" /> against the error of a failure; the source flows through unchanged.</summary>
	/// <param name="source">The source.</param>
	/// <param name="effect">The effect; receives the <paramref name="cancellationToken" />; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before the <paramref name="effect" /> runs and passed into it.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with this <see cref="Suspicious{TValue}" />.</returns>
	public static Task<Suspicious<TValue>> TapError<TValue>
	(
		this Suspicious<TValue> source,
		Func<Error, CancellationToken, Task> effect,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(effect);
		return source.TapError(error => effect(error, cancellationToken), cancellationToken);
	}

	/// <summary>Awaits the <paramref name="source" /> and runs a sync <paramref name="effect" /> against the value; the result flows through unchanged.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="effect">The effect.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with the unchanged result.</returns>
	public static Task<Suspicious<TValue>> Tap<TValue>
	(
		this Task<Suspicious<TValue>> source,
		Action<TValue> effect,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(effect);

		return Core(source, effect, cancellationToken);

		static async Task<Suspicious<TValue>> Core(Task<Suspicious<TValue>> source, Action<TValue> effect, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();

			return result.Tap(effect);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and runs a sync result-returning <paramref name="effect" /> against the value; its failure REPLACES the result.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="effect">The effect; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with a failed <see cref="Suspicious{TValue}" /> carrying the effect's error, or the unchanged result.</returns>
	public static Task<Suspicious<TValue>> Tap<TValue>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, Suspicious> effect,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(effect);

		return Core(source, effect, cancellationToken);

		static async Task<Suspicious<TValue>> Core(Task<Suspicious<TValue>> source, Func<TValue, Suspicious> effect, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();

			return result.Tap(effect);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and runs an async <paramref name="effect" /> against the value; the result flows through unchanged.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="effect">The effect; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes and before the <paramref name="effect" /> runs.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with the unchanged result.</returns>
	public static Task<Suspicious<TValue>> Tap<TValue>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, Task> effect,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(effect);

		return Core(source, effect, cancellationToken);

		static async Task<Suspicious<TValue>> Core(Task<Suspicious<TValue>> source, Func<TValue, Task> effect, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			return await result.Tap(effect, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and runs an async token-receiving <paramref name="effect" /> against the value; the result flows through unchanged.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="effect">The effect; receives the <paramref name="cancellationToken" />; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes, then before the <paramref name="effect" /> runs and passed into it.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with the unchanged result.</returns>
	public static Task<Suspicious<TValue>> Tap<TValue>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, CancellationToken, Task> effect,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(effect);
		return source.Tap(value => effect(value, cancellationToken), cancellationToken);
	}

	/// <summary>Awaits the <paramref name="source" /> and runs an async result-returning <paramref name="effect" /> against the value; its failure REPLACES the result.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="effect">The effect; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes and before the <paramref name="effect" /> runs.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with a failed <see cref="Suspicious{TValue}" /> carrying the effect's error, or the unchanged result.</returns>
	public static Task<Suspicious<TValue>> Tap<TValue>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, Task<Suspicious>> effect,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(effect);

		return Core(source, effect, cancellationToken);

		static async Task<Suspicious<TValue>> Core(Task<Suspicious<TValue>> source, Func<TValue, Task<Suspicious>> effect, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			return await result.Tap(effect, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and runs an async token-receiving result-returning <paramref name="effect" /> against the value; its failure REPLACES the result.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="effect">The effect; receives the <paramref name="cancellationToken" />; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes, then before the <paramref name="effect" /> runs and passed into it.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with a failed <see cref="Suspicious{TValue}" /> carrying the effect's error, or the unchanged result.</returns>
	public static Task<Suspicious<TValue>> Tap<TValue>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, CancellationToken, Task<Suspicious>> effect,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(effect);
		return source.Tap(value => effect(value, cancellationToken), cancellationToken);
	}

	/// <summary>Awaits the <paramref name="source" /> and runs a sync <paramref name="effect" /> against the error of a failure; the result flows through unchanged.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="effect">The effect.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with the unchanged result.</returns>
	public static Task<Suspicious<TValue>> TapError<TValue>
	(
		this Task<Suspicious<TValue>> source,
		Action<Error> effect,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(effect);

		return Core(source, effect, cancellationToken);

		static async Task<Suspicious<TValue>> Core(Task<Suspicious<TValue>> source, Action<Error> effect, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();

			return result.TapError(effect);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and runs an async <paramref name="effect" /> against the error of a failure; the result flows through unchanged.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="effect">The effect; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes and before the <paramref name="effect" /> runs.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with the unchanged result.</returns>
	public static Task<Suspicious<TValue>> TapError<TValue>
	(
		this Task<Suspicious<TValue>> source,
		Func<Error, Task> effect,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(effect);

		return Core(source, effect, cancellationToken);

		static async Task<Suspicious<TValue>> Core(Task<Suspicious<TValue>> source, Func<Error, Task> effect, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			return await result.TapError(effect, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and runs an async token-receiving <paramref name="effect" /> against the error of a failure; the result flows through unchanged.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="effect">The effect; receives the <paramref name="cancellationToken" />; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes, then before the <paramref name="effect" /> runs and passed into it.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with the unchanged result.</returns>
	public static Task<Suspicious<TValue>> TapError<TValue>
	(
		this Task<Suspicious<TValue>> source,
		Func<Error, CancellationToken, Task> effect,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(effect);
		return source.TapError(error => effect(error, cancellationToken), cancellationToken);
	}
}