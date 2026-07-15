using System;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Conversion extensions for <see cref="Suspicious{TValue}" /> — re-typing across the value axis.</summary>
/// <remarks>The sync <c>AsFailure</c> stays an instance member: as an extension it would need TWO explicit type arguments (<c>TResult</c> is return-only and C# has no partial type-argument inference).</remarks>
public static class SuspiciousOfTValueConversionExtensions
{
	/// <summary>Drops the value axis, keeping the outcome and the error.</summary>
	/// <param name="source">The source.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A unit <see cref="Suspicious" />.</returns>
	public static Suspicious AsUnit<TValue>
	(
		this Suspicious<TValue> source
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		return source.Error is { } error ? Suspicious.Fail(error) : Suspicious.Success(source.Outcome);
	}
}