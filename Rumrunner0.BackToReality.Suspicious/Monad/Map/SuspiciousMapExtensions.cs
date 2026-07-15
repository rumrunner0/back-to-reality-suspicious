using System;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Map extensions for the unit <see cref="Suspicious" /> — transforming the error axis.</summary>
public static class SuspiciousMapExtensions
{
	/// <summary>Maps the error of a failure; a success is returned unchanged.</summary>
	/// <param name="source">The source.</param>
	/// <param name="mapper">The mapper.</param>
	/// <returns>A new <see cref="Suspicious" /> with the mapped error, or this instance if it is a success.</returns>
	public static Suspicious MapError
	(
		this Suspicious source,
		Func<Error, Error> mapper
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(mapper);

		return source.Error is { } error ? Suspicious.Fail(mapper(error)) : source;
	}
}