using System;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>LINQ query-syntax extensions for <see cref="Suspicious{TValue}" />.</summary>
/// <remarks>Enable query syntax like <c>from u in FindUser(id) from o in LoadOrders(u) select Report.From(u, o)</c> — C#'s do-notation over the monad.</remarks>
public static class SuspiciousLinqExtensions
{
	/// <summary>Maps the value into a <typeparamref name="TResult" /> — the query-syntax alias of <see cref="Suspicious{TValue}.Map{TResult}" />.</summary>
	/// <param name="source">The source.</param>
	/// <param name="selector">The selector.</param>
	/// <typeparam name="TValue">The source value type.</typeparam>
	/// <typeparam name="TResult">The result value type.</typeparam>
	/// <returns>A new <see cref="Suspicious{TResult}" />.</returns>
	public static Suspicious<TResult> Select<TValue, TResult>
	(
		this Suspicious<TValue> source,
		Func<TValue, TResult> selector
	)
		where TValue : notnull
		where TResult : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(selector);

		return source.Map(selector);
	}

	/// <summary>Chains a <paramref name="binder" /> and projects both values into a <typeparamref name="TResult" /> — the query-syntax form of <see cref="Suspicious{TValue}.Then{TResult}" />.</summary>
	/// <param name="source">The source.</param>
	/// <param name="binder">The binder.</param>
	/// <param name="projector">The projector.</param>
	/// <typeparam name="TValue">The source value type.</typeparam>
	/// <typeparam name="TIntermediate">The intermediate value type.</typeparam>
	/// <typeparam name="TResult">The result value type.</typeparam>
	/// <returns>A new <see cref="Suspicious{TResult}" />.</returns>
	public static Suspicious<TResult> SelectMany<TValue, TIntermediate, TResult>
	(
		this Suspicious<TValue> source,
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
}