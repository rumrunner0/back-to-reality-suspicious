using System;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Conversion extensions for the unit <see cref="Suspicious" /> — re-typing across the value axis.</summary>
public static class SuspiciousConversionExtensions
{
	/// <summary>Reinterprets this failed <see cref="Suspicious" /> as a failed <see cref="Suspicious{TValue}" /> (the error is carried over).</summary>
	/// <param name="source">The source.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A new failed <see cref="Suspicious{TValue}" /> with the same error.</returns>
	/// <remarks>Total on the failure rail only (a success has no value to lift); the guard-style call site is <c>if (result.IsFailure) return result.AsFailure&lt;TValue&gt;();</c>.</remarks>
	/// <exception cref="InvalidOperationException">Thrown if this <see cref="Suspicious" /> is a success (converting a success is a contract violation).</exception>
	public static Suspicious<TValue> AsFailure<TValue>
	(
		this Suspicious source
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);

		if (source.Error is { } error) return Suspicious<TValue>.CreateFailure(error);
		throw new InvalidOperationException($"The {nameof(Suspicious)} is a success; {nameof(AsFailure)} requires a failure");
	}
}