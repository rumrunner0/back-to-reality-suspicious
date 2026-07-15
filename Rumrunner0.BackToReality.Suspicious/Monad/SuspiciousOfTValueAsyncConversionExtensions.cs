using System;
using System.Threading;
using System.Threading.Tasks;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Async Conversion extensions for <c>Task</c>-wrapped <see cref="Suspicious{TValue}" /> results.</summary>
public static class SuspiciousOfTValueAsyncConversionExtensions
{
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
}