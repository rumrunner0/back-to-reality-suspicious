using System;
using System.Threading;
using System.Threading.Tasks;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Async Conversion extensions for <c>Task</c>-wrapped unit <see cref="Suspicious" /> results.</summary>
public static class SuspiciousAsyncConversionExtensions
{
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
}