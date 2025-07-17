using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Result monad that wraps either an actual result or a collection of errors.</summary>
/// <typeparam name="TResult">The result type.</typeparam>
public sealed record class Suspicious<TResult>
{
	#region Instance State

	/// <summary>Result.</summary>
	private readonly TResult? _result;

	/// <summary>Error collection.</summary>
	private readonly ErrorCollection? _errorCollection;

	/// <inheritdoc cref="Suspicious{TResult}" />
	private Suspicious(TResult result)
	{
		ArgumentExceptionExtensions.ThrowIfNull(result);
		this._result = result;
	}

	/// <inheritdoc cref="Suspicious{TResult}" />
	private Suspicious(ErrorCollection errorCollection)
	{
		ArgumentExceptionExtensions.ThrowIfNull(errorCollection);
		this._errorCollection = errorCollection;
	}

	#endregion

	#region Instance API

	// TODO: This is not good for performance.
	// I'll need to maintain the state in a static way.
	/// <summary>State.</summary>
	public SuspiciousState State => (Result: this._result, ErrorCollection: this._errorCollection) switch
	{
		{ Result: { } } => SuspiciousState.Result,
		{ ErrorCollection.HasErrors: true } => SuspiciousState.Error,
		{ ErrorCollection: { } } => SuspiciousState.EmptyErrorCollection,
		_ => SuspiciousState.Unexpected
	};

	/// <summary>Flag that indicates whether the result exists.</summary>
	public bool HasResult => this.State == SuspiciousState.Result;

	/// <summary>Flag that indicates whether any errors exist.</summary>
	public bool HasErrors => this.State == SuspiciousState.Error;

	/// <summary>Flag that indicates whether this <see cref="Suspicious{TResult}" /> was created from an error.</summary>
	public bool FromError => SuspiciousState.ErrorStates.Contains(this.State);

	/// <summary>Result.</summary>
	public TResult Result => this._result ?? throw new InvalidOperationException($"The result doesn't exist. Use '.{nameof(HasResult)}' to ensure the result exists");

	/// <summary>Error collection.</summary>
	public ErrorCollection ErrorCollection => this._errorCollection ?? throw new InvalidOperationException($"The error collection doesn't exist. Use '.{nameof(FromError)}' to ensure the error collection exists, or '.{nameof(HasErrors)}' to ensure actual errors exist");

	/// <summary>Adds an <see cref="Error" /> to the <see cref="ErrorCollection" />.</summary>
	/// <param name="error">The error.</param>
	/// <returns>This <see cref="Suspicious{TResult}" />.</returns>
	/// <remarks>To use this method, this <see cref="Suspicious{TResult}" /> must have been created from an error.</remarks>
	/// <exception cref="InvalidOperationException">If this <see cref="Suspicious{TResult}" /> wasn't created from an error.</exception>
	public Suspicious<TResult> AddError(Error error)
	{
		EnsureCreatedFromError();
		this._errorCollection!.AddError(error);
		return this;
	}

	/// <summary>Adds multiple <see cref="Error" />s to the <see cref="ErrorCollection" />.</summary>
	/// <param name="errors">The errors.</param>
	/// <returns>This <see cref="Suspicious{TResult}" />.</returns>
	/// <remarks>To use this method, this <see cref="Suspicious{TResult}" /> must have been created from an error.</remarks>
	/// <exception cref="InvalidOperationException">If this <see cref="Suspicious{TResult}" /> wasn't created from an error.</exception>
	public Suspicious<TResult> AddErrors(IEnumerable<Error> errors)
	{
		EnsureCreatedFromError();
		this._errorCollection!.AddErrors(errors);
		return this;
	}

	/// <summary>Sets an inner <see cref="ErrorCollection" /> that caused this one.</summary>
	/// <param name="collection">The inner <see cref="ErrorCollection" />.</param>
	/// <returns>This <see cref="Suspicious{TResult}" />.</returns>
	/// <remarks>To use this method, this <see cref="Suspicious{TResult}" /> must have been created from an error.</remarks>
	/// <exception cref="InvalidOperationException">If this <see cref="Suspicious{TResult}" /> wasn't created from an error.</exception>
	public Suspicious<TResult> SetErrorCause(ErrorCollection collection)
	{
		EnsureCreatedFromError();
		this._errorCollection!.SetCause(collection);
		return this;
	}

	/// <summary>
	/// Sets the <see cref="ErrorCollection" /> of an <paramref name="other" /> <see cref="Suspicious{TResult}" />
	/// as the inner <see cref="ErrorCollection" /> of this, indicating that this result was caused by <paramref name="other" />.
	/// </summary>
	/// <param name="other">The <see cref="Suspicious{TResult}" /> whose <see cref="ErrorCollection" /> will be used as the inner.</param>
	/// <typeparam name="TOtherResult">The type of the <paramref name="other" /> <see cref="Suspicious{TResult}" />.</typeparam>
	/// <returns>This <see cref="Suspicious{TResult}" />.</returns>
	/// <exception cref="InvalidOperationException">Thrown if either this instance or <paramref name="other" /> was not created from an error.</exception>
	public Suspicious<TResult> SetErrorCauseFrom<TOtherResult>(Suspicious<TOtherResult> other)
	{
		EnsureCreatedFromError();
		other.EnsureCreatedFromError();
		this._errorCollection!.SetCause(other._errorCollection!);
		return this;
	}

	// TODO: Maybe we'll need a Try version of this method.
	// TODO: Add options to configure throw on error or not (if created from result or anything else).
	/// <summary>Finds the most critical <see cref="ErrorKind" /> based on priority.</summary>
	/// <returns>An <see cref="ErrorKind" /> with the highest priority.</returns>
	/// <exception cref="InvalidOperationException">If this <see cref="Suspicious{TResult}" /> doesn't contain any errors.</exception>
	public ErrorKind FindTheMostCriticalErrorKind()
	{
		EnsureHasErrors();
		return ErrorKind.FindWithHighestPriority(this._errorCollection!.AllErrors.Select(e => e.Kind));
	}

	#endregion

	#region Instance Utilities

	/// <summary>Ensures that this <see cref="Suspicious{TResult}" /> instance was created from an error and has a valid <see cref="ErrorCollection" />.</summary>
	/// <exception cref="InvalidOperationException">Thrown if this <see cref="Suspicious{TResult}" /> was not created from an error.</exception>
	/// <exception cref="UnreachableException">Thrown if the internal <see cref="ErrorCollection" /> is <c>null</c> despite <see cref="FromError" /> being <c>true</c>.</exception>
	private void EnsureCreatedFromError()
	{
		if (!this.FromError) throw new InvalidOperationException($"The {nameof(Suspicious<TResult>)} wasn't created from an error");
		if (this._errorCollection is null) throw new UnreachableException($"The error collection is null but '.{nameof(this.FromError)}' is {this.FromError}");
	}

	/// <summary>Ensures that this <see cref="Suspicious{TResult}" /> instance was created from an error and has actual <see cref="Error" />s in its <see cref="ErrorCollection" />.</summary>
	/// <exception cref="InvalidOperationException">Thrown if this <see cref="Suspicious{TResult}" /> was not created from an error, or if its <see cref="ErrorCollection" /> doesn't contain any errors.</exception>
	/// <exception cref="UnreachableException">Thrown if the internal <see cref="ErrorCollection" /> is <c>null</c> despite <see cref="HasErrors" /> being <c>true</c>.</exception>
	private void EnsureHasErrors()
	{
		if (!this.FromError) throw new InvalidOperationException($"The {nameof(Suspicious<TResult>)} wasn't created from an error");
		if (!this.HasErrors) throw new InvalidOperationException("The error collection doesn't contain any errors");
		if (this._errorCollection is null) throw new UnreachableException($"The error collection is null but '.{nameof(this.HasErrors)}' is {this.HasErrors}");
	}

	/// <summary>Prints members.</summary>
	/// <param name="builder">The <see cref="StringBuilder" />.</param>
	/// <returns><c>true</c> if members should be printed; <c>false</c> otherwise.</returns>
	private bool PrintMembers(StringBuilder builder)
	{
		if (this._result is not null)
		{
			builder.Append(this._result.ToString());
		}
		else if (this._errorCollection is not null)
		{
			builder.Append(this._errorCollection.ToString());
		}

		return true;
	}

	/// <summary>Prints members in redacted mode.</summary>
	/// <param name="builder">The <see cref="StringBuilder" />.</param>
	/// <returns><c>true</c> if members should be printed; <c>false</c> otherwise.</returns>
	private bool PrintMembersRedacted(StringBuilder builder)
	{
		if (this._result is not null)
		{
			builder.Append(this._result.ToString());
		}
		else if (this._errorCollection is not null)
		{
			builder.Append(this._errorCollection.ToStringRedacted());
		}

		return true;
	}

	/// <summary>Creates a string that represents this instance.</summary>
	/// <returns>A string that represents this instance.</returns>
	public override string ToString()
	{
		var builder = new StringBuilder();

		builder.Append($"{nameof(Suspicious<TResult>)} {{ ");
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

	/// <inheritdoc cref="From(TResult)" />
	public static implicit operator Suspicious<TResult>(TResult result) => Suspicious<TResult>.From(result);

	/// <inheritdoc cref="Result" />
	public static implicit operator TResult(Suspicious<TResult> suspicious) => suspicious.Result;

	/// <summary>Creates a <see cref="Suspicious{TResult}" /> from a <paramref name="result" />.</summary>
	/// <param name="result">The <paramref name="result" />.</param>
	/// <returns>A new <see cref="Suspicious{TResult}" />.</returns>
	internal static Suspicious<TResult> From(TResult result) => new (result);

	/// <summary>Creates a <see cref="Suspicious{TResult}" /> from an <paramref name="errorCollection" />.</summary>
	/// <param name="errorCollection">The <see cref="ErrorCollection"/>.</param>
	/// <returns>A new <see cref="Suspicious{TResult}" />.</returns>
	internal static Suspicious<TResult> From(ErrorCollection errorCollection) => new (errorCollection);

	#endregion
}