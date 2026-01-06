using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Result monad that wraps either an actual value or a set of errors.</summary>
/// <typeparam name="TValue">The value type.</typeparam>
public sealed class Suspicious<TValue> where TValue : notnull
{
	#region Instance State

	/// <summary>Value.</summary>
	private readonly TValue _value;

	/// <summary>Error set.</summary>
	private readonly ErrorSet? _errorSet;

	/// <summary>State.</summary>
	private readonly SuspiciousState _state;

	/// <inheritdoc cref="Suspicious{TValue}" />
	private Suspicious(TValue value)
	{
		// Consider trade-offs for more comprehensive type-parameter checking.
		// if (_valueIsOfReferenceType && value is null)
		// {
		// 	throw new ArgumentNullException(nameof(value));
		// }
		//
		// if (_valueIsOfNullableValueType && EqualityComparer<TValue>.Default.Equals(value, default!))
		// {
		// 	throw new ArgumentNullException(nameof(value));
		// }

		if (_valueCanBeNull && value is null) throw new ArgumentNullException(nameof(value));

		this._value = value;
		this._errorSet = null;
		this._state = SuspiciousState.Value;
	}

	/// <inheritdoc cref="Suspicious{TValue}" />
	private Suspicious(ErrorSet errorSet)
	{
		ArgumentExceptionExtensions.ThrowIfNull(errorSet);

		this._value = default!;
		this._errorSet = errorSet;
		this._state = SuspiciousState.Error;
	}

	#endregion

	#region Instance API

	/// <summary>State.</summary>
	public SuspiciousState State => this._state;

	/// <summary>Flag that indicates whether this <see cref="Suspicious{TValue}" /> was created from a value.</summary>
	[MemberNotNullWhen(true, nameof(_value))]
	[MemberNotNullWhen(true, nameof(Value))]
	[MemberNotNullWhen(false, nameof(_errorSet))]
	[MemberNotNullWhen(false, nameof(ErrorSet))]
	public bool FromValue => this._state == SuspiciousState.Value;

	/// <summary>Flag that indicates whether this <see cref="Suspicious{TValue}" /> was created from an error.</summary>
	[MemberNotNullWhen(false, nameof(_value))]
	[MemberNotNullWhen(false, nameof(Value))]
	[MemberNotNullWhen(true, nameof(_errorSet))]
	[MemberNotNullWhen(true, nameof(ErrorSet))]
	public bool FromError => this._state == SuspiciousState.Error;

	// TODO: Rework Success to contain different Success values. NoValue is success, not error.
	/// <summary>EXPERIMENTAL! Flag that indicates whether this <see cref="Suspicious{TValue}" /> represents a success.</summary>
	/// <remarks>Will be <c>true</c> if this <see cref="Suspicious{TValue}" /> was created from a value or contains <see cref="Error.NoValue" /> as the most critical error.</remarks>
	public bool Success => this.FromValue || this.FindMostCriticalErrorDeep()?.Kind == ErrorKind.NoValue;

	/// <summary>Value.</summary>
	/// <remarks>Will be <c>default</c> of <typeparamref name="TValue" />, if this <see cref="Suspicious{TValue}" /> wasn't created from a value.</remarks>
	public TValue Value => this.FromValue ? this._value : default!;

	/// <summary>Error set.</summary>
	/// <remarks>Will be <c>null</c>, if this <see cref="Suspicious{TValue}" /> wasn't created from an error.</remarks>
	public ErrorSet ErrorSet => this.FromError ? this._errorSet : null!;

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
	public Suspicious<TValue> SetCause<TOtherValue>(Suspicious<TOtherValue> other) where TOtherValue : notnull
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

	/// <summary>Determines whether an <see cref="Error" /> with the provided <paramref name="kind" /> exists among errors only in the current <see cref="ErrorSet" />.</summary>
	/// <param name="kind">The kind.</param>
	/// <returns><c>true</c>, if an <see cref="Error" /> exists; <c>false</c>, otherwise.</returns>
	public bool ContainsError(ErrorKind kind) => this.FindError(kind) is not null;

	/// <summary>Determines whether an <see cref="Error" /> with the provided <paramref name="kind" /> exists among all errors in the cause chain, including self.</summary>
	/// <param name="kind">The kind.</param>
	/// <returns><c>true</c>, if an <see cref="Error" /> exists; <c>false</c>, otherwise.</returns>
	public bool ContainsErrorDeep(ErrorKind kind) => this.FindErrorDeep(kind) is not null;

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
		if (this.FromValue) builder.Append(this._value.ToString());
		else if (this.FromError) builder.Append(this._errorSet.ToString());

		return true;
	}

	/// <summary>Prints members in redacted mode.</summary>
	/// <param name="builder">The <see cref="StringBuilder" />.</param>
	/// <returns><c>true</c> if members should be printed; <c>false</c> otherwise.</returns>
	private bool PrintMembersRedacted(StringBuilder builder)
	{
		if (this.FromValue) builder.Append(this._value.ToString());
		else if (this.FromError) builder.Append(this._errorSet.ToStringRedacted());

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

	#region Static State

	/// <summary>Flag that indicates whether <typeparamref name="TValue" /> can be <c>null</c>.</summary>
	private static readonly bool _valueCanBeNull = default(TValue) is null;

	// Consider trade-offs for more comprehensive type-parameter checking.
	// /// <summary>Flag that indicates whether <typeparamref name="TValue" /> is a reference type.</summary>
	// private static readonly bool _valueIsOfReferenceType = !typeof(TValue).IsValueType;
	//
	// /// <summary>Flag that indicates whether <typeparamref name="TValue" /> is a <see cref="Nullable{T}" /> value type.</summary>
	// private static readonly bool _valueIsOfNullableValueType = Nullable.GetUnderlyingType(typeof(TValue)) is not null;

	#endregion

	#region Static API

	/// <summary>Creates a <see cref="Suspicious{TValue}" /> from a <paramref name="value" />.</summary>
	/// <param name="value">The <paramref name="value" />.</param>
	/// <returns>A new <see cref="Suspicious{TValue}" />.</returns>
	internal static Suspicious<TValue> From(TValue value) => new (value);

	/// <summary>Creates a <see cref="Suspicious{TValue}" /> from an <paramref name="errorSet" />.</summary>
	/// <param name="errorSet">The <see cref="ErrorSet"/>.</param>
	/// <returns>A new <see cref="Suspicious{TValue}" />.</returns>
	internal static Suspicious<TValue> From(ErrorSet errorSet) => new (errorSet);

	/// <summary>Implicitly converts a <typeparamref name="TValue" /> to a <see cref="string" />.</summary>
	/// <param name="source">The <typeparamref name="TValue" />.</param>
	public static implicit operator Suspicious<TValue>(TValue source) => From(source);

	/// <summary>EXPERIMENTAL! May break switch-case. Implicitly converts a <see cref="Suspicious{TValue}" /> to a <see cref="bool" /> indicating that this <see cref="Suspicious{TValue}" /> was created from a value.</summary>
	/// <param name="source">The source.</param>
	/// <remarks><c>true</c>, only if <see cref="FromValue" /> is <c>true</c>; <c>false</c>, otherwise. Simply, this is a shortcut for <see cref="FromValue" />.</remarks>
	public static implicit operator bool(Suspicious<TValue> source) => source.FromValue;

	#endregion
}