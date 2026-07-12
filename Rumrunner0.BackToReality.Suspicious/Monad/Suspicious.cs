using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;
using Rumrunner0.BackToReality.Suspicious.Serialization;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Result monad without a value that represents the outcome of a void-like operation.</summary>
/// <remarks>Also hosts the static factories for <see cref="Suspicious{TValue}" />. Success is a per-instance fact which means an instance is a success iff no <see cref="Error" /> is attached.</remarks>
[JsonConverter(typeof(SuspiciousJsonConverter))]
public sealed class Suspicious
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

	/// <summary>Flag that indicates whether this <see cref="Suspicious" /> is a success (no <see cref="Error" /> is attached).</summary>
	[MemberNotNullWhen(false, nameof(_error))]
	[MemberNotNullWhen(false, nameof(Error))]
	public bool IsSuccess => this._error is null;

	/// <summary>Flag that indicates whether this <see cref="Suspicious" /> is a failure (an <see cref="Error" /> is attached).</summary>
	[MemberNotNullWhen(true, nameof(_error))]
	[MemberNotNullWhen(true, nameof(Error))]
	public bool IsFailure => this._error is not null;

	/// <summary>Error.</summary>
	/// <remarks>Non-<c>null</c> iff this <see cref="Suspicious" /> is a failure.</remarks>
	public Error? Error => this._error;

	/// <summary>Determines whether the <see cref="Outcome" /> equals the provided <paramref name="kind" />.</summary>
	/// <param name="kind">The kind.</param>
	/// <returns><c>true</c>, if the outcomes are equal; <c>false</c>, otherwise.</returns>
	public bool Is(OutcomeKind kind)
	{
		ArgumentExceptionExtensions.ThrowIfNull(kind);
		return this._outcome == kind;
	}

	/// <summary>Matches this <see cref="Suspicious" /> into a <typeparamref name="TResult" />.</summary>
	/// <param name="onSuccess">The handler for the success rail.</param>
	/// <param name="onError">The handler for the failure rail.</param>
	/// <typeparam name="TResult">The result type.</typeparam>
	/// <returns>The result of the invoked handler.</returns>
	public TResult Match<TResult>(Func<TResult> onSuccess, Func<Error, TResult> onError)
	{
		ArgumentExceptionExtensions.ThrowIfNull(onSuccess);
		ArgumentExceptionExtensions.ThrowIfNull(onError);
		return this._error is not null ? onError(this._error) : onSuccess();
	}

	/// <summary>Switches on this <see cref="Suspicious" />.</summary>
	/// <param name="onSuccess">The handler for the success rail.</param>
	/// <param name="onError">The handler for the failure rail.</param>
	public void Switch(Action onSuccess, Action<Error> onError)
	{
		ArgumentExceptionExtensions.ThrowIfNull(onSuccess);
		ArgumentExceptionExtensions.ThrowIfNull(onError);
		if (this._error is not null) onError(this._error); else onSuccess();
	}

	/// <summary>Maps the <see cref="Error" /> of a failure; a success is returned unchanged.</summary>
	/// <param name="mapper">The mapper.</param>
	/// <returns>A new <see cref="Suspicious" /> with the mapped <see cref="Error" />, or this instance if it is a success.</returns>
	public Suspicious MapError(Func<Error, Error> mapper)
	{
		ArgumentExceptionExtensions.ThrowIfNull(mapper);
		return this._error is not null ? Fail(mapper(this._error)) : this;
	}

	/// <summary>Reinterprets this failed <see cref="Suspicious" /> as a failed <see cref="Suspicious{TValue}" /> (the <see cref="Error" /> is carried over).</summary>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A new failed <see cref="Suspicious{TValue}" /> with the same <see cref="Error" />.</returns>
	/// <remarks>Total on the failure rail only (a success has no value to lift); the guard-style call site is <c>if (result.IsFailure) return result.AsFailure&lt;TValue&gt;();</c>.</remarks>
	/// <exception cref="InvalidOperationException">Thrown if this <see cref="Suspicious" /> is a success (converting a success is a contract violation).</exception>
	public Suspicious<TValue> AsFailure<TValue>() where TValue : notnull
	{
		if (this._error is null) throw new InvalidOperationException($"The {nameof(Suspicious)} is a success; {nameof(this.AsFailure)} requires a failure");
		return Suspicious<TValue>.CreateFailure(this._error);
	}

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
	/// <param name="kind">The kind; its <see cref="OutcomeKind.Side" /> must allow the success side.</param>
	/// <returns>A new successful <see cref="Suspicious" />.</returns>
	public static Suspicious Success(OutcomeKind kind) => new (kind);

	/// <summary>Creates a failed <see cref="Suspicious" /> from an <paramref name="error" />.</summary>
	/// <param name="error">The error.</param>
	/// <returns>A new failed <see cref="Suspicious" /> whose <see cref="Outcome" /> is the <see cref="Monad.Error.Kind" /> of the <paramref name="error" />.</returns>
	public static Suspicious Fail(Error error) => new (error);

	/// <summary>Creates a failed <see cref="Suspicious" /> with an <see cref="OutcomeKind.Invalid" /> <see cref="Error" />.</summary>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <param name="callerMember">The caller member.</param>
	/// <param name="callerFilePath">The caller file path.</param>
	/// <param name="callerLine">The caller line.</param>
	/// <returns>A new failed <see cref="Suspicious" />.</returns>
	public static Suspicious Invalid
	(
		string description,
		Error? cause = null,
		[CallerMemberName] string callerMember = "",
		[CallerFilePath] string callerFilePath = "",
		[CallerLineNumber] int callerLine = 0
	)
	{
		return Fail(Error.Invalid
		(
			description,
			cause,
			callerMember,
			callerFilePath,
			callerLine
		));
	}

	/// <summary>Creates a failed <see cref="Suspicious" /> with an <see cref="OutcomeKind.Conflict" /> <see cref="Error" />.</summary>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <param name="callerMember">The caller member.</param>
	/// <param name="callerFilePath">The caller file path.</param>
	/// <param name="callerLine">The caller line.</param>
	/// <returns>A new failed <see cref="Suspicious" />.</returns>
	public static Suspicious Conflict
	(
		string description,
		Error? cause = null,
		[CallerMemberName] string callerMember = "",
		[CallerFilePath] string callerFilePath = "",
		[CallerLineNumber] int callerLine = 0
	)
	{
		return Fail(Error.Conflict
		(
			description,
			cause,
			callerMember,
			callerFilePath,
			callerLine
		));
	}

	/// <summary>Creates a failed <see cref="Suspicious" /> with an <see cref="OutcomeKind.Failure" /> <see cref="Error" />.</summary>
	/// <param name="description">The description.</param>
	/// <param name="exception">The exception.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <param name="callerMember">The caller member.</param>
	/// <param name="callerFilePath">The caller file path.</param>
	/// <param name="callerLine">The caller line.</param>
	/// <returns>A new failed <see cref="Suspicious" />.</returns>
	public static Suspicious Failure
	(
		string? description = null,
		Exception? exception = null,
		Error? cause = null,
		[CallerMemberName] string callerMember = "",
		[CallerFilePath] string callerFilePath = "",
		[CallerLineNumber] int callerLine = 0
	)
	{
		return Fail(Error.Failure
		(
			description,
			exception,
			cause,
			callerMember,
			callerFilePath,
			callerLine
		));
	}

	/// <summary>Creates a failed <see cref="Suspicious" /> with an <see cref="OutcomeKind.Unavailable" /> <see cref="Error" />.</summary>
	/// <param name="description">The description.</param>
	/// <param name="exception">The exception.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <param name="callerMember">The caller member.</param>
	/// <param name="callerFilePath">The caller file path.</param>
	/// <param name="callerLine">The caller line.</param>
	/// <returns>A new failed <see cref="Suspicious" />.</returns>
	public static Suspicious Unavailable
	(
		string? description = null,
		Exception? exception = null,
		Error? cause = null,
		[CallerMemberName] string callerMember = "",
		[CallerFilePath] string callerFilePath = "",
		[CallerLineNumber] int callerLine = 0
	)
	{
		return Fail(Error.Unavailable
		(
			description,
			exception,
			cause,
			callerMember,
			callerFilePath,
			callerLine
		));
	}

	/// <summary>Creates a failed <see cref="Suspicious" /> with an <see cref="OutcomeKind.Unexpected" /> <see cref="Error" />.</summary>
	/// <param name="exception">The exception.</param>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <param name="callerMember">The caller member.</param>
	/// <param name="callerFilePath">The caller file path.</param>
	/// <param name="callerLine">The caller line.</param>
	/// <returns>A new failed <see cref="Suspicious" />.</returns>
	public static Suspicious Unexpected
	(
		Exception exception,
		string? description = null,
		Error? cause = null,
		[CallerMemberName] string callerMember = "",
		[CallerFilePath] string callerFilePath = "",
		[CallerLineNumber] int callerLine = 0
	)
	{
		return Fail(Error.Unexpected
		(
			exception,
			description,
			cause,
			callerMember,
			callerFilePath,
			callerLine
		));
	}

	/// <summary>Creates a failed <see cref="Suspicious" /> with an <see cref="OutcomeKind.Unexpected" /> <see cref="Error" />.</summary>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <param name="callerMember">The caller member.</param>
	/// <param name="callerFilePath">The caller file path.</param>
	/// <param name="callerLine">The caller line.</param>
	/// <returns>A new failed <see cref="Suspicious" />.</returns>
	public static Suspicious Unexpected
	(
		string? description = null,
		Error? cause = null,
		[CallerMemberName] string callerMember = "",
		[CallerFilePath] string callerFilePath = "",
		[CallerLineNumber] int callerLine = 0
	)
	{
		return Fail(Error.Unexpected
		(
			description,
			cause,
			callerMember,
			callerFilePath,
			callerLine
		));
	}

	#endregion

	#region Creation (Generic)

	/// <summary>Creates an <see cref="OutcomeKind.Ok" /> <see cref="Suspicious{TValue}" /> from a <paramref name="value" />.</summary>
	/// <param name="value">The value.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A new <see cref="OutcomeKind.Ok" /> <see cref="Suspicious{TValue}" />.</returns>
	public static Suspicious<TValue> Ok<TValue>(TValue value) where TValue : notnull => Suspicious<TValue>.CreateSuccess(OutcomeKind.Ok, value);

	/// <summary>Creates a successful miss <see cref="OutcomeKind.NoValue" /> <see cref="Suspicious{TValue}" />.</summary>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>The cached <see cref="OutcomeKind.NoValue" /> <see cref="Suspicious{TValue}" />.</returns>
	/// <remarks>A kind-named factory constructs on the home rail of its kind, and the home rail of <see cref="OutcomeKind.NoValue" /> is success (a plain miss). For a miss the producer treats as a failure, use <c>Fail&lt;TValue&gt;(Error.NoValue(…))</c> — the failure rail is the explicit opt-in.</remarks>
	public static Suspicious<TValue> NoValue<TValue>() where TValue : notnull => Suspicious<TValue>.NoValue;

	/// <summary>Creates a successful <see cref="Suspicious{TValue}" /> with the provided <paramref name="kind" /> and <paramref name="value" />.</summary>
	/// <param name="kind">The kind; its <see cref="OutcomeKind.Side" /> must allow the success side.</param>
	/// <param name="value">The value.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A new successful <see cref="Suspicious{TValue}" />.</returns>
	public static Suspicious<TValue> Success<TValue>(OutcomeKind kind, TValue value) where TValue : notnull => Suspicious<TValue>.CreateSuccess(kind, value);

	/// <summary>Creates a successful <see cref="Suspicious{TValue}" /> with the provided <paramref name="kind" /> and no value.</summary>
	/// <param name="kind">The kind; its <see cref="OutcomeKind.Side" /> must allow the success side.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A new successful <see cref="Suspicious{TValue}" /> without a value.</returns>
	public static Suspicious<TValue> Success<TValue>(OutcomeKind kind) where TValue : notnull => Suspicious<TValue>.CreateSuccess(kind);

	/// <summary>Creates a failed <see cref="Suspicious{TValue}" /> from an <paramref name="error" />.</summary>
	/// <param name="error">The error.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A new failed <see cref="Suspicious{TValue}" /> whose outcome is the <see cref="Monad.Error.Kind" /> of the <paramref name="error" />.</returns>
	public static Suspicious<TValue> Fail<TValue>(Error error) where TValue : notnull => Suspicious<TValue>.CreateFailure(error);

	/// <summary>Creates a failed <see cref="Suspicious{TValue}" /> with an <see cref="OutcomeKind.Invalid" /> <see cref="Error" />.</summary>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <param name="callerMember">The caller member.</param>
	/// <param name="callerFilePath">The caller file path.</param>
	/// <param name="callerLine">The caller line.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A new failed <see cref="Suspicious{TValue}" />.</returns>
	public static Suspicious<TValue> Invalid<TValue>
	(
		string description,
		Error? cause = null,
		[CallerMemberName] string callerMember = "",
		[CallerFilePath] string callerFilePath = "",
		[CallerLineNumber] int callerLine = 0
	) where TValue : notnull
	{
		return Fail<TValue>(Error.Invalid
		(
			description,
			cause,
			callerMember,
			callerFilePath,
			callerLine
		));
	}

	/// <summary>Creates a failed <see cref="Suspicious{TValue}" /> with an <see cref="OutcomeKind.Conflict" /> <see cref="Error" />.</summary>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <param name="callerMember">The caller member.</param>
	/// <param name="callerFilePath">The caller file path.</param>
	/// <param name="callerLine">The caller line.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A new failed <see cref="Suspicious{TValue}" />.</returns>
	public static Suspicious<TValue> Conflict<TValue>
	(
		string description,
		Error? cause = null,
		[CallerMemberName] string callerMember = "",
		[CallerFilePath] string callerFilePath = "",
		[CallerLineNumber] int callerLine = 0
	) where TValue : notnull
	{
		return Fail<TValue>(Error.Conflict
		(
			description,
			cause,
			callerMember,
			callerFilePath,
			callerLine
		));
	}

	/// <summary>Creates a failed <see cref="Suspicious{TValue}" /> with an <see cref="OutcomeKind.Failure" /> <see cref="Error" />.</summary>
	/// <param name="description">The description.</param>
	/// <param name="exception">The exception.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <param name="callerMember">The caller member.</param>
	/// <param name="callerFilePath">The caller file path.</param>
	/// <param name="callerLine">The caller line.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A new failed <see cref="Suspicious{TValue}" />.</returns>
	public static Suspicious<TValue> Failure<TValue>
	(
		string? description = null,
		Exception? exception = null,
		Error? cause = null,
		[CallerMemberName] string callerMember = "",
		[CallerFilePath] string callerFilePath = "",
		[CallerLineNumber] int callerLine = 0
	) where TValue : notnull
	{
		return Fail<TValue>(Error.Failure
		(
			description,
			exception,
			cause,
			callerMember,
			callerFilePath,
			callerLine
		));
	}

	/// <summary>Creates a failed <see cref="Suspicious{TValue}" /> with an <see cref="OutcomeKind.Unavailable" /> <see cref="Error" />.</summary>
	/// <param name="description">The description.</param>
	/// <param name="exception">The exception.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <param name="callerMember">The caller member.</param>
	/// <param name="callerFilePath">The caller file path.</param>
	/// <param name="callerLine">The caller line.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A new failed <see cref="Suspicious{TValue}" />.</returns>
	public static Suspicious<TValue> Unavailable<TValue>
	(
		string? description = null,
		Exception? exception = null,
		Error? cause = null,
		[CallerMemberName] string callerMember = "",
		[CallerFilePath] string callerFilePath = "",
		[CallerLineNumber] int callerLine = 0
	) where TValue : notnull
	{
		return Fail<TValue>(Error.Unavailable
		(
			description,
			exception,
			cause,
			callerMember,
			callerFilePath,
			callerLine
		));
	}

	/// <summary>Creates a failed <see cref="Suspicious{TValue}" /> with an <see cref="OutcomeKind.Unexpected" /> <see cref="Error" />.</summary>
	/// <param name="exception">The exception.</param>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <param name="callerMember">The caller member.</param>
	/// <param name="callerFilePath">The caller file path.</param>
	/// <param name="callerLine">The caller line.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A new failed <see cref="Suspicious{TValue}" />.</returns>
	public static Suspicious<TValue> Unexpected<TValue>
	(
		Exception exception,
		string? description = null,
		Error? cause = null,
		[CallerMemberName] string callerMember = "",
		[CallerFilePath] string callerFilePath = "",
		[CallerLineNumber] int callerLine = 0
	) where TValue : notnull
	{
		return Fail<TValue>(Error.Unexpected
		(
			exception,
			description,
			cause,
			callerMember,
			callerFilePath,
			callerLine
		));
	}

	/// <summary>Creates a failed <see cref="Suspicious{TValue}" /> with an <see cref="OutcomeKind.Unexpected" /> <see cref="Error" />.</summary>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <param name="callerMember">The caller member.</param>
	/// <param name="callerFilePath">The caller file path.</param>
	/// <param name="callerLine">The caller line.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A new failed <see cref="Suspicious{TValue}" />.</returns>
	public static Suspicious<TValue> Unexpected<TValue>
	(
		string? description = null,
		Error? cause = null,
		[CallerMemberName] string callerMember = "",
		[CallerFilePath] string callerFilePath = "",
		[CallerLineNumber] int callerLine = 0
	) where TValue : notnull
	{
		return Fail<TValue>(Error.Unexpected
		(
			description,
			cause,
			callerMember,
			callerFilePath,
			callerLine
		));
	}

	#endregion

	#region Aggregation

	/// <summary>Combines multiple <see cref="Suspicious" /> into one that indicates whether all of them succeeded.</summary>
	/// <param name="results">The results; must be non-empty.</param>
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

		if (isEmpty)
		{
			ArgumentExceptionExtensions.Throw("At least one result is required", nameof(results));
		}

		return errors.Count switch
		{
			0 => Ok(),
			1 => Fail(errors[0]),
			_ => Fail(Error.Aggregate(errors))
		};
	}

	/// <summary>Combines multiple <see cref="Suspicious{TValue}" /> into a unit <see cref="Suspicious" /> that indicates whether all of them succeeded.</summary>
	/// <param name="results">The results; must be non-empty.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns><see cref="Ok()" /> if all results are successes; a failed <see cref="Suspicious" /> carrying the single <see cref="Error" /> or an <see cref="Monad.Error.Aggregate" /> of all of them, otherwise.</returns>
	/// <remarks>Values are discarded — this answers "did they all succeed".</remarks>
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

	#endregion

	#region Conversion

	/// <summary>Implicitly converts an <see cref="Error" /> to a failed <see cref="Suspicious" />.</summary>
	/// <param name="error">The error.</param>
	public static implicit operator Suspicious(Error error) => Fail(error);

	#endregion
}