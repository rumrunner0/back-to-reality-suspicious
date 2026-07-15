using System;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

public sealed partial class Suspicious
{
	#region Conversion

	/// <summary>Reinterprets this failed <see cref="Suspicious" /> as a failed <see cref="Suspicious{TValue}" /> (the error is carried over).</summary>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <returns>A new failed <see cref="Suspicious{TValue}" /> with the same error.</returns>
	/// <remarks>Total on the failure rail only (a success has no value to lift); the guard-style call site is <c>if (result.IsFailure) return result.AsFailure&lt;TValue&gt;();</c>.</remarks>
	/// <exception cref="InvalidOperationException">Thrown if this <see cref="Suspicious" /> is a success (converting a success is a contract violation).</exception>
	public Suspicious<TValue> AsFailure<TValue>() where TValue : notnull
	{
		if (this._error is null) throw new InvalidOperationException($"The {nameof(Suspicious)} is a success; {nameof(this.AsFailure)} requires a failure");
		return Suspicious<TValue>.CreateFailure(this._error);
	}

	/// <summary>Implicitly converts an <see cref="Error" /> to a failed <see cref="Suspicious" />.</summary>
	/// <param name="error">The error.</param>
	public static implicit operator Suspicious(Error error) => Fail(error);

	#endregion
}