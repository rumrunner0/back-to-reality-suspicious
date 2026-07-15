using System;
using System.Threading;
using System.Threading.Tasks;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Async Conversion extensions for <c>Task</c>-wrapped <see cref="Suspicious{TValue}" /> results.</summary>
/// <remarks>There is deliberately NO <c>Task</c>-source <c>AsFailure</c>: it would force TWO explicit type arguments (the receiver's <c>TValue</c> becomes a method type parameter that inference can't supply alone), and a guard-gated re-typing needs an awaited result anyway — await first, then call the instance <c>AsFailure</c>.</remarks>
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
}