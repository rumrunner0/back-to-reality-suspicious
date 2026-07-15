using System;
using System.Threading;
using System.Threading.Tasks;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Task-based async extensions for <c>Task</c>-wrapped unit <see cref="Suspicious" /> results.</summary>
/// <remarks>
/// <para>* Sync and async are same-named overloads — the delegate shape selects the member; chains flow without intermediate awaits.</para>
/// <para>* The <c>CancellationToken</c> is checked right after the source completes (and before an async continuation runs); <see cref="OperationCanceledException" /> always propagates — cancellation is control flow, never a result.</para>
/// <para>* Exceptions from continuations are never caught; a continuation that produces <c>null</c> throws <see cref="ArgumentNullException" />.</para>
/// </remarks>
public static class SuspiciousAsyncExtensions
{
	#region Then

	/// <summary>Awaits the <paramref name="source" /> and chains a sync <paramref name="binder" />; a failure short-circuits.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="binder">The binder.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes.</param>
	/// <returns>A task with the result of the <paramref name="binder" />, or the failed result unchanged.</returns>
	public static Task<Suspicious> Then
	(
		this Task<Suspicious> source,
		Func<Suspicious> binder,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(binder);

		return Core(source, binder, cancellationToken);

		static async Task<Suspicious> Core(Task<Suspicious> source, Func<Suspicious> binder, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();

			return result.Then(binder);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and chains an async <paramref name="binder" />; a failure short-circuits.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="binder">The binder; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes and before the <paramref name="binder" /> runs.</param>
	/// <returns>A task with the result of the <paramref name="binder" />, or the failed result unchanged.</returns>
	public static Task<Suspicious> Then
	(
		this Task<Suspicious> source,
		Func<Task<Suspicious>> binder,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(binder);

		return Core(source, binder, cancellationToken);

		static async Task<Suspicious> Core(Task<Suspicious> source, Func<Task<Suspicious>> binder, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			return await result.Then(binder, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and chains an async token-receiving <paramref name="binder" />; a failure short-circuits.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="binder">The binder; receives the <paramref name="cancellationToken" />; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes, then before the <paramref name="binder" /> runs and passed into it.</param>
	/// <returns>A task with the result of the <paramref name="binder" />, or the failed result unchanged.</returns>
	public static Task<Suspicious> Then
	(
		this Task<Suspicious> source,
		Func<CancellationToken, Task<Suspicious>> binder,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(binder);
		return source.Then(() => binder(cancellationToken), cancellationToken);
	}

	/// <summary>Awaits the <paramref name="source" /> and chains a sync <paramref name="binder" /> that returns a <see cref="Suspicious{TValue}" />; a failure short-circuits.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="binder">The binder.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with the result of the <paramref name="binder" />, or a failed <see cref="Suspicious{TValue}" /> carrying the error.</returns>
	public static Task<Suspicious<TValue>> Then<TValue>
	(
		this Task<Suspicious> source,
		Func<Suspicious<TValue>> binder,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(binder);

		return Core(source, binder, cancellationToken);

		static async Task<Suspicious<TValue>> Core(Task<Suspicious> source, Func<Suspicious<TValue>> binder, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();

			return result.Then(binder);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and chains an async <paramref name="binder" /> that returns a <see cref="Suspicious{TValue}" />; a failure short-circuits.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="binder">The binder; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes and before the <paramref name="binder" /> runs.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with the result of the <paramref name="binder" />, or a failed <see cref="Suspicious{TValue}" /> carrying the error.</returns>
	public static Task<Suspicious<TValue>> Then<TValue>
	(
		this Task<Suspicious> source,
		Func<Task<Suspicious<TValue>>> binder,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(binder);

		return Core(source, binder, cancellationToken);

		static async Task<Suspicious<TValue>> Core(Task<Suspicious> source, Func<Task<Suspicious<TValue>>> binder, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			return await result.Then(binder, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and chains an async token-receiving <paramref name="binder" /> that returns a <see cref="Suspicious{TValue}" />; a failure short-circuits.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="binder">The binder; receives the <paramref name="cancellationToken" />; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes, then before the <paramref name="binder" /> runs and passed into it.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with the result of the <paramref name="binder" />, or a failed <see cref="Suspicious{TValue}" /> carrying the error.</returns>
	public static Task<Suspicious<TValue>> Then<TValue>
	(
		this Task<Suspicious> source,
		Func<CancellationToken, Task<Suspicious<TValue>>> binder,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(binder);
		return source.Then(() => binder(cancellationToken), cancellationToken);
	}

	#endregion

	#region MapError

	/// <summary>Awaits the <paramref name="source" /> and maps the error of a failure with a sync <paramref name="mapper" />; a success is returned unchanged.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="mapper">The mapper.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes.</param>
	/// <returns>A task with a new <see cref="Suspicious" /> carrying the mapped error, or the unchanged success.</returns>
	public static Task<Suspicious> MapError
	(
		this Task<Suspicious> source,
		Func<Error, Error> mapper,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(mapper);

		return Core(source, mapper, cancellationToken);

		static async Task<Suspicious> Core(Task<Suspicious> source, Func<Error, Error> mapper, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();

			return result.MapError(mapper);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and maps the error of a failure with an async <paramref name="mapper" />; a success is returned unchanged.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="mapper">The mapper; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes and before the <paramref name="mapper" /> runs.</param>
	/// <returns>A task with a new <see cref="Suspicious" /> carrying the mapped error, or the unchanged success.</returns>
	public static Task<Suspicious> MapError
	(
		this Task<Suspicious> source,
		Func<Error, Task<Error>> mapper,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(mapper);

		return Core(source, mapper, cancellationToken);

		static async Task<Suspicious> Core(Task<Suspicious> source, Func<Error, Task<Error>> mapper, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			return await result.MapError(mapper, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and maps the error of a failure with an async token-receiving <paramref name="mapper" />; a success is returned unchanged.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="mapper">The mapper; receives the <paramref name="cancellationToken" />; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes, then before the <paramref name="mapper" /> runs and passed into it.</param>
	/// <returns>A task with a new <see cref="Suspicious" /> carrying the mapped error, or the unchanged success.</returns>
	public static Task<Suspicious> MapError
	(
		this Task<Suspicious> source,
		Func<Error, CancellationToken, Task<Error>> mapper,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(mapper);
		return source.MapError(error => mapper(error, cancellationToken), cancellationToken);
	}

	#endregion

	#region Tap

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

	#endregion

	#region TapError

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

	#endregion

	#region Match

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

	#endregion

	#region Switch

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

	#endregion

	#region Conversion

	/// <summary>Awaits the <paramref name="source" /> and reinterprets the failed unit result as a failed <see cref="Suspicious{TValue}" />.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with a new failed <see cref="Suspicious{TValue}" /> carrying the same error.</returns>
	/// <exception cref="InvalidOperationException">Thrown (in the returned task) if the result is a success — converting a success is a contract violation.</exception>
	public static Task<Suspicious<TValue>> AsFailure<TValue>
	(
		this Task<Suspicious> source,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);

		return Core(source, cancellationToken);

		static async Task<Suspicious<TValue>> Core(Task<Suspicious> source, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();

			return result.AsFailure<TValue>();
		}
	}

	#endregion
}