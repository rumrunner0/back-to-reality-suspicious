using System;
using System.Threading;
using System.Threading.Tasks;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Async Then extensions for the unit <see cref="Suspicious" /> and <c>Task</c>-wrapped unit results.</summary>
/// <remarks>The <c>CancellationToken</c> is checked before a continuation runs (plain sources) or right after the source completes (task sources); <see cref="OperationCanceledException" /> always propagates — cancellation is control flow, never a result.</remarks>
public static class SuspiciousAsyncThenExtensions
{
	/// <summary>Chains an async <paramref name="binder" />; a failure short-circuits.</summary>
	/// <param name="source">The source.</param>
	/// <param name="binder">The binder; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before the <paramref name="binder" /> runs.</param>
	/// <returns>A task with the result of the <paramref name="binder" />, or this failed <see cref="Suspicious" /> unchanged.</returns>
	/// <remarks>The <paramref name="binder" /> runs on ANY success; a non-<c>ok</c> success kind is consumed (the binder's outcome wins).</remarks>
	/// <exception cref="ArgumentNullException">Thrown if the <paramref name="binder" /> is <c>null</c> (synchronously), or if it produces <c>null</c> (in the returned task).</exception>
	public static Task<Suspicious> Then
	(
		this Suspicious source,
		Func<Task<Suspicious>> binder,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(binder);

		if (source.IsFailure) return Task.FromResult(source);
		return Core(binder, cancellationToken);

		static async Task<Suspicious> Core(Func<Task<Suspicious>> binder, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var task = binder();
			if (task is null) throw new ArgumentNullException(nameof(binder), "The binder produced null");

			var result = await task.ConfigureAwait(false);
			if (result is null) throw new ArgumentNullException(nameof(binder), "The binder produced null");

			return result;
		}
	}

	/// <summary>Chains an async token-receiving <paramref name="binder" />; a failure short-circuits.</summary>
	/// <param name="source">The source.</param>
	/// <param name="binder">The binder; receives the <paramref name="cancellationToken" />; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before the <paramref name="binder" /> runs and passed into it.</param>
	/// <returns>A task with the result of the <paramref name="binder" />, or this failed <see cref="Suspicious" /> unchanged.</returns>
	public static Task<Suspicious> Then
	(
		this Suspicious source,
		Func<CancellationToken, Task<Suspicious>> binder,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(binder);
		return source.Then(() => binder(cancellationToken), cancellationToken);
	}

	/// <summary>Chains an async <paramref name="binder" /> that returns a <see cref="Suspicious{TValue}" />; a failure short-circuits.</summary>
	/// <param name="source">The source.</param>
	/// <param name="binder">The binder; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before the <paramref name="binder" /> runs.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with the result of the <paramref name="binder" />, or a failed <see cref="Suspicious{TValue}" /> carrying this error.</returns>
	/// <remarks>The <paramref name="binder" /> runs on ANY success; a non-<c>ok</c> success kind is consumed (the binder's outcome wins).</remarks>
	/// <exception cref="ArgumentNullException">Thrown if the <paramref name="binder" /> is <c>null</c> (synchronously), or if it produces <c>null</c> (in the returned task).</exception>
	public static Task<Suspicious<TValue>> Then<TValue>
	(
		this Suspicious source,
		Func<Task<Suspicious<TValue>>> binder,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(binder);

		if (source.IsFailure) return Task.FromResult(source.AsFailure<TValue>());
		return Core(binder, cancellationToken);

		static async Task<Suspicious<TValue>> Core(Func<Task<Suspicious<TValue>>> binder, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var task = binder();
			if (task is null) throw new ArgumentNullException(nameof(binder), "The binder produced null");

			var result = await task.ConfigureAwait(false);
			if (result is null) throw new ArgumentNullException(nameof(binder), "The binder produced null");

			return result;
		}
	}

	/// <summary>Chains an async token-receiving <paramref name="binder" /> that returns a <see cref="Suspicious{TValue}" />; a failure short-circuits.</summary>
	/// <param name="source">The source.</param>
	/// <param name="binder">The binder; receives the <paramref name="cancellationToken" />; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before the <paramref name="binder" /> runs and passed into it.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with the result of the <paramref name="binder" />, or a failed <see cref="Suspicious{TValue}" /> carrying this error.</returns>
	public static Task<Suspicious<TValue>> Then<TValue>
	(
		this Suspicious source,
		Func<CancellationToken, Task<Suspicious<TValue>>> binder,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(binder);
		return source.Then(() => binder(cancellationToken), cancellationToken);
	}

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
}