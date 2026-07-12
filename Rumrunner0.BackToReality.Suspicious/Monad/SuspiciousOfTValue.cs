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
public sealed class Suspicious<TValue> where TValue : notnull
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
	/// <remarks>Safe access paths are <see cref="TryGetValue" />, <see cref="GetValueOr(TValue)" />, <see cref="Match{TResult}(Func{TValue, TResult}, Func{TResult}, Func{Monad.Error, TResult})" /> and <see cref="Switch(Action{TValue}, Action, Action{Monad.Error})" />.</remarks>
	/// <exception cref="InvalidOperationException">Thrown if no value is present — accessing <see cref="Value" /> on a valueless result is a contract violation, not control flow.</exception>
	public TValue Value => this._hasValue ? this._value : throw new InvalidOperationException($"The {nameof(Suspicious<TValue>)} has no value; the outcome is {this._outcome}");

	/// <summary>Error.</summary>
	/// <remarks>Non-<c>null</c> if and only if this <see cref="Suspicious{TValue}" /> is a failure.</remarks>
	public Error? Error => this._error;

	/// <summary>Determines whether the <see cref="Outcome" /> equals the provided <paramref name="kind" />.</summary>
	/// <param name="kind">The kind.</param>
	/// <returns><c>true</c>, if the outcomes are equal; <c>false</c>, otherwise.</returns>
	public bool Is(OutcomeKind kind)
	{
		ArgumentExceptionExtensions.ThrowIfNull(kind);
		return this._outcome == kind;
	}

	/// <summary>Tries to get the value.</summary>
	/// <param name="value">The value, if present.</param>
	/// <returns><c>true</c>, if a value is present; <c>false</c>, otherwise.</returns>
	/// <remarks>The imperative access path — for loops and early returns. At boundaries where every rail must be handled, prefer <see cref="Match{TResult}(Func{TValue, TResult}, Func{TResult}, Func{Monad.Error, TResult})" /> or <see cref="Switch(Action{TValue}, Action, Action{Monad.Error})" />.</remarks>
	public bool TryGetValue([MaybeNullWhen(false)] out TValue value)
	{
		value = this._hasValue ? this._value : default;
		return this._hasValue;
	}

	/// <summary>Gets the value, or the provided <paramref name="fallback" /> if no value is present.</summary>
	/// <param name="fallback">The fallback value.</param>
	/// <returns>The value or the <paramref name="fallback" />.</returns>
	/// <remarks>For flows with a genuine fallback — the <see cref="Error" /> is deliberately discarded. At boundaries where every rail must be handled, prefer <see cref="Match{TResult}(Func{TValue, TResult}, Func{TResult}, Func{Monad.Error, TResult})" /> or <see cref="Switch(Action{TValue}, Action, Action{Monad.Error})" />.</remarks>
	public TValue GetValueOr(TValue fallback)
	{
		if (_valueCanBeNull && fallback is null) throw new ArgumentNullException(nameof(fallback));
		return this._hasValue ? this._value : fallback;
	}

	/// <summary>Gets the value, or the result of the provided <paramref name="fallbackFactory" /> if no value is present.</summary>
	/// <param name="fallbackFactory">The fallback factory.</param>
	/// <returns>The value or the result of the <paramref name="fallbackFactory" />.</returns>
	/// <remarks>For flows with a genuine fallback — the <see cref="Error" /> is deliberately discarded. At boundaries where every rail must be handled, prefer <see cref="Match{TResult}(Func{TValue, TResult}, Func{TResult}, Func{Monad.Error, TResult})" /> or <see cref="Switch(Action{TValue}, Action, Action{Monad.Error})" />.</remarks>
	public TValue GetValueOr(Func<TValue> fallbackFactory)
	{
		ArgumentExceptionExtensions.ThrowIfNull(fallbackFactory);
		return this._hasValue ? this._value : fallbackFactory();
	}

	/// <summary>Matches this <see cref="Suspicious{TValue}" /> into a <typeparamref name="TResult" />.</summary>
	/// <param name="onValue">The handler for a success with a value.</param>
	/// <param name="onError">The handler for the failure rail.</param>
	/// <typeparam name="TResult">The result type.</typeparam>
	/// <returns>The result of the invoked handler.</returns>
	/// <remarks>Use this overload only in flows where a success without a value can't occur; otherwise use the overload with an <c>onNoValue</c> handler.</remarks>
	/// <exception cref="InvalidOperationException">Thrown if this <see cref="Suspicious{TValue}" /> is a success without a value — a contract violation of this overload.</exception>
	public TResult Match<TResult>(Func<TValue, TResult> onValue, Func<Error, TResult> onError)
	{
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		if (this._hasValue) return onValue(this._value);
		if (this._error is not null) return onError(this._error);

		throw new InvalidOperationException($"The {nameof(Suspicious<TValue>)} is a success without a value; use the {nameof(this.Match)} overload with an 'onNoValue' handler");
	}

	/// <summary>Matches this <see cref="Suspicious{TValue}" /> into a <typeparamref name="TResult" />.</summary>
	/// <param name="onValue">The handler for a success with a value.</param>
	/// <param name="onNoValue">The handler for a success without a value.</param>
	/// <param name="onError">The handler for the failure rail.</param>
	/// <typeparam name="TResult">The result type.</typeparam>
	/// <returns>The result of the invoked handler.</returns>
	public TResult Match<TResult>(Func<TValue, TResult> onValue, Func<TResult> onNoValue, Func<Error, TResult> onError)
	{
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onNoValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		if (this._hasValue) return onValue(this._value);
		if (this._error is not null) return onError(this._error);

		return onNoValue();
	}

	/// <summary>Switches on this <see cref="Suspicious{TValue}" />.</summary>
	/// <param name="onValue">The handler for a success with a value.</param>
	/// <param name="onError">The handler for the failure rail.</param>
	/// <remarks>Use this overload only in flows where a success without a value can't occur; otherwise use the overload with an <c>onNoValue</c> handler.</remarks>
	/// <exception cref="InvalidOperationException">Thrown if this <see cref="Suspicious{TValue}" /> is a success without a value — a contract violation of this overload.</exception>
	public void Switch(Action<TValue> onValue, Action<Error> onError)
	{
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		if (this._hasValue) onValue(this._value);
		else if (this._error is not null) onError(this._error);
		else throw new InvalidOperationException($"The {nameof(Suspicious<TValue>)} is a success without a value; use the {nameof(this.Switch)} overload with an 'onNoValue' handler");
	}

	/// <summary>Switches on this <see cref="Suspicious{TValue}" />.</summary>
	/// <param name="onValue">The handler for a success with a value.</param>
	/// <param name="onNoValue">The handler for a success without a value.</param>
	/// <param name="onError">The handler for the failure rail.</param>
	public void Switch(Action<TValue> onValue, Action onNoValue, Action<Error> onError)
	{
		ArgumentExceptionExtensions.ThrowIfNull(onValue);
		ArgumentExceptionExtensions.ThrowIfNull(onNoValue);
		ArgumentExceptionExtensions.ThrowIfNull(onError);

		if (this._hasValue) onValue(this._value);
		else if (this._error is not null) onError(this._error);
		else onNoValue();
	}

	/// <summary>Maps the value into a <typeparamref name="TResult" />; valueless results are propagated unchanged.</summary>
	/// <param name="mapper">The mapper.</param>
	/// <typeparam name="TResult">The result value type.</typeparam>
	/// <returns>A new <see cref="Suspicious{TResult}" />.</returns>
	/// <remarks>A success with a value keeps its <see cref="Outcome" /> (custom success kinds are preserved); a success without a value and a failure short-circuit.</remarks>
	public Suspicious<TResult> Map<TResult>(Func<TValue, TResult> mapper) where TResult : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(mapper);

		if (this._hasValue) return Suspicious<TResult>.CreateSuccess(this._outcome, mapper(this._value));
		if (this._error is not null) return Suspicious<TResult>.CreateFailure(this._error);

		return Suspicious<TResult>.CreateSuccess(this._outcome);
	}

	/// <summary>Chains a <paramref name="binder" /> that itself returns a <see cref="Suspicious{TResult}" />; valueless results short-circuit.</summary>
	/// <param name="binder">The binder.</param>
	/// <typeparam name="TResult">The result value type.</typeparam>
	/// <returns>The result of the <paramref name="binder" />, or a propagated valueless result.</returns>
	/// <remarks>The <paramref name="binder" /> runs ONLY when a value is present; both a success without a value and a failure are propagated unchanged (fail-fast).</remarks>
	public Suspicious<TResult> Then<TResult>(Func<TValue, Suspicious<TResult>> binder) where TResult : notnull
	{
		ArgumentExceptionExtensions.ThrowIfNull(binder);

		if (this._hasValue) return binder(this._value);
		if (this._error is not null) return Suspicious<TResult>.CreateFailure(this._error);

		return Suspicious<TResult>.CreateSuccess(this._outcome);
	}

	/// <summary>Maps the <see cref="Error" /> of a failure; a success is returned unchanged.</summary>
	/// <param name="mapper">The mapper.</param>
	/// <returns>A new <see cref="Suspicious{TValue}" /> with the mapped <see cref="Error" />, or this instance if it is a success.</returns>
	public Suspicious<TValue> MapError(Func<Error, Error> mapper)
	{
		ArgumentExceptionExtensions.ThrowIfNull(mapper);
		return this._error is not null ? CreateFailure(mapper(this._error)) : this;
	}

	/// <summary>Drops the value axis, keeping the <see cref="Outcome" /> and the <see cref="Error" />.</summary>
	/// <returns>A unit <see cref="Suspicious" />.</returns>
	public Suspicious AsUnit()
	{
		return this._error is not null ? Suspicious.Fail(this._error) : Suspicious.Success(this._outcome);
	}

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