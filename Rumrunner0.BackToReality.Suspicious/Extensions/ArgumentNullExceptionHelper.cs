using System;
using System.Runtime.CompilerServices;

namespace Rumrunner0.BackToReality.Suspicious.Extensions;

/// <summary>
/// Helper for <see cref="ArgumentNullException" />.
/// </summary>
internal static class ArgumentNullExceptionHelper
{
	/// <summary>
	/// Throws, if a <paramref name="source" /> is <c>null</c>.
	/// </summary>
	/// <param name="source">The source.</param>
	/// <param name="argumentName">The name of the <paramref name="source" /> argument.</param>
	/// <exception cref="ArgumentNullException">If the <paramref name="source" /> is <c>null</c>.</exception>
	internal static void ThrowIfNull(object? source, [CallerArgumentExpression("source")] string? argumentName = null)
	{
		if (source is null) throw new ArgumentNullException(argumentName);
	}
}