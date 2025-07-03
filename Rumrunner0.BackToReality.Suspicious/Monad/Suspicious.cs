using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Rumrunner0.BackToReality.Suspicious.Extensions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>
/// Result monad that wraps either an actual result or a set of errors.
/// </summary>
/// <typeparam name="TResult">The result type.</typeparam>
public sealed record class Suspicious<TResult>
{
	#region Instance State

	/// <summary>Result.</summary>
	private readonly TResult? _result;

	/// <summary>Error set.</summary>
	private readonly ErrorSet? _errorSet;

	/// <inheritdoc cref="Suspicious{TResult}" />
	private Suspicious(TResult result)
	{
		ArgumentNullExceptionHelper.ThrowIfNull(result);
		this._result = result;
	}

	/// <inheritdoc cref="Suspicious{TResult}" />
	private Suspicious(ErrorSet errorSet)
	{
		ArgumentNullExceptionHelper.ThrowIfNull(errorSet);
		this._errorSet = errorSet;
	}

	#endregion

	#region Instance API

	/// <summary>State.</summary>
	public SuspiciousState State => (Result: this._result, ErrorSet: this._errorSet) switch
	{
		{ Result: { } } => SuspiciousState.Result,
		{ ErrorSet.HasErrors: true } => SuspiciousState.Error,
		{ ErrorSet: { } } => SuspiciousState.EmptyErrorSet,
		_ => SuspiciousState.Unexpected
	};

	/// <summary>Flag that indicates whether the result exists.</summary>
	public bool HasResult => this.State == SuspiciousState.Result;

	/// <summary>Flag that indicates whether any errors exist.</summary>
	public bool HasErrors => this.State == SuspiciousState.Error;

	/// <summary>Flag that indicates whether this <see cref="Suspicious{TResult}" /> was created from an error.</summary>
	public bool FromError => SuspiciousState.ErrorStates.Contains(this.State);

	/// <inheritdoc cref="_result" />
	public TResult Result => this._result ?? throw new InvalidOperationException($"The result doesn't exist. Use '.{nameof(HasResult)}' to ensure the result exists");

	/// <inheritdoc cref="_errorSet" />
	public ErrorSet ErrorSet => this._errorSet ?? throw new InvalidOperationException($"The error set doesn't exist. Use '.{nameof(FromError)}' to ensure the error set exists, or '.{nameof(HasErrors)}' to ensure actual errors exist");

	/// <summary>Adds an error to the error set.</summary>
	/// <param name="error">The error.</param>
	/// <returns>This <see cref="Suspicious{TResult}" />.</returns>
	/// <remarks>To use this method, this <see cref="Suspicious{TResult}" /> must have been created from an error.</remarks>
	/// <exception cref="InvalidOperationException">If this <see cref="Suspicious{TResult}" /> wasn't created from an error.</exception>
	public Suspicious<TResult> WithError(Error error)
	{
		if (!this.FromError) throw new InvalidOperationException($"The {nameof(Suspicious<TResult>)} wasn't created from an error");
		if (this._errorSet is null) throw new UnreachableException($"The error set is null but '.{nameof(this.FromError)}' is {this.FromError}");
		this._errorSet.WithError(error);
		return this;
	}

	/// <summary>Adds multiple errors to the error set.</summary>
	/// <param name="errors">The errors.</param>
	/// <returns>This <see cref="Suspicious{TResult}" />.</returns>
	/// <remarks>To use this method, this <see cref="Suspicious{TResult}" /> must have been created from an error.</remarks>
	/// <exception cref="InvalidOperationException">If this <see cref="Suspicious{TResult}" /> wasn't created from an error.</exception>
	public Suspicious<TResult> WithErrors(IEnumerable<Error> errors)
	{
		if (!this.FromError) throw new InvalidOperationException($"The {nameof(Suspicious<TResult>)} wasn't created from an error");
		if (this._errorSet is null) throw new UnreachableException($"The error set is null but '.{nameof(this.FromError)}' is {this.FromError}");
		this._errorSet.WithErrors(errors);
		return this;
	}

	/// <summary>Finds the most critical error kind based on priority.</summary>
	/// <returns>An <see cref="ErrorKind" /> with the highest priority.</returns>
	/// <exception cref="InvalidOperationException">If this <see cref="Suspicious{TResult}" /> doesn't contain any errors.</exception>
	public ErrorKind FindTheMostCriticalErrorKind()
	{
		if (!this.HasErrors) throw new InvalidOperationException("The error set doesn't contain any errors");
		if (this._errorSet is null) throw new UnreachableException($"The error set is null but '.{nameof(this.HasErrors)}' is {this.HasErrors}");
		return ErrorKind.WithHighestPriority(this._errorSet.AllErrors.Select(e => e.Kind));
	}

	#endregion

	#region Instance Utilities

	/// <summary>Prints members.</summary>
	/// <param name="builder">The <see cref="StringBuilder" />.</param>
	/// <returns><c>true</c>, if members should be printed, <c>false</c>, otherwise.</returns>
	private bool PrintMembers(StringBuilder builder)
	{
		var previousMemberExists = false;

		if (this._result is not null)
		{
			builder.Append($"Result = {this._result}");
			previousMemberExists = true;
		}

		if (this._errorSet is not null)
		{
			if (previousMemberExists) builder.Append(", ");
			builder.Append(this._errorSet);
		}

		return true;
	}

	/// <summary>Prints members in redacted mode.</summary>
	/// <param name="builder">The <see cref="StringBuilder" />.</param>
	/// <returns><c>true</c>, if members should be printed, <c>false</c>, otherwise.</returns>
	private bool PrintMembersRedacted(StringBuilder builder)
	{
		var previousMemberExists = false;

		if (this._result is not null)
		{
			builder.Append($"Result = {this._result.ToString()}");
			previousMemberExists = true;
		}

		if (this._errorSet is not null)
		{
			if (previousMemberExists) builder.Append(", ");
			builder.Append(this._errorSet.ToStringRedacted());
		}

		return true;
	}

	/// <summary>Creates a string that represents this instance in redacted mode.</summary>
	/// <returns>A string that represents this instance in redacted mode.</returns>
	public string ToStringRedacted()
	{
		var builder = new StringBuilder();

		builder.Append("{ ");
		if (this.PrintMembersRedacted(builder)) builder.Append(' ');
		builder.Append('}');

		return builder.ToString();
	}

	#endregion

	#region Static API

	/// <inheritdoc cref="From(TResult)" />
	public static implicit operator Suspicious<TResult>(TResult result) => Suspicious<TResult>.From(result);

	/// <summary>Creates a <see cref="Suspicious{TResult}" /> from a <paramref name="result" />.</summary>
	/// <param name="result">The <paramref name="result" />.</param>
	/// <returns>A new <see cref="Suspicious{TResult}" />.</returns>
	internal static Suspicious<TResult> From(TResult result) => new (result);

	/// <summary>Creates a <see cref="Suspicious{TResult}" /> from an <paramref name="errorSet" />.</summary>
	/// <param name="errorSet">The <see cref="Monad.ErrorSet"/>.</param>
	/// <returns>A new <see cref="Suspicious{TResult}" />.</returns>
	internal static Suspicious<TResult> From(ErrorSet errorSet) => new (errorSet);

	#endregion
}