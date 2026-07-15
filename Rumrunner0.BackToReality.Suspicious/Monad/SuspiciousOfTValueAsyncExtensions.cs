using System;
using System.Threading;
using System.Threading.Tasks;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Task-based async extensions for <c>Task</c>-wrapped <see cref="Suspicious{TValue}" /> results.</summary>
/// <remarks>
/// <para>* Sync and async are same-named overloads — the delegate shape selects the member; chains flow without intermediate awaits.</para>
/// <para>* The <c>CancellationToken</c> is checked right after the source completes (and before an async continuation runs); <see cref="OperationCanceledException" /> always propagates — cancellation is control flow, never a result.</para>
/// <para>* Exceptions from continuations are never caught; a continuation that produces <c>null</c> throws <see cref="ArgumentNullException" />.</para>
/// </remarks>
public static class SuspiciousOfTValueAsyncExtensions
{
	#region Then

	/// <summary>Awaits the <paramref name="source" /> and chains a sync <paramref name="binder" />; valueless results short-circuit.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="binder">The binder.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes.</param>
	/// <typeparam name="TValue">The source value type.</typeparam>
	/// <typeparam name="TResult">The result value type.</typeparam>
	/// <returns>A task with the result of the <paramref name="binder" />, or a propagated valueless result.</returns>
	public static Task<Suspicious<TResult>> Then<TValue, TResult>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, Suspicious<TResult>> binder,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	where TResult : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(binder);

		return Core(source, binder, cancellationToken);

		static async Task<Suspicious<TResult>> Core(Task<Suspicious<TValue>> source, Func<TValue, Suspicious<TResult>> binder, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();

			return result.Then(binder);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and chains an async <paramref name="binder" />; valueless results short-circuit.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="binder">The binder; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes and before the <paramref name="binder" /> runs.</param>
	/// <typeparam name="TValue">The source value type.</typeparam>
	/// <typeparam name="TResult">The result value type.</typeparam>
	/// <returns>A task with the result of the <paramref name="binder" />, or a propagated valueless result.</returns>
	public static Task<Suspicious<TResult>> Then<TValue, TResult>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, Task<Suspicious<TResult>>> binder,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	where TResult : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(binder);

		return Core(source, binder, cancellationToken);

		static async Task<Suspicious<TResult>> Core(Task<Suspicious<TValue>> source, Func<TValue, Task<Suspicious<TResult>>> binder, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			return await result.Then(binder, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and chains an async token-receiving <paramref name="binder" />; valueless results short-circuit.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="binder">The binder; receives the <paramref name="cancellationToken" />; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes, then before the <paramref name="binder" /> runs and passed into it.</param>
	/// <typeparam name="TValue">The source value type.</typeparam>
	/// <typeparam name="TResult">The result value type.</typeparam>
	/// <returns>A task with the result of the <paramref name="binder" />, or a propagated valueless result.</returns>
	public static Task<Suspicious<TResult>> Then<TValue, TResult>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, CancellationToken, Task<Suspicious<TResult>>> binder,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	where TResult : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(binder);
		return source.Then(value => binder(value, cancellationToken), cancellationToken);
	}

	/// <summary>Awaits the <paramref name="source" /> and chains a sync unit-returning <paramref name="binder" />; valueless results short-circuit.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="binder">The binder.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes.</param>
	/// <typeparam name="TValue">The source value type.</typeparam>
	/// <returns>A task with the result of the <paramref name="binder" />, or a propagated valueless result.</returns>
	public static Task<Suspicious> Then<TValue>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, Suspicious> binder,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(binder);

		return Core(source, binder, cancellationToken);

		static async Task<Suspicious> Core(Task<Suspicious<TValue>> source, Func<TValue, Suspicious> binder, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();

			return result.Then(binder);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and chains an async unit-returning <paramref name="binder" />; valueless results short-circuit.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="binder">The binder; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes and before the <paramref name="binder" /> runs.</param>
	/// <typeparam name="TValue">The source value type.</typeparam>
	/// <returns>A task with the result of the <paramref name="binder" />, or a propagated valueless result.</returns>
	public static Task<Suspicious> Then<TValue>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, Task<Suspicious>> binder,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(binder);

		return Core(source, binder, cancellationToken);

		static async Task<Suspicious> Core(Task<Suspicious<TValue>> source, Func<TValue, Task<Suspicious>> binder, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			return await result.Then(binder, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and chains an async token-receiving unit-returning <paramref name="binder" />; valueless results short-circuit.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="binder">The binder; receives the <paramref name="cancellationToken" />; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes, then before the <paramref name="binder" /> runs and passed into it.</param>
	/// <typeparam name="TValue">The source value type.</typeparam>
	/// <returns>A task with the result of the <paramref name="binder" />, or a propagated valueless result.</returns>
	public static Task<Suspicious> Then<TValue>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, CancellationToken, Task<Suspicious>> binder,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(binder);
		return source.Then(value => binder(value, cancellationToken), cancellationToken);
	}

	#endregion

	#region Map

	/// <summary>Awaits the <paramref name="source" /> and maps the value with a sync <paramref name="mapper" />; valueless results are propagated unchanged.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="mapper">The mapper.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes.</param>
	/// <typeparam name="TValue">The source value type.</typeparam>
	/// <typeparam name="TResult">The result value type.</typeparam>
	/// <returns>A task with a new <see cref="Suspicious{TResult}" />.</returns>
	public static Task<Suspicious<TResult>> Map<TValue, TResult>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, TResult> mapper,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	where TResult : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(mapper);

		return Core(source, mapper, cancellationToken);

		static async Task<Suspicious<TResult>> Core(Task<Suspicious<TValue>> source, Func<TValue, TResult> mapper, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();

			return result.Map(mapper);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and maps the value with an async <paramref name="mapper" />; valueless results are propagated unchanged.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="mapper">The mapper; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes and before the <paramref name="mapper" /> runs.</param>
	/// <typeparam name="TValue">The source value type.</typeparam>
	/// <typeparam name="TResult">The result value type.</typeparam>
	/// <returns>A task with a new <see cref="Suspicious{TResult}" />.</returns>
	public static Task<Suspicious<TResult>> Map<TValue, TResult>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, Task<TResult>> mapper,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	where TResult : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(mapper);

		return Core(source, mapper, cancellationToken);

		static async Task<Suspicious<TResult>> Core(Task<Suspicious<TValue>> source, Func<TValue, Task<TResult>> mapper, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			return await result.Map(mapper, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and maps the value with an async token-receiving <paramref name="mapper" />; valueless results are propagated unchanged.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="mapper">The mapper; receives the <paramref name="cancellationToken" />; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes, then before the <paramref name="mapper" /> runs and passed into it.</param>
	/// <typeparam name="TValue">The source value type.</typeparam>
	/// <typeparam name="TResult">The result value type.</typeparam>
	/// <returns>A task with a new <see cref="Suspicious{TResult}" />.</returns>
	public static Task<Suspicious<TResult>> Map<TValue, TResult>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, CancellationToken, Task<TResult>> mapper,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	where TResult : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(mapper);
		return source.Map(value => mapper(value, cancellationToken), cancellationToken);
	}

	#endregion

	#region MapError

	/// <summary>Awaits the <paramref name="source" /> and maps the error of a failure with a sync <paramref name="mapper" />; a success is returned unchanged.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="mapper">The mapper.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with a new <see cref="Suspicious{TValue}" /> carrying the mapped error, or the unchanged success.</returns>
	public static Task<Suspicious<TValue>> MapError<TValue>
	(
		this Task<Suspicious<TValue>> source,
		Func<Error, Error> mapper,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(mapper);

		return Core(source, mapper, cancellationToken);

		static async Task<Suspicious<TValue>> Core(Task<Suspicious<TValue>> source, Func<Error, Error> mapper, CancellationToken cancellationToken)
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
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with a new <see cref="Suspicious{TValue}" /> carrying the mapped error, or the unchanged success.</returns>
	public static Task<Suspicious<TValue>> MapError<TValue>
	(
		this Task<Suspicious<TValue>> source,
		Func<Error, Task<Error>> mapper,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(mapper);

		return Core(source, mapper, cancellationToken);

		static async Task<Suspicious<TValue>> Core(Task<Suspicious<TValue>> source, Func<Error, Task<Error>> mapper, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			return await result.MapError(mapper, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and maps the error of a failure with an async token-receiving <paramref name="mapper" />; a success is returned unchanged.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="mapper">The mapper; receives the <paramref name="cancellationToken" />; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes, then before the <paramref name="mapper" /> runs and passed into it.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with a new <see cref="Suspicious{TValue}" /> carrying the mapped error, or the unchanged success.</returns>
	public static Task<Suspicious<TValue>> MapError<TValue>
	(
		this Task<Suspicious<TValue>> source,
		Func<Error, CancellationToken, Task<Error>> mapper,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(mapper);
		return source.MapError(error => mapper(error, cancellationToken), cancellationToken);
	}

	#endregion

	#region Tap

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

	#endregion

	#region TapError

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

	#endregion

	#region Match

	/// <summary>Awaits the <paramref name="source" /> and matches it into a <typeparamref name="TResult" /> with sync handlers.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="onValue">The handler for a success with a value.</param>
	/// <param name="onError">The handler for the failure rail.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <typeparam name="TResult">The result type.</typeparam>
	/// <returns>A task with the result of the invoked handler.</returns>
	/// <remarks>Use this overload only in flows where a success without a value can't occur; otherwise use the overload with an <c>onNoValue</c> handler.</remarks>
	public static Task<TResult> Match<TValue, TResult>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, TResult> onValue,
		Func<Error, TResult> onError,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		return Core(source, onValue, onError, cancellationToken);

		static async Task<TResult> Core(Task<Suspicious<TValue>> source, Func<TValue, TResult> onValue, Func<Error, TResult> onError, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();

			return result.Match(onValue, onError);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and matches it into a <typeparamref name="TResult" /> with sync handlers.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="onValue">The handler for a success with a value.</param>
	/// <param name="onNoValue">The handler for a success without a value.</param>
	/// <param name="onError">The handler for the failure rail.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <typeparam name="TResult">The result type.</typeparam>
	/// <returns>A task with the result of the invoked handler.</returns>
	public static Task<TResult> Match<TValue, TResult>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, TResult> onValue,
		Func<TResult> onNoValue,
		Func<Error, TResult> onError,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onNoValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		return Core(source, onValue, onNoValue, onError, cancellationToken);

		static async Task<TResult> Core(Task<Suspicious<TValue>> source, Func<TValue, TResult> onValue, Func<TResult> onNoValue, Func<Error, TResult> onError, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();

			return result.Match(onValue, onNoValue, onError);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and matches it into a <typeparamref name="TResult" /> with async handlers.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="onValue">The handler for a success with a value; must not produce <c>null</c>.</param>
	/// <param name="onError">The handler for the failure rail; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes and before a handler runs.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <typeparam name="TResult">The result type.</typeparam>
	/// <returns>A task with the result of the invoked handler.</returns>
	/// <remarks>Use this overload only in flows where a success without a value can't occur; otherwise use the overload with an <c>onNoValue</c> handler.</remarks>
	public static Task<TResult> Match<TValue, TResult>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, Task<TResult>> onValue,
		Func<Error, Task<TResult>> onError,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		return Core(source, onValue, onError, cancellationToken);

		static async Task<TResult> Core(Task<Suspicious<TValue>> source, Func<TValue, Task<TResult>> onValue, Func<Error, Task<TResult>> onError, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			return await result.Match(onValue, onError, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and matches it into a <typeparamref name="TResult" /> with async handlers.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="onValue">The handler for a success with a value; must not produce <c>null</c>.</param>
	/// <param name="onNoValue">The handler for a success without a value; must not produce <c>null</c>.</param>
	/// <param name="onError">The handler for the failure rail; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes and before a handler runs.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <typeparam name="TResult">The result type.</typeparam>
	/// <returns>A task with the result of the invoked handler.</returns>
	public static Task<TResult> Match<TValue, TResult>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, Task<TResult>> onValue,
		Func<Task<TResult>> onNoValue,
		Func<Error, Task<TResult>> onError,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onNoValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		return Core(source, onValue, onNoValue, onError, cancellationToken);

		static async Task<TResult> Core(Task<Suspicious<TValue>> source, Func<TValue, Task<TResult>> onValue, Func<Task<TResult>> onNoValue, Func<Error, Task<TResult>> onError, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			return await result.Match(onValue, onNoValue, onError, cancellationToken).ConfigureAwait(false);
		}
	}

	#endregion

	#region Switch

	/// <summary>Awaits the <paramref name="source" /> and switches on it with sync handlers.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="onValue">The handler for a success with a value.</param>
	/// <param name="onError">The handler for the failure rail.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task that completes when the invoked handler completes.</returns>
	/// <remarks>Use this overload only in flows where a success without a value can't occur; otherwise use the overload with an <c>onNoValue</c> handler.</remarks>
	public static Task Switch<TValue>
	(
		this Task<Suspicious<TValue>> source,
		Action<TValue> onValue,
		Action<Error> onError,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		return Core(source, onValue, onError, cancellationToken);

		static async Task Core(Task<Suspicious<TValue>> source, Action<TValue> onValue, Action<Error> onError, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();

			result.Switch(onValue, onError);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and switches on it with sync handlers.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="onValue">The handler for a success with a value.</param>
	/// <param name="onNoValue">The handler for a success without a value.</param>
	/// <param name="onError">The handler for the failure rail.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task that completes when the invoked handler completes.</returns>
	public static Task Switch<TValue>
	(
		this Task<Suspicious<TValue>> source,
		Action<TValue> onValue,
		Action onNoValue,
		Action<Error> onError,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onNoValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		return Core(source, onValue, onNoValue, onError, cancellationToken);

		static async Task Core(Task<Suspicious<TValue>> source, Action<TValue> onValue, Action onNoValue, Action<Error> onError, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();

			result.Switch(onValue, onNoValue, onError);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and switches on it with async handlers.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="onValue">The handler for a success with a value; must not produce <c>null</c>.</param>
	/// <param name="onError">The handler for the failure rail; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes and before a handler runs.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task that completes when the invoked handler completes.</returns>
	/// <remarks>Use this overload only in flows where a success without a value can't occur; otherwise use the overload with an <c>onNoValue</c> handler.</remarks>
	public static Task Switch<TValue>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, Task> onValue,
		Func<Error, Task> onError,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		return Core(source, onValue, onError, cancellationToken);

		static async Task Core(Task<Suspicious<TValue>> source, Func<TValue, Task> onValue, Func<Error, Task> onError, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			await result.Switch(onValue, onError, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and switches on it with async handlers.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="onValue">The handler for a success with a value; must not produce <c>null</c>.</param>
	/// <param name="onNoValue">The handler for a success without a value; must not produce <c>null</c>.</param>
	/// <param name="onError">The handler for the failure rail; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes and before a handler runs.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task that completes when the invoked handler completes.</returns>
	public static Task Switch<TValue>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, Task> onValue,
		Func<Task> onNoValue,
		Func<Error, Task> onError,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onNoValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		return Core(source, onValue, onNoValue, onError, cancellationToken);

		static async Task Core(Task<Suspicious<TValue>> source, Func<TValue, Task> onValue, Func<Task> onNoValue, Func<Error, Task> onError, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			await result.Switch(onValue, onNoValue, onError, cancellationToken).ConfigureAwait(false);
		}
	}

	#endregion

	#region Conversion

	/// <summary>Awaits the <paramref name="source" /> and drops the value axis, keeping the outcome and the error.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with a unit <see cref="Suspicious" />.</returns>
	public static Task<Suspicious> AsUnit<TValue>
	(
		this Task<Suspicious<TValue>> source,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);

		return Core(source, cancellationToken);

		static async Task<Suspicious> Core(Task<Suspicious<TValue>> source, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();

			return result.AsUnit();
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and reinterprets the failed result as a failed <see cref="Suspicious{TResult}" />.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes.</param>
	/// <typeparam name="TValue">The source value type.</typeparam>
	/// <typeparam name="TResult">The result value type.</typeparam>
	/// <returns>A task with a new failed <see cref="Suspicious{TResult}" /> carrying the same error.</returns>
	/// <exception cref="InvalidOperationException">Thrown (in the returned task) if the result is a success — converting a success is a contract violation.</exception>
	public static Task<Suspicious<TResult>> AsFailure<TValue, TResult>
	(
		this Task<Suspicious<TValue>> source,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	where TResult : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);

		return Core(source, cancellationToken);

		static async Task<Suspicious<TResult>> Core(Task<Suspicious<TValue>> source, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();

			return result.AsFailure<TResult>();
		}
	}

	#endregion
}