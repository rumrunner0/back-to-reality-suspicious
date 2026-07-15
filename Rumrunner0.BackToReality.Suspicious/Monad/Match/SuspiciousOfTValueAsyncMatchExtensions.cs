using System;
using System.Threading;
using System.Threading.Tasks;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Async Match extensions for <see cref="Suspicious{TValue}" /> and <c>Task</c>-wrapped generic results.</summary>
/// <remarks>The plain-source async <c>Match</c> keeps its token-less and token-taking forms as SEPARATE overloads (not one optional parameter) — the "no defaults substituted" tie-breaker would otherwise make the sync <c>Match</c> win for task-returning handlers.</remarks>
public static class SuspiciousOfTValueAsyncMatchExtensions
{
	/// <summary>Matches this <see cref="Suspicious{TValue}" /> into a <typeparamref name="TResult" /> with async handlers.</summary>
	/// <param name="source">The source.</param>
	/// <param name="onValue">The handler for a success with a value; must not produce <c>null</c>.</param>
	/// <param name="onError">The handler for the failure rail; must not produce <c>null</c>.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <typeparam name="TResult">The result type.</typeparam>
	/// <returns>A task with the result of the invoked handler.</returns>
	/// <remarks>Use this overload only in flows where a success without a value can't occur; otherwise use the overload with an <c>onNoValue</c> handler.</remarks>
	/// <exception cref="InvalidOperationException">Thrown (in the returned task) if this <see cref="Suspicious{TValue}" /> is a success without a value — a contract violation of this overload.</exception>
	public static Task<TResult> Match<TValue, TResult>
	(
		this Suspicious<TValue> source,
		Func<TValue, Task<TResult>> onValue,
		Func<Error, Task<TResult>> onError
	)
	where TValue : notnull
	{
		return source.Match(onValue, onError, CancellationToken.None);
	}

	/// <summary>Matches this <see cref="Suspicious{TValue}" /> into a <typeparamref name="TResult" /> with async handlers.</summary>
	/// <param name="source">The source.</param>
	/// <param name="onValue">The handler for a success with a value; must not produce <c>null</c>.</param>
	/// <param name="onError">The handler for the failure rail; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before a handler runs.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <typeparam name="TResult">The result type.</typeparam>
	/// <returns>A task with the result of the invoked handler.</returns>
	/// <remarks>Use this overload only in flows where a success without a value can't occur; otherwise use the overload with an <c>onNoValue</c> handler.</remarks>
	/// <exception cref="InvalidOperationException">Thrown (in the returned task) if this <see cref="Suspicious{TValue}" /> is a success without a value — a contract violation of this overload.</exception>
	public static Task<TResult> Match<TValue, TResult>
	(
		this Suspicious<TValue> source,
		Func<TValue, Task<TResult>> onValue,
		Func<Error, Task<TResult>> onError,
		CancellationToken cancellationToken
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		return Core(source, onValue, onError, cancellationToken);

		static async Task<TResult> Core(Suspicious<TValue> source, Func<TValue, Task<TResult>> onValue, Func<Error, Task<TResult>> onError, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (source.HasValue)
			{
				var task = onValue(source.Value);
				if (task is null) throw new ArgumentNullException(nameof(onValue), "The handler produced null");
				return await task.ConfigureAwait(false);
			}

			if (source.Error is { } error)
			{
				var task = onError(error);
				if (task is null) throw new ArgumentNullException(nameof(onError), "The handler produced null");
				return await task.ConfigureAwait(false);
			}

			throw new InvalidOperationException($"The {nameof(Suspicious<TValue>)} is a success without a value; use the {nameof(Match)} overload with an 'onNoValue' handler");
		}
	}

	/// <summary>Matches this <see cref="Suspicious{TValue}" /> into a <typeparamref name="TResult" /> with async handlers.</summary>
	/// <param name="source">The source.</param>
	/// <param name="onValue">The handler for a success with a value; must not produce <c>null</c>.</param>
	/// <param name="onNoValue">The handler for a success without a value; must not produce <c>null</c>.</param>
	/// <param name="onError">The handler for the failure rail; must not produce <c>null</c>.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <typeparam name="TResult">The result type.</typeparam>
	/// <returns>A task with the result of the invoked handler.</returns>
	public static Task<TResult> Match<TValue, TResult>
	(
		this Suspicious<TValue> source,
		Func<TValue, Task<TResult>> onValue,
		Func<Task<TResult>> onNoValue,
		Func<Error, Task<TResult>> onError
	)
	where TValue : notnull
	{
		return source.Match(onValue, onNoValue, onError, CancellationToken.None);
	}

	/// <summary>Matches this <see cref="Suspicious{TValue}" /> into a <typeparamref name="TResult" /> with async handlers.</summary>
	/// <param name="source">The source.</param>
	/// <param name="onValue">The handler for a success with a value; must not produce <c>null</c>.</param>
	/// <param name="onNoValue">The handler for a success without a value; must not produce <c>null</c>.</param>
	/// <param name="onError">The handler for the failure rail; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before a handler runs.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <typeparam name="TResult">The result type.</typeparam>
	/// <returns>A task with the result of the invoked handler.</returns>
	public static Task<TResult> Match<TValue, TResult>
	(
		this Suspicious<TValue> source,
		Func<TValue, Task<TResult>> onValue,
		Func<Task<TResult>> onNoValue,
		Func<Error, Task<TResult>> onError,
		CancellationToken cancellationToken
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onNoValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		return Core(source, onValue, onNoValue, onError, cancellationToken);

		static async Task<TResult> Core(Suspicious<TValue> source, Func<TValue, Task<TResult>> onValue, Func<Task<TResult>> onNoValue, Func<Error, Task<TResult>> onError, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (source.HasValue)
			{
				var task = onValue(source.Value);
				if (task is null) throw new ArgumentNullException(nameof(onValue), "The handler produced null");
				return await task.ConfigureAwait(false);
			}

			if (source.Error is { } error)
			{
				var task = onError(error);
				if (task is null) throw new ArgumentNullException(nameof(onError), "The handler produced null");
				return await task.ConfigureAwait(false);
			}

			var noValueTask = onNoValue();
			if (noValueTask is null) throw new ArgumentNullException(nameof(onNoValue), "The handler produced null");
			return await noValueTask.ConfigureAwait(false);
		}
	}

	/// <summary>Switches on this <see cref="Suspicious{TValue}" /> with async handlers.</summary>
	/// <param name="source">The source.</param>
	/// <param name="onValue">The handler for a success with a value; must not produce <c>null</c>.</param>
	/// <param name="onError">The handler for the failure rail; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before a handler runs.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task that completes when the invoked handler completes.</returns>
	/// <remarks>Use this overload only in flows where a success without a value can't occur; otherwise use the overload with an <c>onNoValue</c> handler.</remarks>
	/// <exception cref="InvalidOperationException">Thrown (in the returned task) if this <see cref="Suspicious{TValue}" /> is a success without a value — a contract violation of this overload.</exception>
	public static Task Switch<TValue>
	(
		this Suspicious<TValue> source,
		Func<TValue, Task> onValue,
		Func<Error, Task> onError,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		return Core(source, onValue, onError, cancellationToken);

		static async Task Core(Suspicious<TValue> source, Func<TValue, Task> onValue, Func<Error, Task> onError, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (source.HasValue)
			{
				var task = onValue(source.Value);
				if (task is null) throw new ArgumentNullException(nameof(onValue), "The handler produced null");
				await task.ConfigureAwait(false);
				return;
			}

			if (source.Error is { } error)
			{
				var task = onError(error);
				if (task is null) throw new ArgumentNullException(nameof(onError), "The handler produced null");
				await task.ConfigureAwait(false);
				return;
			}

			throw new InvalidOperationException($"The {nameof(Suspicious<TValue>)} is a success without a value; use the {nameof(Switch)} overload with an 'onNoValue' handler");
		}
	}

	/// <summary>Switches on this <see cref="Suspicious{TValue}" /> with async handlers.</summary>
	/// <param name="source">The source.</param>
	/// <param name="onValue">The handler for a success with a value; must not produce <c>null</c>.</param>
	/// <param name="onNoValue">The handler for a success without a value; must not produce <c>null</c>.</param>
	/// <param name="onError">The handler for the failure rail; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked before a handler runs.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task that completes when the invoked handler completes.</returns>
	public static Task Switch<TValue>
	(
		this Suspicious<TValue> source,
		Func<TValue, Task> onValue,
		Func<Task> onNoValue,
		Func<Error, Task> onError,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onNoValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		return Core(source, onValue, onNoValue, onError, cancellationToken);

		static async Task Core(Suspicious<TValue> source, Func<TValue, Task> onValue, Func<Task> onNoValue, Func<Error, Task> onError, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (source.HasValue)
			{
				var task = onValue(source.Value);
				if (task is null) throw new ArgumentNullException(nameof(onValue), "The handler produced null");
				await task.ConfigureAwait(false);
				return;
			}

			if (source.Error is { } error)
			{
				var task = onError(error);
				if (task is null) throw new ArgumentNullException(nameof(onError), "The handler produced null");
				await task.ConfigureAwait(false);
				return;
			}

			var noValueTask = onNoValue();
			if (noValueTask is null) throw new ArgumentNullException(nameof(onNoValue), "The handler produced null");
			await noValueTask.ConfigureAwait(false);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and matches it into a <typeparamref name="TResult" /> with sync handlers.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="onValue">The handler for a success with a value.</param>
	/// <param name="onError">The handler for the failure rail.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <typeparam name="TResult">The result type.</typeparam>
	/// <returns>A task with the result of the invoked handler.</returns>
	/// <remarks>Use this overload only in flows where a success without a value can't occur; otherwise use the overload with an <c>onNoValue</c> handler.</remarks>
	public static Task<TResult> Match<TValue, TResult>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, TResult> onValue,
		Func<Error, TResult> onError,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		return Core(source, onValue, onError, cancellationToken);

		static async Task<TResult> Core(Task<Suspicious<TValue>> source, Func<TValue, TResult> onValue, Func<Error, TResult> onError, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();

			return result.Match(onValue, onError);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and matches it into a <typeparamref name="TResult" /> with sync handlers.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="onValue">The handler for a success with a value.</param>
	/// <param name="onNoValue">The handler for a success without a value.</param>
	/// <param name="onError">The handler for the failure rail.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <typeparam name="TResult">The result type.</typeparam>
	/// <returns>A task with the result of the invoked handler.</returns>
	public static Task<TResult> Match<TValue, TResult>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, TResult> onValue,
		Func<TResult> onNoValue,
		Func<Error, TResult> onError,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onNoValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		return Core(source, onValue, onNoValue, onError, cancellationToken);

		static async Task<TResult> Core(Task<Suspicious<TValue>> source, Func<TValue, TResult> onValue, Func<TResult> onNoValue, Func<Error, TResult> onError, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();

			return result.Match(onValue, onNoValue, onError);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and matches it into a <typeparamref name="TResult" /> with async handlers.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="onValue">The handler for a success with a value; must not produce <c>null</c>.</param>
	/// <param name="onError">The handler for the failure rail; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes and before a handler runs.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <typeparam name="TResult">The result type.</typeparam>
	/// <returns>A task with the result of the invoked handler.</returns>
	/// <remarks>Use this overload only in flows where a success without a value can't occur; otherwise use the overload with an <c>onNoValue</c> handler.</remarks>
	public static Task<TResult> Match<TValue, TResult>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, Task<TResult>> onValue,
		Func<Error, Task<TResult>> onError,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		return Core(source, onValue, onError, cancellationToken);

		static async Task<TResult> Core(Task<Suspicious<TValue>> source, Func<TValue, Task<TResult>> onValue, Func<Error, Task<TResult>> onError, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			return await result.Match(onValue, onError, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and matches it into a <typeparamref name="TResult" /> with async handlers.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="onValue">The handler for a success with a value; must not produce <c>null</c>.</param>
	/// <param name="onNoValue">The handler for a success without a value; must not produce <c>null</c>.</param>
	/// <param name="onError">The handler for the failure rail; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes and before a handler runs.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <typeparam name="TResult">The result type.</typeparam>
	/// <returns>A task with the result of the invoked handler.</returns>
	public static Task<TResult> Match<TValue, TResult>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, Task<TResult>> onValue,
		Func<Task<TResult>> onNoValue,
		Func<Error, Task<TResult>> onError,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onNoValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		return Core(source, onValue, onNoValue, onError, cancellationToken);

		static async Task<TResult> Core(Task<Suspicious<TValue>> source, Func<TValue, Task<TResult>> onValue, Func<Task<TResult>> onNoValue, Func<Error, Task<TResult>> onError, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			return await result.Match(onValue, onNoValue, onError, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and switches on it with sync handlers.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="onValue">The handler for a success with a value.</param>
	/// <param name="onError">The handler for the failure rail.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task that completes when the invoked handler completes.</returns>
	/// <remarks>Use this overload only in flows where a success without a value can't occur; otherwise use the overload with an <c>onNoValue</c> handler.</remarks>
	public static Task Switch<TValue>
	(
		this Task<Suspicious<TValue>> source,
		Action<TValue> onValue,
		Action<Error> onError,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		return Core(source, onValue, onError, cancellationToken);

		static async Task Core(Task<Suspicious<TValue>> source, Action<TValue> onValue, Action<Error> onError, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();

			result.Switch(onValue, onError);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and switches on it with sync handlers.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="onValue">The handler for a success with a value.</param>
	/// <param name="onNoValue">The handler for a success without a value.</param>
	/// <param name="onError">The handler for the failure rail.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task that completes when the invoked handler completes.</returns>
	public static Task Switch<TValue>
	(
		this Task<Suspicious<TValue>> source,
		Action<TValue> onValue,
		Action onNoValue,
		Action<Error> onError,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onNoValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		return Core(source, onValue, onNoValue, onError, cancellationToken);

		static async Task Core(Task<Suspicious<TValue>> source, Action<TValue> onValue, Action onNoValue, Action<Error> onError, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();

			result.Switch(onValue, onNoValue, onError);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and switches on it with async handlers.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="onValue">The handler for a success with a value; must not produce <c>null</c>.</param>
	/// <param name="onError">The handler for the failure rail; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes and before a handler runs.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task that completes when the invoked handler completes.</returns>
	/// <remarks>Use this overload only in flows where a success without a value can't occur; otherwise use the overload with an <c>onNoValue</c> handler.</remarks>
	public static Task Switch<TValue>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, Task> onValue,
		Func<Error, Task> onError,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		return Core(source, onValue, onError, cancellationToken);

		static async Task Core(Task<Suspicious<TValue>> source, Func<TValue, Task> onValue, Func<Error, Task> onError, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			await result.Switch(onValue, onError, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>Awaits the <paramref name="source" /> and switches on it with async handlers.</summary>
	/// <param name="source">The source task.</param>
	/// <param name="onValue">The handler for a success with a value; must not produce <c>null</c>.</param>
	/// <param name="onNoValue">The handler for a success without a value; must not produce <c>null</c>.</param>
	/// <param name="onError">The handler for the failure rail; must not produce <c>null</c>.</param>
	/// <param name="cancellationToken">The cancellation token; checked after the <paramref name="source" /> completes and before a handler runs.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task that completes when the invoked handler completes.</returns>
	public static Task Switch<TValue>
	(
		this Task<Suspicious<TValue>> source,
		Func<TValue, Task> onValue,
		Func<Task> onNoValue,
		Func<Error, Task> onError,
		CancellationToken cancellationToken = default
	)
	where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(source);
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onNoValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		return Core(source, onValue, onNoValue, onError, cancellationToken);

		static async Task Core(Task<Suspicious<TValue>> source, Func<TValue, Task> onValue, Func<Task> onNoValue, Func<Error, Task> onError, CancellationToken cancellationToken)
		{
			var result = await source.ConfigureAwait(false);
			await result.Switch(onValue, onNoValue, onError, cancellationToken).ConfigureAwait(false);
		}
	}
}