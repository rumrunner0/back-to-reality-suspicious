using System;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

public sealed partial class Suspicious<TValue> where TValue : notnull
{
	#region Conversion

	/// <summary>Drops the value axis, keeping the outcome and the error.</summary>
	/// <returns>A unit <see cref="Suspicious" />.</returns>
	public Suspicious AsUnit()
	{
		return this._error is { } error ? Suspicious.Fail(error) : Suspicious.Success(this._outcome);
	}

	/// <summary>Reinterprets this failed <see cref="Suspicious{TValue}" /> as a failed <see cref="Suspicious{TResult}" /> (the <see cref="Error" /> is carried over).</summary>
	/// <typeparam name="TResult">The result value type.</typeparam>
	/// <returns>A new failed <see cref="Suspicious{TResult}" /> with the same <see cref="Error" />.</returns>
	/// <remarks>
	/// <para>* Total on the failure rail only (a success has no value to lift); the guard-style call site is <c>if (result.IsFailure) return result.AsFailure&lt;TResult&gt;();</c>.</para>
	/// <para>* Takes exactly ONE explicit type argument by design: an extension form would force two (<c>TResult</c> is return-only and C# has no partial type-argument inference) — that is why the conversion family lives on the types.</para>
	/// </remarks>
	/// <exception cref="InvalidOperationException">Thrown if this <see cref="Suspicious{TValue}" /> is a success (converting a success is a contract violation).</exception>
	public Suspicious<TResult> AsFailure<TResult>() where TResult : notnull
	{
		if (this._error is null) throw new InvalidOperationException($"The {nameof(Suspicious<TValue>)} is a success; {nameof(this.AsFailure)} requires a failure");
		return Suspicious<TResult>.CreateFailure(this._error);
	}

	/// <summary>Implicitly converts a <typeparamref name="TValue" /> to an <see cref="OutcomeKind.Ok" /> <see cref="Suspicious{TValue}" />.</summary>
	/// <param name="value">The value.</param>
	public static implicit operator Suspicious<TValue>(TValue value) => CreateSuccess(OutcomeKind.Ok, value);

	/// <summary>Implicitly converts an <see cref="Error" /> to a failed <see cref="Suspicious{TValue}" />.</summary>
	/// <param name="error">The error.</param>
	public static implicit operator Suspicious<TValue>(Error error) => CreateFailure(error);

	#endregion
}