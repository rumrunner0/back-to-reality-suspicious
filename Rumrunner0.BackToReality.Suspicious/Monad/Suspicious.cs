using System;
using System.Diagnostics;
using System.Text;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Result monad that wraps either an actual value or a set of errors.</summary>
/// <typeparam name="TValue">The value type.</typeparam>
public sealed record class Suspicious<TValue>
{
	#region Instance State

	/// <summary>Value.</summary>
	private readonly TValue? _value;

	/// <summary>Error set.</summary>
	private readonly ErrorSet? _errorSet;

	/// <inheritdoc cref="Suspicious{TValue}" />
	private Suspicious(TValue value)
	{
		ArgumentExceptionExtensions.ThrowIfNull(value);
		this._value = value;
	}

	/// <inheritdoc cref="Suspicious{TValue}" />
	private Suspicious(ErrorSet errorSet)
	{
		ArgumentExceptionExtensions.ThrowIfNull(errorSet);
		this._errorSet = errorSet;
	}

	#endregion

	#region Instance API

	/// <summary>Value.</summary>
	/// <remarks>Will be <c>null</c> or <c>default</c>, if this <see cref="Suspicious{TValue}" /> wasn't created from a value.</remarks>
	public TValue Value => this._value!;

	/// <summary>Error set.</summary>
	/// <remarks>Will be <c>null</c>, if this <see cref="Suspicious{TValue}" /> wasn't created from an error.</remarks>
	public ErrorSet ErrorSet => this._errorSet!;

	/// <summary>Flag that indicates whether this <see cref="Suspicious{TValue}" /> was created from a value.</summary>
	public bool FromValue => this.State == SuspiciousState.Value;

	/// <summary>Flag that indicates whether this <see cref="Suspicious{TValue}" /> was created from an error.</summary>
	public bool FromError => this.State == SuspiciousState.Error;

	/// <summary>State.</summary>
	public SuspiciousState State
	{
		get
		{
			if (this._value is not null) return SuspiciousState.Value;
			if (this._errorSet is not null) return SuspiciousState.Error;
			return SuspiciousState.Unexpected;
		}
	}

	/// <summary>Adds an <see cref="Error" /> to the <see cref="ErrorSet" />.</summary>
	/// <param name="error">The error.</param>
	/// <returns>This <see cref="Suspicious{TValue}" />.</returns>
	/// <remarks>To use this method, this <see cref="Suspicious{TValue}" /> must have been created from an error.</remarks>
	/// <exception cref="InvalidOperationException">Thrown if this <see cref="Suspicious{TValue}" /> wasn't created from an error.</exception>
	public Suspicious<TValue> AddError(Error error)
	{
		this.EnsureCreatedFromError();
		this._errorSet!.TryAddError(error);
		return this;
	}

	/// <summary>
	/// Sets the <see cref="ErrorSet" /> of an <paramref name="other" /> <see cref="Suspicious{TValue}" />
	/// as the inner <see cref="ErrorSet" /> of this, indicating that this result was caused by <paramref name="other" />.
	/// </summary>
	/// <param name="other">The <see cref="Suspicious{TValue}" /> whose <see cref="ErrorSet" /> will be used as the inner.</param>
	/// <typeparam name="TOtherValue">The type of the <paramref name="other" /> <see cref="Suspicious{TValue}" />.</typeparam>
	/// <returns>This <see cref="Suspicious{TValue}" />.</returns>
	/// <exception cref="InvalidOperationException">Thrown if either this instance or <paramref name="other" /> was not created from an error.</exception>
	public Suspicious<TValue> SetCause<TOtherValue>(Suspicious<TOtherValue> other)
	{
		this.EnsureCreatedFromError();
		other.EnsureCreatedFromError();
		this._errorSet!.SetCause(other._errorSet!);
		return this;
	}

	/// <summary>Searches for the first <see cref="Error" /> with the provided <paramref name="kind" /> among errors only in the current <see cref="ErrorSet" />.</summary>
	/// <param name="kind">The kind.</param>
	/// <returns>An <see cref="Error" /> or <c>null</c>.</returns>
	public Error? FindError(ErrorKind kind)
	{
		this.EnsureCreatedFromError();
		return this._errorSet!.FindError(kind);
	}

	/// <summary>Searches for the first <see cref="Error" /> with the provided <paramref name="kind" /> among all errors in the cause chain, including self.</summary>
	/// <param name="kind">The kind.</param>
	/// <returns>An <see cref="Error" /> or <c>null</c>.</returns>
	public Error? FindErrorDeep(ErrorKind kind)
	{
		this.EnsureCreatedFromError();
		return this._errorSet!.FindErrorDeep(kind);
	}

	/// <summary>Searches for the most critical <see cref="Error" /> among all errors in the cause chain, including self.</summary>
	/// <returns>An <see cref="Error" /> or <c>null</c>.</returns>
	/// <exception cref="InvalidOperationException">Thrown if this <see cref="Suspicious{TValue}" /> doesn't contain any errors.</exception>
	public Error? FindMostCriticalErrorDeep()
	{
		this.EnsureCreatedFromError();
		return this._errorSet!.FindMostCriticalErrorDeep();
	}

	#endregion

	#region Instance Utilities

	/// <summary>Ensures that this <see cref="Suspicious{TValue}" /> instance was created from an error and has a valid <see cref="ErrorSet" />.</summary>
	/// <exception cref="InvalidOperationException">Thrown if this <see cref="Suspicious{TValue}" /> was not created from an error.</exception>
	/// <exception cref="UnreachableException">Thrown if the internal <see cref="ErrorSet" /> is <c>null</c> despite <see cref="FromError" /> being <c>true</c>.</exception>
	private void EnsureCreatedFromError()
	{
		if (!this.FromError) throw new InvalidOperationException($"The {nameof(Suspicious<TValue>)} wasn't created from an error");
		if (this._errorSet is null) throw new UnreachableException($"The error set is null but '.{nameof(this.FromError)}' is {this.FromError}");
	}

	/// <summary>Ensures that this <see cref="Suspicious{TValue}" /> instance was created from an error and contains actual <see cref="Error" />s in its <see cref="ErrorSet" />.</summary>
	/// <exception cref="InvalidOperationException">Thrown if this <see cref="Suspicious{TValue}" /> was not created from an error, or if its <see cref="ErrorSet" /> doesn't contain any errors.</exception>
	private void EnsureContainErrors()
	{
		this.EnsureCreatedFromError();
		if (!this._errorSet!.ContainsErrors) throw new InvalidOperationException("The error set doesn't contain any errors");
	}

	/// <summary>Prints members.</summary>
	/// <param name="builder">The <see cref="StringBuilder" />.</param>
	/// <returns><c>true</c> if members should be printed; <c>false</c> otherwise.</returns>
	private bool PrintMembers(StringBuilder builder)
	{
		if (this._value is not null)
		{
			builder.Append(this._value.ToString());
		}
		else if (this._errorSet is not null)
		{
			builder.Append(this._errorSet.ToString());
		}

		return true;
	}

	/// <summary>Prints members in redacted mode.</summary>
	/// <param name="builder">The <see cref="StringBuilder" />.</param>
	/// <returns><c>true</c> if members should be printed; <c>false</c> otherwise.</returns>
	private bool PrintMembersRedacted(StringBuilder builder)
	{
		if (this._value is not null)
		{
			builder.Append(this._value.ToString());
		}
		else if (this._errorSet is not null)
		{
			builder.Append(this._errorSet.ToStringRedacted());
		}

		return true;
	}

	/// <summary>Creates a string that represents this instance.</summary>
	/// <returns>A string that represents this instance.</returns>
	public override string ToString()
	{
		var builder = new StringBuilder();

		builder.Append($"{nameof(Suspicious<TValue>)} {{ ");
		if (this.PrintMembers(builder)) builder.Append(' ');
		builder.Append('}');

		return builder.ToString();
	}

	/// <summary>Creates a string that represents this instance in redacted mode.</summary>
	/// <returns>A string that represents this instance in redacted mode.</returns>
	public string ToStringRedacted()
	{
		var builder = new StringBuilder();

		this.PrintMembersRedacted(builder);

		return builder.ToString();
	}

	#endregion

	#region Static API

	/// <inheritdoc cref="From(TValue)" />
	public static implicit operator Suspicious<TValue>(TValue value) => From(value);

	/// <summary>Creates a <see cref="Suspicious{TValue}" /> from a <paramref name="value" />.</summary>
	/// <param name="value">The <paramref name="value" />.</param>
	/// <returns>A new <see cref="Suspicious{TValue}" />.</returns>
	internal static Suspicious<TValue> From(TValue value) => new (value);

	/// <summary>Creates a <see cref="Suspicious{TValue}" /> from an <paramref name="errorSet" />.</summary>
	/// <param name="errorSet">The <see cref="ErrorSet"/>.</param>
	/// <returns>A new <see cref="Suspicious{TValue}" />.</returns>
	internal static Suspicious<TValue> From(ErrorSet errorSet) => new (errorSet);

	#endregion
}