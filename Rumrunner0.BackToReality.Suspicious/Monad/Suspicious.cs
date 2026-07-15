using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Rumrunner0.BackToReality.SharedExtensions.Collections;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;
using Rumrunner0.BackToReality.Suspicious.Serialization;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Result monad without a value that represents the outcome of a void-like operation.</summary>
/// <remarks>
/// <para>* Hosts the static factories for <see cref="Suspicious{TValue}" />.</para>
/// <para>* Success is a per-instance fact which means an instance is a success iff no <see cref="Error" /> is attached.</para>
/// </remarks>
[JsonConverter(typeof(SuspiciousJsonConverter))]
public sealed partial class Suspicious
{
	#region Instance State

	/// <summary>Outcome.</summary>
	private readonly OutcomeKind _outcome;

	/// <summary>Error.</summary>
	private readonly Error? _error;

	/// <inheritdoc cref="Suspicious" />
	private Suspicious(OutcomeKind outcome)
	{
		ArgumentExceptionExtensions.ThrowIfNull(outcome);
		if (!outcome.Side.AllowsSuccess) ArgumentExceptionExtensions.Throw($"The kind {outcome} doesn't allow the success side", nameof(outcome));

		this._outcome = outcome;
		this._error = null;
	}

	/// <inheritdoc cref="Suspicious" />
	private Suspicious(Error error)
	{
		ArgumentExceptionExtensions.ThrowIfNull(error);

		this._outcome = error.Kind;
		this._error = error;
	}

	#endregion

	#region Common API

	/// <summary>Outcome.</summary>
	public OutcomeKind Outcome => this._outcome;

	/// <summary>Error.</summary>
	/// <remarks>Non-<c>null</c> iff this <see cref="Suspicious" /> is a failure.</remarks>
	public Error? Error => this._error;

	/// <summary>Flag that indicates whether this <see cref="Suspicious" /> is a success (no <see cref="Error" /> is attached).</summary>
	[MemberNotNullWhen(false, nameof(_error))]
	[MemberNotNullWhen(false, nameof(Error))]
	public bool IsSuccess => this._error is null;

	/// <summary>Flag that indicates whether this <see cref="Suspicious" /> is a failure (an <see cref="Error" /> is attached).</summary>
	[MemberNotNullWhen(true, nameof(_error))]
	[MemberNotNullWhen(true, nameof(Error))]
	public bool IsFailure => this._error is not null;

	#endregion

	#region Display

	/// <summary>Prints members.</summary>
	/// <param name="builder">The <see cref="StringBuilder" />.</param>
	/// <returns><c>true</c> if members should be printed; <c>false</c> otherwise.</returns>
	private bool PrintMembers(StringBuilder builder)
	{
		builder.Append($"Outcome = {this._outcome}");
		if (this._error is not null) builder.Append($", Error = {this._error}");

		return true;
	}

	/// <inheritdoc />
	public override string ToString()
	{
		var builder = new StringBuilder();

		builder.Append($"{nameof(Suspicious)} {{ ");
		if (this.PrintMembers(builder)) builder.Append(' ');
		builder.Append('}');

		return builder.ToString();
	}

	#endregion

	#region Creation (Unit)

	/// <summary>Cached <see cref="OutcomeKind.Ok" /> <see cref="Suspicious" />.</summary>
	private static readonly Suspicious _ok = new (OutcomeKind.Ok);

	/// <summary>Creates an <see cref="OutcomeKind.Ok" /> <see cref="Suspicious" />.</summary>
	/// <returns>The cached <see cref="OutcomeKind.Ok" /> <see cref="Suspicious" />.</returns>
	public static Suspicious Ok() => _ok;

	/// <summary>Creates a successful <see cref="Suspicious" /> with the provided <paramref name="kind" />.</summary>
	/// <param name="kind">The kind that must allow the success <see cref="OutcomeKind.Side" />.</param>
	/// <returns>A new successful <see cref="Suspicious" />.</returns>
	public static Suspicious Success(OutcomeKind kind) => new (kind);

	/// <summary>Creates a failed <see cref="Suspicious" /> from an <paramref name="error" />.</summary>
	/// <param name="error">The error.</param>
	/// <returns>A new failed <see cref="Suspicious" /> whose <see cref="Outcome" /> is taken from <paramref name="error" />.</returns>
	public static Suspicious Fail(Error error) => new (error);

	#endregion

	#region Creation (Generic)

	/// <summary>Creates an <see cref="OutcomeKind.Ok" /> <see cref="Suspicious{TValue}" /> from a <paramref name="value" />.</summary>
	/// <param name="value">The value.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A new <see cref="OutcomeKind.Ok" /> <see cref="Suspicious{TValue}" />.</returns>
	public static Suspicious<TValue> Ok<TValue>(TValue value)
	where TValue : notnull
	{
		return Suspicious<TValue>.CreateSuccess(OutcomeKind.Ok, value);
	}

	/// <summary>Creates a successful miss <see cref="OutcomeKind.NoValue" /> <see cref="Suspicious{TValue}" />.</summary>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>The cached <see cref="OutcomeKind.NoValue" /> <see cref="Suspicious{TValue}" />.</returns>
	/// <remarks>The home rail of <see cref="OutcomeKind.NoValue" /> is success (a plain miss). For a miss the producer treats as a failure, use <c>Fail&lt;TValue&gt;(Error.NoValue(…))</c> (the failure rail is the explicit opt-in).</remarks>
	public static Suspicious<TValue> NoValue<TValue>()
	where TValue : notnull
	{
		return Suspicious<TValue>.NoValue;
	}

	/// <summary>Creates a successful <see cref="Suspicious{TValue}" /> with the provided <paramref name="kind" /> and <paramref name="value" />.</summary>
	/// <param name="kind">The kind that must allow the success <see cref="OutcomeKind.Side" />.</param>
	/// <param name="value">The value.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A new successful <see cref="Suspicious{TValue}" />.</returns>
	public static Suspicious<TValue> Success<TValue>(OutcomeKind kind, TValue value)
	where TValue : notnull
	{
		return Suspicious<TValue>.CreateSuccess(kind, value);
	}

	/// <summary>Creates a successful <see cref="Suspicious{TValue}" /> with the provided <paramref name="kind" /> and no value.</summary>
	/// <param name="kind">The kind that must allow the success <see cref="OutcomeKind.Side" />.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A new successful <see cref="Suspicious{TValue}" /> without a value.</returns>
	public static Suspicious<TValue> Success<TValue>(OutcomeKind kind)
	where TValue : notnull
	{
		return Suspicious<TValue>.CreateSuccess(kind);
	}

	/// <summary>Creates a failed <see cref="Suspicious{TValue}" /> from an <paramref name="error" />.</summary>
	/// <param name="error">The error.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A new failed <see cref="Suspicious{TValue}" /> whose <see cref="Outcome" /> is taken from <paramref name="error" />.</returns>
	public static Suspicious<TValue> Fail<TValue>(Error error)
	where TValue : notnull
	{
		return Suspicious<TValue>.CreateFailure(error);
	}

	#endregion

	#region Aggregation

	/// <summary>Combines multiple <see cref="Suspicious" /> into one that indicates whether all of them succeeded.</summary>
	/// <param name="results">The results that must not be empty.</param>
	/// <returns><see cref="Ok()" /> if all results are successes; a failed <see cref="Suspicious" /> carrying the single <see cref="Error" /> or an <see cref="Monad.Error.Aggregate" /> of all of them, otherwise.</returns>
	public static Suspicious Combine(params IEnumerable<Suspicious> results)
	{
		ArgumentExceptionExtensions.ThrowIfNull(results);

		var errors = new List<Error>();
		var isEmpty = true;

		foreach (var result in results)
		{
			isEmpty = false;
			if (result.IsFailure) errors.Add(result.Error);
		}

		if (isEmpty) ArgumentExceptionExtensions.Throw("At least one result is required", nameof(results));

		return errors.Count switch
		{
			0 => Ok(),
			1 => Fail(errors[0]),
			_ => Fail(Error.Aggregate(errors))
		};
	}

	/// <summary>Combines multiple <see cref="Suspicious{TValue}" /> into a unit <see cref="Suspicious" /> that indicates whether all of them succeeded.</summary>
	/// <param name="results">The results that must not be empty.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns><see cref="Ok()" /> if all results are successes; a failed <see cref="Suspicious" /> carrying the single <see cref="Error" /> or an <see cref="Monad.Error.Aggregate" /> of all of them, otherwise.</returns>
	/// <remarks>Values are discarded.</remarks>
	public static Suspicious Combine<TValue>(params IEnumerable<Suspicious<TValue>> results) where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(results);

		var errors = new List<Error>();
		var isEmpty = true;

		foreach (var result in results)
		{
			isEmpty = false;
			if (result.IsFailure) errors.Add(result.Error);
		}

		if (isEmpty) ArgumentExceptionExtensions.Throw("At least one result is required", nameof(results));

		return errors.Count switch
		{
			0 => Ok(),
			1 => Fail(errors[0]),
			_ => Fail(Error.Aggregate(errors))
		};
	}

	/// <summary>Combines multiple <see cref="Suspicious" /> tasks into one that indicates whether all of them succeeded.</summary>
	/// <param name="results">The result tasks that must not be empty.</param>
	/// <param name="ct">The cancellation token that cancels the wait (not the underlying tasks).</param>
	/// <returns>A task with <see cref="Ok()" /> if all results are successes; a failed <see cref="Suspicious" /> carrying the single <see cref="Error" /> or an <see cref="Monad.Error.Aggregate" /> of all of them, otherwise.</returns>
	/// <remarks>A faulted input task faults the combined task (exceptions are never converted into results).</remarks>
	public static Task<Suspicious> Combine(IEnumerable<Task<Suspicious>> results, CancellationToken ct = default)
	{
		ArgumentExceptionExtensions.ThrowIfNull(results);

		var tasks = results as Task<Suspicious>[] ?? results.ToArray();
		if (tasks.None()) ArgumentExceptionExtensions.Throw("At least one result is required", nameof(results));

		return Core(tasks, ct);

		static async Task<Suspicious> Core(Task<Suspicious>[] tasks, CancellationToken ct)
		{
			var all = Task.WhenAll(tasks);
			var results = ct.CanBeCanceled ? await all.WaitAsync(ct).ConfigureAwait(false) : await all.ConfigureAwait(false);

			return Combine(results);
		}
	}

	/// <summary>Combines multiple <see cref="Suspicious{TValue}" /> tasks into a unit <see cref="Suspicious" /> that indicates whether all of them succeeded.</summary>
	/// <param name="results">The result tasks that must not be empty.</param>
	/// <param name="ct">The cancellation token that cancels the wait (not the underlying tasks).</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A task with <see cref="Ok()" /> if all results are successes; a failed <see cref="Suspicious" /> carrying the single <see cref="Error" /> or an <see cref="Monad.Error.Aggregate" /> of all of them, otherwise.</returns>
	/// <remarks>Values are discarded. A faulted input task faults the combined task (exceptions are never converted into results).</remarks>
	public static Task<Suspicious> Combine<TValue>(IEnumerable<Task<Suspicious<TValue>>> results, CancellationToken ct = default) where TValue : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(results);

		var tasks = results as Task<Suspicious<TValue>>[] ?? results.ToArray();
		if (tasks.None()) ArgumentExceptionExtensions.Throw("At least one result is required", nameof(results));

		return Core(tasks, ct);

		static async Task<Suspicious> Core(Task<Suspicious<TValue>>[] tasks, CancellationToken ct)
		{
			var all = Task.WhenAll(tasks);
			var results = ct.CanBeCanceled ? await all.WaitAsync(ct).ConfigureAwait(false) : await all.ConfigureAwait(false);

			return Combine(results);
		}
	}

	#endregion
}