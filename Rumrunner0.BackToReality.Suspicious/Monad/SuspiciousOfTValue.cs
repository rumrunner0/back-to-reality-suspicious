using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Serialization;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;
using Rumrunner0.BackToReality.Suspicious.Serialization;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Result monad that represents the outcome of an operation and, optionally, its value.</summary>
/// <typeparam name="TValue">The value type.</typeparam>
/// <remarks>
/// <para>* Success is a per-instance fact which means an instance is a success iff no <see cref="Error" /> is attached.</para>
/// <para>* A success may carry a value (<see cref="OutcomeKind.Ok" /> or a custom success kind) or not (<see cref="OutcomeKind.NoValue" /> or a custom success kind).</para>
/// <para>* Create instances via the factories on the non-generic <see cref="Suspicious" />.</para>
/// </remarks>
[JsonConverter(typeof(SuspiciousJsonConverterFactory))]
public sealed partial class Suspicious<TValue> where TValue : notnull
{
	#region Instance State

	/// <summary>Outcome.</summary>
	private readonly OutcomeKind _outcome;

	/// <summary>Value.</summary>
	private readonly TValue _value;

	/// <summary>Flag that indicates whether a value is present.</summary>
	private readonly bool _hasValue;

	/// <summary>Error.</summary>
	private readonly Error? _error;

	/// <inheritdoc cref="Suspicious{TValue}" />
	private Suspicious(OutcomeKind outcome, TValue value)
	{
		ArgumentExceptionExtensions.ThrowIfNull(outcome);
		if (!outcome.Side.AllowsSuccess) ArgumentExceptionExtensions.Throw($"The kind {outcome} doesn't allow the success side", nameof(outcome));
		if (_valueCanBeNull && value is null) throw new ArgumentNullException(nameof(value));

		this._outcome = outcome;
		this._value = value;
		this._hasValue = true;
		this._error = null;
	}

	/// <inheritdoc cref="Suspicious{TValue}" />
	private Suspicious(OutcomeKind outcome)
	{
		ArgumentExceptionExtensions.ThrowIfNull(outcome);
		if (!outcome.Side.AllowsSuccess) ArgumentExceptionExtensions.Throw($"The kind {outcome} doesn't allow the success side", nameof(outcome));

		this._outcome = outcome;
		this._value = default!;
		this._hasValue = false;
		this._error = null;
	}

	/// <inheritdoc cref="Suspicious{TValue}" />
	private Suspicious(Error error)
	{
		ArgumentExceptionExtensions.ThrowIfNull(error);

		this._outcome = error.Kind;
		this._value = default!;
		this._hasValue = false;
		this._error = error;
	}

	#endregion

	#region Common API

	/// <summary>Outcome.</summary>
	public OutcomeKind Outcome => this._outcome;

	/// <summary>Flag that indicates whether this <see cref="Suspicious{TValue}" /> is a success (no <see cref="Error" /> is attached).</summary>
	[MemberNotNullWhen(false, nameof(_error))]
	[MemberNotNullWhen(false, nameof(Error))]
	public bool IsSuccess => this._error is null;

	/// <summary>Flag that indicates whether this <see cref="Suspicious{TValue}" /> is a failure (an <see cref="Error" /> is attached).</summary>
	[MemberNotNullWhen(true, nameof(_error))]
	[MemberNotNullWhen(true, nameof(Error))]
	public bool IsFailure => this._error is not null;

	/// <summary>Flag that indicates whether a value is present.</summary>
	/// <remarks>A present value implies a success; the reverse doesn't hold (see <see cref="OutcomeKind.NoValue" />).</remarks>
	public bool HasValue => this._hasValue;

	/// <summary>Value.</summary>
	/// <remarks>Safe access paths are <c>TryGetValue</c>, <c>GetValueOr</c> and the three-way <c>Match</c>/<c>Switch</c> extensions.</remarks>
	/// <exception cref="InvalidOperationException">Thrown if no value is present — accessing <see cref="Value" /> on a valueless result is a contract violation, not control flow.</exception>
	public TValue Value => this._hasValue ? this._value : throw new InvalidOperationException($"The {nameof(Suspicious<TValue>)} has no value; the outcome is {this._outcome}");

	/// <summary>Error.</summary>
	/// <remarks>Non-<c>null</c> if and only if this <see cref="Suspicious{TValue}" /> is a failure.</remarks>
	public Error? Error => this._error;

	#endregion

	#region Display

	/// <summary>Prints members.</summary>
	/// <param name="builder">The <see cref="StringBuilder" />.</param>
	/// <returns><c>true</c> if members should be printed; <c>false</c> otherwise.</returns>
	private bool PrintMembers(StringBuilder builder)
	{
		builder.Append($"Outcome = {this._outcome}");

		if (this._hasValue) builder.Append($", Value = {this._value}");
		if (this._error is not null) builder.Append($", Error = {this._error}");

		return true;
	}

	/// <inheritdoc />
	public override string ToString()
	{
		var builder = new StringBuilder();

		builder.Append($"{nameof(Suspicious<TValue>)} {{ ");
		if (this.PrintMembers(builder)) builder.Append(' ');
		builder.Append('}');

		return builder.ToString();
	}

	#endregion

	#region Creation

	/// <summary>Flag that indicates whether <typeparamref name="TValue" /> can be <c>null</c>.</summary>
	private static readonly bool _valueCanBeNull = default(TValue) is null;

	/// <summary>Cached <see cref="OutcomeKind.NoValue" /> <see cref="Suspicious{TValue}" />.</summary>
	internal static Suspicious<TValue> NoValue { get; } = new (OutcomeKind.NoValue);

	/// <summary>Creates a successful <see cref="Suspicious{TValue}" /> with the provided <paramref name="kind" /> and <paramref name="value" />.</summary>
	/// <param name="kind">The kind.</param>
	/// <param name="value">The value.</param>
	/// <returns>A new successful <see cref="Suspicious{TValue}" />.</returns>
	internal static Suspicious<TValue> CreateSuccess(OutcomeKind kind, TValue value) => new (kind, value);

	/// <summary>Creates a successful <see cref="Suspicious{TValue}" /> with the provided <paramref name="kind" /> and no value.</summary>
	/// <param name="kind">The kind.</param>
	/// <returns>A new successful <see cref="Suspicious{TValue}" /> without a value.</returns>
	internal static Suspicious<TValue> CreateSuccess(OutcomeKind kind) => new (kind);

	/// <summary>Creates a failed <see cref="Suspicious{TValue}" /> from an <paramref name="error" />.</summary>
	/// <param name="error">The error.</param>
	/// <returns>A new failed <see cref="Suspicious{TValue}" />.</returns>
	internal static Suspicious<TValue> CreateFailure(Error error) => new (error);

	/// <summary>Implicitly converts a <typeparamref name="TValue" /> to an <see cref="OutcomeKind.Ok" /> <see cref="Suspicious{TValue}" />.</summary>
	/// <param name="value">The value.</param>
	public static implicit operator Suspicious<TValue>(TValue value) => CreateSuccess(OutcomeKind.Ok, value);

	/// <summary>Implicitly converts an <see cref="Error" /> to a failed <see cref="Suspicious{TValue}" />.</summary>
	/// <param name="error">The error.</param>
	public static implicit operator Suspicious<TValue>(Error error) => CreateFailure(error);

	#endregion
}