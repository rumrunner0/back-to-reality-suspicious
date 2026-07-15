using System;
using System.Threading;
using System.Threading.Tasks;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Async Then extensions for <see cref="Suspicious{TValue}" /> and <c>Task</c>-wrapped generic results.</summary>
/// <remarks>The <c>CancellationToken</c> is checked before a continuation runs (plain sources) or right after the source completes (task sources); <see cref="OperationCanceledException" /> always propagates — cancellation is control flow, never a result.</remarks>
public static class SuspiciousOfTValueAsyncThenExtensions
{
	/// <summary>Chains an async <paramref name="binder" />; valueless results short-circuit.</summary>
	/// <param name="source">The source.</param>
	/// <param name="binder">The binder; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before the <paramref name="binder" /> runs.</param>
	/// <typeparam name="TValue">The source value type.</typeparam>
	/// <typeparam name="TResult">The result value type.</typeparam>
	/// <returns>A task with the result of the <paramref name="binder" />, or a propagated valueless result.</returns>
	/// <remarks>The <paramref name="binder" /> runs ONLY when a value is present; both a success without a value and a failure are propagated unchanged (fail-fast).</remarks>
	/// <exception cref="ArgumentNullException">Thrown if the <paramref name="binder" /> is <c>null</c> (synchronously), or if it produces <c>null</c> (in the returned task).</exception>
	public static Task<Suspicious<TResult>> Then<TValue, TResult>
	(
		this Suspicious<TValue> source,
		Func<TValue, Task<Suspicious<TResult>>> binder,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	where TResult : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(binder);

		if (!source.HasValue) return Task.FromResult(source.IsFailure ? source.AsFailure<TResult>() : Suspicious.Success<TResult>(source.Outcome));
		return Core(source, binder, cancellationToken);

		static async Task<Suspicious<TResult>> Core(Suspicious<TValue> source, Func<TValue, Task<Suspicious<TResult>>> binder, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var task = binder(source.Value);
			if (task is null) throw new ArgumentNullException(nameof(binder), "The binder produced null");

			var result = await task.ConfigureAwait(false);
			if (result is null) throw new ArgumentNullException(nameof(binder), "The binder produced null");

			return result;
		}
	}

	/// <summary>Chains an async token-receiving <paramref name="binder" />; valueless results short-circuit.</summary>
	/// <param name="source">The source.</param>
	/// <param name="binder">The binder; receives the <paramref name="cancellationToken" />; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before the <paramref name="binder" /> runs and passed into it.</param>
	/// <typeparam name="TValue">The source value type.</typeparam>
	/// <typeparam name="TResult">The result value type.</typeparam>
	/// <returns>A task with the result of the <paramref name="binder" />, or a propagated valueless result.</returns>
	public static Task<Suspicious<TResult>> Then<TValue, TResult>
	(
		this Suspicious<TValue> source,
		Func<TValue, CancellationToken, Task<Suspicious<TResult>>> binder,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	where TResult : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(binder);
		return source.Then(value => binder(value, cancellationToken), cancellationToken);
	}

	/// <summary>Chains an async <paramref name="binder" /> that returns a unit result; valueless results short-circuit.</summary>
	/// <param name="source">The source.</param>
	/// <param name="binder">The binder; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before the <paramref name="binder" /> runs.</param>
	/// <typeparam name="TValue">The source value type.</typeparam>
	/// <returns>A task with the result of the <paramref name="binder" />, or a propagated valueless result.</returns>
	/// <remarks>The <paramref name="binder" /> runs ONLY when a value is present; both a success without a value and a failure are propagated unchanged (fail-fast).</remarks>
	/// <exception cref="ArgumentNullException">Thrown if the <paramref name="binder" /> is <c>null</c> (synchronously), or if it produces <c>null</c> (in the returned task).</exception>
	public static Task<Suspicious> Then<TValue>
	(
		this Suspicious<TValue> source,
		Func<TValue, Task<Suspicious>> binder,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(binder);

		if (!source.HasValue) return Task.FromResult(source.Error is { } error ? Suspicious.Fail(error) : Suspicious.Success(source.Outcome));
		return Core(source, binder, cancellationToken);

		static async Task<Suspicious> Core(Suspicious<TValue> source, Func<TValue, Task<Suspicious>> binder, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var task = binder(source.Value);
			if (task is null) throw new ArgumentNullException(nameof(binder), "The binder produced null");

			var result = await task.ConfigureAwait(false);
			if (result is null) throw new ArgumentNullException(nameof(binder), "The binder produced null");

			return result;
		}
	}

	/// <summary>Chains an async token-receiving <paramref name="binder" /> that returns a unit result; valueless results short-circuit.</summary>
	/// <param name="source">The source.</param>
	/// <param name="binder">The binder; receives the <paramref name="cancellationToken" />; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before the <paramref name="binder" /> runs and passed into it.</param>
	/// <typeparam name="TValue">The source value type.</typeparam>
	/// <returns>A task with the result of the <paramref name="binder" />, or a propagated valueless result.</returns>
	public static Task<Suspicious> Then<TValue>
	(
		this Suspicious<TValue> source,
		Func<TValue, CancellationToken, Task<Suspicious>> binder,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(binder);
		return source.Then(value => binder(value, cancellationToken), cancellationToken);
	}

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
}