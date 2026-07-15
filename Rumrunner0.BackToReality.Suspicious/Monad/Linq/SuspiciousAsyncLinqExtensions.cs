using System;
using System.Threading.Tasks;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>LINQ query-syntax extensions for <c>Task</c>-wrapped <see cref="Suspicious{TValue}" /> results.</summary>
/// <remarks>Enable async query syntax like <c>from u in FindUserAsync(id) from o in LoadOrdersAsync(u) select Report.From(u, o)</c>; mixed sync/async <c>from</c> clauses are supported in both directions. The query pattern has no slot for a <c>CancellationToken</c> — delegates capture their own.</remarks>
public static class SuspiciousAsyncLinqExtensions
{
	/// <summary>Awaits the <paramref name="source" /> and maps the value into a <typeparamref name="TResult" />.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="selector">The selector.</param>
	/// <typeparam name="TValue">The source value type.</typeparam>
	/// <typeparam name="TResult">The result value type.</typeparam>
	/// <returns>A task with a new <see cref="Suspicious{TResult}" />.</returns>
	/// <remarks>LINQ query-syntax alias of the async <c>Map</c> extension.</remarks>
	public static Task<Suspicious<TResult>> Select<TValue, TResult>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, TResult> selector
	)
	where TValue : notnull
	where TResult : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(selector);

		return source.Map(selector);
	}

	/// <summary>Awaits the <paramref name="source" />, chains an async <paramref name="binder" /> and projects both values into a <typeparamref name="TResult" />.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="binder">The binder; must not produce <c>null</c>.</param>
	/// <param name="projector">The projector.</param>
	/// <typeparam name="TValue">The source value type.</typeparam>
	/// <typeparam name="TIntermediate">The intermediate value type.</typeparam>
	/// <typeparam name="TResult">The result value type.</typeparam>
	/// <returns>A task with a new <see cref="Suspicious{TResult}" />.</returns>
	/// <remarks>LINQ query-syntax form of the async <c>Then</c> extension — the async-first/async-second <c>from</c> pair.</remarks>
	public static Task<Suspicious<TResult>> SelectMany<TValue, TIntermediate, TResult>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, Task<Suspicious<TIntermediate>>> binder,
		Func<TValue, TIntermediate, TResult> projector
	)
	where TValue : notnull
	where TIntermediate : notnull
	where TResult : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(binder);
		ArgumentExceptionExtensions.ThrowIfNull(projector);

		return source.Then(value => binder(value).Map(intermediate => projector(value, intermediate)));
	}

	/// <summary>Awaits the <paramref name="source" />, chains a sync <paramref name="binder" /> and projects both values into a <typeparamref name="TResult" />.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="binder">The binder.</param>
	/// <param name="projector">The projector.</param>
	/// <typeparam name="TValue">The source value type.</typeparam>
	/// <typeparam name="TIntermediate">The intermediate value type.</typeparam>
	/// <typeparam name="TResult">The result value type.</typeparam>
	/// <returns>A task with a new <see cref="Suspicious{TResult}" />.</returns>
	/// <remarks>The async-first/sync-second <c>from</c> pair.</remarks>
	public static Task<Suspicious<TResult>> SelectMany<TValue, TIntermediate, TResult>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, Suspicious<TIntermediate>> binder,
		Func<TValue, TIntermediate, TResult> projector
	)
	where TValue : notnull
	where TIntermediate : notnull
	where TResult : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(binder);
		ArgumentExceptionExtensions.ThrowIfNull(projector);

		return source.Then(value => binder(value).Map(intermediate => projector(value, intermediate)));
	}

	/// <summary>Chains an async <paramref name="binder" /> and projects both values into a <typeparamref name="TResult" />.</summary>
	/// <param name="source">The source.</param>
	/// <param name="binder">The binder; must not produce <c>null</c>.</param>
	/// <param name="projector">The projector.</param>
	/// <typeparam name="TValue">The source value type.</typeparam>
	/// <typeparam name="TIntermediate">The intermediate value type.</typeparam>
	/// <typeparam name="TResult">The result value type.</typeparam>
	/// <returns>A task with a new <see cref="Suspicious{TResult}" />.</returns>
	/// <remarks>The sync-first/async-second <c>from</c> pair.</remarks>
	public static Task<Suspicious<TResult>> SelectMany<TValue, TIntermediate, TResult>
	(
		this Suspicious<TValue> source,
		Func<TValue, Task<Suspicious<TIntermediate>>> binder,
		Func<TValue, TIntermediate, TResult> projector
	)
	where TValue : notnull
	where TIntermediate : notnull
	where TResult : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(binder);
		ArgumentExceptionExtensions.ThrowIfNull(projector);

		return source.Then(value => binder(value).Map(intermediate => projector(value, intermediate)));
	}
}