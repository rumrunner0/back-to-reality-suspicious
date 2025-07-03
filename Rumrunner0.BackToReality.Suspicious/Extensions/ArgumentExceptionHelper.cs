using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rumrunner0.BackToReality.SharedExtensions.Extensions;

namespace Rumrunner0.BackToReality.Suspicious.Extensions;

/// <summary>
/// Helper for <see cref="ArgumentException" />.
/// </summary>
internal static class ArgumentExceptionHelper
{
	/// <summary>
	/// Throws, if a <paramref name="source" /> is <c>null</c> or empty.
	/// </summary>
	/// <param name="source">The source.</param>
	/// <param name="argumentName">The name of the <paramref name="source" /> argument.</param>
	/// <typeparam name="T">The type of <paramref name="source" /> items.</typeparam>
	/// <exception cref="ArgumentNullException">If the <paramref name="source" /> is <c>null</c>.</exception>
	/// <exception cref="ArgumentException">If the <paramref name="source" /> is empty.</exception>
	internal static void ThrowIfNullOrEmpty<T>(ICollection<T> source, [CallerArgumentExpression("source")] string? argumentName = null)
	{
		ArgumentNullExceptionHelper.ThrowIfNull(source, argumentName);
		ArgumentExceptionHelper.ThrowIfEmpty(source, argumentName);
	}

	/// <summary>
	/// Throws, if a <paramref name="source" /> is empty.
	/// </summary>
	/// <param name="source">The source.</param>
	/// <param name="argumentName">The name of the <paramref name="source" /> argument.</param>
	/// <typeparam name="T">The type of the <paramref name="source" /> items.</typeparam>
	/// <exception cref="ArgumentException">If the <paramref name="source" /> is empty.</exception>
	internal static void ThrowIfEmpty<T>(ICollection<T> source, [CallerArgumentExpression("source")] string? argumentName = null)
	{
		if (source.None()) throw new ArgumentException($"{argumentName} is empty");
	}

	/// <summary>
	/// Throws, if a <paramref name="source" /> is empty.
	/// </summary>
	/// <param name="source">The source.</param>
	/// <param name="argumentName">The name of the <paramref name="source" /> argument.</param>
	/// <exception cref="ArgumentException">If the <paramref name="source" /> is empty.</exception>
	internal static void ThrowIfNullOrEmptyOrWhiteSpace(string source, [CallerArgumentExpression("source")] string? argumentName = null)
	{
		ArgumentNullExceptionHelper.ThrowIfNull(source, argumentName);
		ArgumentExceptionHelper.ThrowIfEmptyOrWhiteSpace(source, argumentName);
	}

	/// <summary>
	/// Throws, if a <paramref name="source" /> string is empty.
	/// </summary>
	/// <param name="source">The source.</param>
	/// <param name="argumentName">The name of the <paramref name="source" /> argument.</param>
	/// <exception cref="ArgumentException">If the <paramref name="source" /> is empty.</exception>
	internal static void ThrowIfEmptyOrWhiteSpace(string source, [CallerArgumentExpression("source")] string? argumentName = null)
	{
		if (source.IsEmptyOrWhitespace()) throw new ArgumentException($"{argumentName} is empty or whitespace");
	}
}