using System;
using System.Threading;
using System.Threading.Tasks;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Async Map extensions for <see cref="Suspicious{TValue}" /> and <c>Task</c>-wrapped generic results.</summary>
/// <remarks>The plain-source async <c>Map</c> keeps its token-less and token-taking forms as SEPARATE overloads (not one optional parameter) — the "no defaults substituted" tie-breaker would otherwise make the sync <c>Map</c> win for task-returning mappers.</remarks>
public static class SuspiciousOfTValueAsyncMapExtensions
{
	/// <summary>Maps the value with an async <paramref name="mapper" />; valueless results are propagated unchanged.</summary>
	/// <param name="source">The source.</param>
	/// <param name="mapper">The mapper; must not produce <c>null</c>.</param>
	/// <typeparam name="TValue">The source value type.</typeparam>
	/// <typeparam name="TResult">The result value type.</typeparam>
	/// <returns>A task with a new <see cref="Suspicious{TResult}" />.</returns>
	/// <remarks>A success with a value keeps its outcome (custom success kinds are preserved); a success without a value and a failure short-circuit.</remarks>
	/// <exception cref="ArgumentNullException">Thrown if the <paramref name="mapper" /> is <c>null</c> (synchronously), or if it produces <c>null</c> (in the returned task).</exception>
	public static Task<Suspicious<TResult>> Map<TValue, TResult>
	(
		this Suspicious<TValue> source,
		Func<TValue, Task<TResult>> mapper
	)
	where TValue : notnull
	where TResult : notnull
	{
		return source.Map(mapper, CancellationToken.None);
	}

	/// <summary>Maps the value with an async <paramref name="mapper" />; valueless results are propagated unchanged.</summary>
	/// <param name="source">The source.</param>
	/// <param name="mapper">The mapper; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before the <paramref name="mapper" /> runs.</param>
	/// <typeparam name="TValue">The source value type.</typeparam>
	/// <typeparam name="TResult">The result value type.</typeparam>
	/// <returns>A task with a new <see cref="Suspicious{TResult}" />.</returns>
	/// <remarks>A success with a value keeps its outcome (custom success kinds are preserved); a success without a value and a failure short-circuit.</remarks>
	/// <exception cref="ArgumentNullException">Thrown if the <paramref name="mapper" /> is <c>null</c> (synchronously), or if it produces <c>null</c> (in the returned task).</exception>
	public static Task<Suspicious<TResult>> Map<TValue, TResult>
	(
		this Suspicious<TValue> source,
		Func<TValue, Task<TResult>> mapper,
		CancellationToken cancellationToken
	)
	where TValue : notnull
	where TResult : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(mapper);

		if (!source.HasValue) return Task.FromResult(source.IsFailure ? source.AsFailure<TResult>() : Suspicious.Success<TResult>(source.Outcome));
		return Core(source, mapper, cancellationToken);

		static async Task<Suspicious<TResult>> Core(Suspicious<TValue> source, Func<TValue, Task<TResult>> mapper, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var task = mapper(source.Value);
			if (task is null) throw new ArgumentNullException(nameof(mapper), "The mapper produced null");

			var mapped = await task.ConfigureAwait(false);
			return source.Map(_ => mapped);
		}
	}

	/// <summary>Maps the value with an async token-receiving <paramref name="mapper" />; valueless results are propagated unchanged.</summary>
	/// <param name="source">The source.</param>
	/// <param name="mapper">The mapper; receives the <paramref name="cancellationToken" />; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before the <paramref name="mapper" /> runs and passed into it.</param>
	/// <typeparam name="TValue">The source value type.</typeparam>
	/// <typeparam name="TResult">The result value type.</typeparam>
	/// <returns>A task with a new <see cref="Suspicious{TResult}" />.</returns>
	public static Task<Suspicious<TResult>> Map<TValue, TResult>
	(
		this Suspicious<TValue> source,
		Func<TValue, CancellationToken, Task<TResult>> mapper,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	where TResult : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(mapper);
		return source.Map(value => mapper(value, cancellationToken), cancellationToken);
	}

	/// <summary>Maps the error of a failure with an async <paramref name="mapper" />; a success is returned unchanged.</summary>
	/// <param name="source">The source.</param>
	/// <param name="mapper">The mapper; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before the <paramref name="mapper" /> runs.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with a new <see cref="Suspicious{TValue}" /> carrying the mapped error, or the unchanged success.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the <paramref name="mapper" /> is <c>null</c> (synchronously), or if it produces <c>null</c> (in the returned task).</exception>
	public static Task<Suspicious<TValue>> MapError<TValue>
	(
		this Suspicious<TValue> source,
		Func<Error, Task<Error>> mapper,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(mapper);

		if (source.Error is not { } error) return Task.FromResult(source);
		return Core(source, error, mapper, cancellationToken);

		static async Task<Suspicious<TValue>> Core(Suspicious<TValue> source, Error error, Func<Error, Task<Error>> mapper, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var task = mapper(error);
			if (task is null) throw new ArgumentNullException(nameof(mapper), "The mapper produced null");

			var mapped = await task.ConfigureAwait(false);
			return source.MapError(_ => mapped);
		}
	}

	/// <summary>Maps the error of a failure with an async token-receiving <paramref name="mapper" />; a success is returned unchanged.</summary>
	/// <param name="source">The source.</param>
	/// <param name="mapper">The mapper; receives the <paramref name="cancellationToken" />; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before the <paramref name="mapper" /> runs and passed into it.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with a new <see cref="Suspicious{TValue}" /> carrying the mapped error, or the unchanged success.</returns>
	public static Task<Suspicious<TValue>> MapError<TValue>
	(
		this Suspicious<TValue> source,
		Func<Error, CancellationToken, Task<Error>> mapper,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(mapper);
		return source.MapError(error => mapper(error, cancellationToken), cancellationToken);
	}

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
}