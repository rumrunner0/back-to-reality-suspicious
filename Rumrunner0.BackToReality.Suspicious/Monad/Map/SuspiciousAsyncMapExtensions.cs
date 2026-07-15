using System;
using System.Threading;
using System.Threading.Tasks;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Async Map extensions for the unit <see cref="Suspicious" /> and <c>Task</c>-wrapped unit results.</summary>
public static class SuspiciousAsyncMapExtensions
{
	/// <summary>Maps the error of a failure with an async <paramref name="mapper" />; a success is returned unchanged.</summary>
	/// <param name="source">The source.</param>
	/// <param name="mapper">The mapper; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before the <paramref name="mapper" /> runs.</param>
	/// <returns>A task with a new <see cref="Suspicious" /> carrying the mapped error, or the unchanged success.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the <paramref name="mapper" /> is <c>null</c> (synchronously), or if it produces <c>null</c> (in the returned task).</exception>
	public static Task<Suspicious> MapError
	(
		this Suspicious source,
		Func<Error, Task<Error>> mapper,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(mapper);

		if (source.Error is not { } error) return Task.FromResult(source);
		return Core(source, error, mapper, cancellationToken);

		static async Task<Suspicious> Core(Suspicious source, Error error, Func<Error, Task<Error>> mapper, CancellationToken cancellationToken)
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
	/// <returns>A task with a new <see cref="Suspicious" /> carrying the mapped error, or the unchanged success.</returns>
	public static Task<Suspicious> MapError
	(
		this Suspicious source,
		Func<Error, CancellationToken, Task<Error>> mapper,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(mapper);
		return source.MapError(error => mapper(error, cancellationToken), cancellationToken);
	}

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
}