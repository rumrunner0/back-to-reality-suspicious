using System;
using System.Threading;
using System.Threading.Tasks;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Async Tap extensions for the unit <see cref="Suspicious" /> and <c>Task</c>-wrapped unit results.</summary>
public static class SuspiciousAsyncTapExtensions
{
	/// <summary>Runs an async <paramref name="effect" /> against a success; the source flows through unchanged.</summary>
	/// <param name="source">The source.</param>
	/// <param name="effect">The effect; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before the <paramref name="effect" /> runs.</param>
	/// <returns>A task with this <see cref="Suspicious" />.</returns>
	/// <remarks>The <paramref name="effect" /> runs on ANY success.</remarks>
	/// <exception cref="ArgumentNullException">Thrown if the <paramref name="effect" /> is <c>null</c> (synchronously), or if it produces <c>null</c> (in the returned task).</exception>
	public static Task<Suspicious> Tap
	(
		this Suspicious source,
		Func<Task> effect,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(effect);

		if (source.IsFailure) return Task.FromResult(source);
		return Core(source, effect, cancellationToken);

		static async Task<Suspicious> Core(Suspicious source, Func<Task> effect, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var task = effect();
			if (task is null) throw new ArgumentNullException(nameof(effect), "The effect produced null");

			await task.ConfigureAwait(false);
			return source;
		}
	}

	/// <summary>Runs an async token-receiving <paramref name="effect" /> against a success; the source flows through unchanged.</summary>
	/// <param name="source">The source.</param>
	/// <param name="effect">The effect; receives the <paramref name="cancellationToken" />; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before the <paramref name="effect" /> runs and passed into it.</param>
	/// <returns>A task with this <see cref="Suspicious" />.</returns>
	public static Task<Suspicious> Tap
	(
		this Suspicious source,
		Func<CancellationToken, Task> effect,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(effect);
		return source.Tap(() => effect(cancellationToken), cancellationToken);
	}

	/// <summary>Runs an async result-returning <paramref name="effect" /> against a success; its failure REPLACES this result, its success is discarded.</summary>
	/// <param name="source">The source.</param>
	/// <param name="effect">The effect; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before the <paramref name="effect" /> runs.</param>
	/// <returns>A task with the failed result of the <paramref name="effect" />, or this <see cref="Suspicious" /> unchanged.</returns>
	/// <remarks>The <paramref name="effect" /> runs on ANY success; only its failure rail matters — this instance flows through on its success, so the original success kind is PRESERVED.</remarks>
	/// <exception cref="ArgumentNullException">Thrown if the <paramref name="effect" /> is <c>null</c> (synchronously), or if it produces <c>null</c> (in the returned task).</exception>
	public static Task<Suspicious> Tap
	(
		this Suspicious source,
		Func<Task<Suspicious>> effect,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(effect);

		if (source.IsFailure) return Task.FromResult(source);
		return Core(source, effect, cancellationToken);

		static async Task<Suspicious> Core(Suspicious source, Func<Task<Suspicious>> effect, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var task = effect();
			if (task is null) throw new ArgumentNullException(nameof(effect), "The effect produced null");

			var result = await task.ConfigureAwait(false);
			if (result is null) throw new ArgumentNullException(nameof(effect), "The effect produced null");

			return result.IsFailure ? result : source;
		}
	}

	/// <summary>Runs an async token-receiving result-returning <paramref name="effect" /> against a success; its failure REPLACES this result, its success is discarded.</summary>
	/// <param name="source">The source.</param>
	/// <param name="effect">The effect; receives the <paramref name="cancellationToken" />; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before the <paramref name="effect" /> runs and passed into it.</param>
	/// <returns>A task with the failed result of the <paramref name="effect" />, or this <see cref="Suspicious" /> unchanged.</returns>
	public static Task<Suspicious> Tap
	(
		this Suspicious source,
		Func<CancellationToken, Task<Suspicious>> effect,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(effect);
		return source.Tap(() => effect(cancellationToken), cancellationToken);
	}

	/// <summary>Runs an async <paramref name="effect" /> against the error of a failure; the source flows through unchanged.</summary>
	/// <param name="source">The source.</param>
	/// <param name="effect">The effect; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before the <paramref name="effect" /> runs.</param>
	/// <returns>A task with this <see cref="Suspicious" />.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the <paramref name="effect" /> is <c>null</c> (synchronously), or if it produces <c>null</c> (in the returned task).</exception>
	public static Task<Suspicious> TapError
	(
		this Suspicious source,
		Func<Error, Task> effect,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(effect);

		if (source.Error is not { } error) return Task.FromResult(source);
		return Core(source, error, effect, cancellationToken);

		static async Task<Suspicious> Core(Suspicious source, Error error, Func<Error, Task> effect, CancellationToken cancellationToken)
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
	/// <returns>A task with this <see cref="Suspicious" />.</returns>
	public static Task<Suspicious> TapError
	(
		this Suspicious source,
		Func<Error, CancellationToken, Task> effect,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(effect);
		return source.TapError(error => effect(error, cancellationToken), cancellationToken);
	}

	/// <summary>Awaits the <paramref name="source" /> and runs a sync <paramref name="effect" /> against a success; the result flows through unchanged.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="effect">The effect.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes.</param>
	/// <returns>A task with the unchanged result.</returns>
	public static Task<Suspicious> Tap
	(
		this Task<Suspicious> source,
		Action effect,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(effect);

		return Core(source, effect, cancellationToken);

		static async Task<Suspicious> Core(Task<Suspicious> source, Action effect, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();

			return result.Tap(effect);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and runs a sync result-returning <paramref name="effect" /> against a success; its failure REPLACES the result.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="effect">The effect; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes.</param>
	/// <returns>A task with the failed result of the <paramref name="effect" />, or the unchanged result.</returns>
	public static Task<Suspicious> Tap
	(
		this Task<Suspicious> source,
		Func<Suspicious> effect,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(effect);

		return Core(source, effect, cancellationToken);

		static async Task<Suspicious> Core(Task<Suspicious> source, Func<Suspicious> effect, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();

			return result.Tap(effect);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and runs an async <paramref name="effect" /> against a success; the result flows through unchanged.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="effect">The effect; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes and before the <paramref name="effect" /> runs.</param>
	/// <returns>A task with the unchanged result.</returns>
	public static Task<Suspicious> Tap
	(
		this Task<Suspicious> source,
		Func<Task> effect,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(effect);

		return Core(source, effect, cancellationToken);

		static async Task<Suspicious> Core(Task<Suspicious> source, Func<Task> effect, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			return await result.Tap(effect, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and runs an async token-receiving <paramref name="effect" /> against a success; the result flows through unchanged.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="effect">The effect; receives the <paramref name="cancellationToken" />; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes, then before the <paramref name="effect" /> runs and passed into it.</param>
	/// <returns>A task with the unchanged result.</returns>
	public static Task<Suspicious> Tap
	(
		this Task<Suspicious> source,
		Func<CancellationToken, Task> effect,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(effect);
		return source.Tap(() => effect(cancellationToken), cancellationToken);
	}

	/// <summary>Awaits the <paramref name="source" /> and runs an async result-returning <paramref name="effect" /> against a success; its failure REPLACES the result.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="effect">The effect; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes and before the <paramref name="effect" /> runs.</param>
	/// <returns>A task with the failed result of the <paramref name="effect" />, or the unchanged result.</returns>
	public static Task<Suspicious> Tap
	(
		this Task<Suspicious> source,
		Func<Task<Suspicious>> effect,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(effect);

		return Core(source, effect, cancellationToken);

		static async Task<Suspicious> Core(Task<Suspicious> source, Func<Task<Suspicious>> effect, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			return await result.Tap(effect, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and runs an async token-receiving result-returning <paramref name="effect" /> against a success; its failure REPLACES the result.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="effect">The effect; receives the <paramref name="cancellationToken" />; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes, then before the <paramref name="effect" /> runs and passed into it.</param>
	/// <returns>A task with the failed result of the <paramref name="effect" />, or the unchanged result.</returns>
	public static Task<Suspicious> Tap
	(
		this Task<Suspicious> source,
		Func<CancellationToken, Task<Suspicious>> effect,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(effect);
		return source.Tap(() => effect(cancellationToken), cancellationToken);
	}

	/// <summary>Awaits the <paramref name="source" /> and runs a sync <paramref name="effect" /> against the error of a failure; the result flows through unchanged.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="effect">The effect.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes.</param>
	/// <returns>A task with the unchanged result.</returns>
	public static Task<Suspicious> TapError
	(
		this Task<Suspicious> source,
		Action<Error> effect,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(effect);

		return Core(source, effect, cancellationToken);

		static async Task<Suspicious> Core(Task<Suspicious> source, Action<Error> effect, CancellationToken cancellationToken)
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
	/// <returns>A task with the unchanged result.</returns>
	public static Task<Suspicious> TapError
	(
		this Task<Suspicious> source,
		Func<Error, Task> effect,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(effect);

		return Core(source, effect, cancellationToken);

		static async Task<Suspicious> Core(Task<Suspicious> source, Func<Error, Task> effect, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			return await result.TapError(effect, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and runs an async token-receiving <paramref name="effect" /> against the error of a failure; the result flows through unchanged.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="effect">The effect; receives the <paramref name="cancellationToken" />; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes, then before the <paramref name="effect" /> runs and passed into it.</param>
	/// <returns>A task with the unchanged result.</returns>
	public static Task<Suspicious> TapError
	(
		this Task<Suspicious> source,
		Func<Error, CancellationToken, Task> effect,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(effect);
		return source.TapError(error => effect(error, cancellationToken), cancellationToken);
	}
}