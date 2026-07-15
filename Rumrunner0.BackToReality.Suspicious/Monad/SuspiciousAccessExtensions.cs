using System;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Access extensions for the unit <see cref="Suspicious" /> — outcome queries.</summary>
public static class SuspiciousAccessExtensions
{
	/// <summary>Determines whether the outcome equals the provided <paramref name="kind" />.</summary>
	/// <param name="source">The source.</param>
	/// <param name="kind">The kind.</param>
	/// <returns><c>true</c>, if the outcomes are equal; <c>false</c>, otherwise.</returns>
	public static bool Is
	(
		this Suspicious source,
		OutcomeKind kind
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(kind);

		return source.Outcome == kind;
	}
}