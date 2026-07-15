using System;
using System.Threading;
using System.Threading.Tasks;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Async Match extensions for the unit <see cref="Suspicious" /> and <c>Task</c>-wrapped unit results.</summary>
/// <remarks>The plain-source async <c>Match</c> keeps its token-less and token-taking forms as SEPARATE overloads (not one optional parameter) — the "no defaults substituted" tie-breaker would otherwise make the sync <c>Match</c> win for task-returning handlers.</remarks>
public static class SuspiciousAsyncMatchExtensions
{
	/// <summary>Matches this <see cref="Suspicious" /> into a <typeparamref name="TResult" /> with async handlers.</summary>
	/// <param name="source">The source.</param>
	/// <param name="onSuccess">The handler for the success rail; must not produce <c>null</c>.</param>
	/// <param name="onError">The handler for the failure rail; must not produce <c>null</c>.</param>
	/// <typeparam name="TResult">The result type.</typeparam>
	/// <returns>A task with the result of the invoked handler.</returns>
	/// <exception cref="ArgumentNullException">Thrown if a handler is <c>null</c> (synchronously), or if it produces <c>null</c> (in the returned task).</exception>
	public static Task<TResult> Match<TResult>
	(
		this Suspicious source,
		Func<Task<TResult>> onSuccess,
		Func<Error, Task<TResult>> onError
	)
	{
		return source.Match(onSuccess, onError, CancellationToken.None);
	}

	/// <summary>Matches this <see cref="Suspicious" /> into a <typeparamref name="TResult" /> with async handlers.</summary>
	/// <param name="source">The source.</param>
	/// <param name="onSuccess">The handler for the success rail; must not produce <c>null</c>.</param>
	/// <param name="onError">The handler for the failure rail; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before a handler runs.</param>
	/// <typeparam name="TResult">The result type.</typeparam>
	/// <returns>A task with the result of the invoked handler.</returns>
	/// <exception cref="ArgumentNullException">Thrown if a handler is <c>null</c> (synchronously), or if it produces <c>null</c> (in the returned task).</exception>
	public static Task<TResult> Match<TResult>
	(
		this Suspicious source,
		Func<Task<TResult>> onSuccess,
		Func<Error, Task<TResult>> onError,
		CancellationToken cancellationToken
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onSuccess);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		return Core(source, onSuccess, onError, cancellationToken);

		static async Task<TResult> Core(Suspicious source, Func<Task<TResult>> onSuccess, Func<Error, Task<TResult>> onError, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (source.Error is { } error)
			{
				var errorTask = onError(error);
				if (errorTask is null) throw new ArgumentNullException(nameof(onError), "The handler produced null");
				return await errorTask.ConfigureAwait(false);
			}

			var successTask = onSuccess();
			if (successTask is null) throw new ArgumentNullException(nameof(onSuccess), "The handler produced null");
			return await successTask.ConfigureAwait(false);
		}
	}

	/// <summary>Switches on this <see cref="Suspicious" /> with async handlers.</summary>
	/// <param name="source">The source.</param>
	/// <param name="onSuccess">The handler for the success rail; must not produce <c>null</c>.</param>
	/// <param name="onError">The handler for the failure rail; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before a handler runs.</param>
	/// <returns>A task that completes when the invoked handler completes.</returns>
	/// <exception cref="ArgumentNullException">Thrown if a handler is <c>null</c> (synchronously), or if it produces <c>null</c> (in the returned task).</exception>
	public static Task Switch
	(
		this Suspicious source,
		Func<Task> onSuccess,
		Func<Error, Task> onError,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onSuccess);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		return Core(source, onSuccess, onError, cancellationToken);

		static async Task Core(Suspicious source, Func<Task> onSuccess, Func<Error, Task> onError, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (source.Error is { } error)
			{
				var errorTask = onError(error);
				if (errorTask is null) throw new ArgumentNullException(nameof(onError), "The handler produced null");
				await errorTask.ConfigureAwait(false);
				return;
			}

			var successTask = onSuccess();
			if (successTask is null) throw new ArgumentNullException(nameof(onSuccess), "The handler produced null");
			await successTask.ConfigureAwait(false);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and matches it into a <typeparamref name="TResult" /> with sync handlers.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="onSuccess">The handler for the success rail.</param>
	/// <param name="onError">The handler for the failure rail.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes.</param>
	/// <typeparam name="TResult">The result type.</typeparam>
	/// <returns>A task with the result of the invoked handler.</returns>
	public static Task<TResult> Match<TResult>
	(
		this Task<Suspicious> source,
		Func<TResult> onSuccess,
		Func<Error, TResult> onError,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onSuccess);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		return Core(source, onSuccess, onError, cancellationToken);

		static async Task<TResult> Core(Task<Suspicious> source, Func<TResult> onSuccess, Func<Error, TResult> onError, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();

			return result.Match(onSuccess, onError);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and matches it into a <typeparamref name="TResult" /> with async handlers.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="onSuccess">The handler for the success rail; must not produce <c>null</c>.</param>
	/// <param name="onError">The handler for the failure rail; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes and before a handler runs.</param>
	/// <typeparam name="TResult">The result type.</typeparam>
	/// <returns>A task with the result of the invoked handler.</returns>
	public static Task<TResult> Match<TResult>
	(
		this Task<Suspicious> source,
		Func<Task<TResult>> onSuccess,
		Func<Error, Task<TResult>> onError,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onSuccess);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		return Core(source, onSuccess, onError, cancellationToken);

		static async Task<TResult> Core(Task<Suspicious> source, Func<Task<TResult>> onSuccess, Func<Error, Task<TResult>> onError, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			return await result.Match(onSuccess, onError, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and switches on it with sync handlers.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="onSuccess">The handler for the success rail.</param>
	/// <param name="onError">The handler for the failure rail.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes.</param>
	/// <returns>A task that completes when the invoked handler completes.</returns>
	public static Task Switch
	(
		this Task<Suspicious> source,
		Action onSuccess,
		Action<Error> onError,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onSuccess);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		return Core(source, onSuccess, onError, cancellationToken);

		static async Task Core(Task<Suspicious> source, Action onSuccess, Action<Error> onError, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();

			result.Switch(onSuccess, onError);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and switches on it with async handlers.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="onSuccess">The handler for the success rail; must not produce <c>null</c>.</param>
	/// <param name="onError">The handler for the failure rail; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes and before a handler runs.</param>
	/// <returns>A task that completes when the invoked handler completes.</returns>
	public static Task Switch
	(
		this Task<Suspicious> source,
		Func<Task> onSuccess,
		Func<Error, Task> onError,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onSuccess);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		return Core(source, onSuccess, onError, cancellationToken);

		static async Task Core(Task<Suspicious> source, Func<Task> onSuccess, Func<Error, Task> onError, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			await result.Switch(onSuccess, onError, cancellationToken).ConfigureAwait(false);
		}
	}
}