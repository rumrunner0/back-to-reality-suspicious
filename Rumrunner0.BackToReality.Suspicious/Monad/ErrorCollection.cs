using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;
using Rumrunner0.BackToReality.SharedExtensions.Collections;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Collection of <see cref="Error"/>s related to <see cref="Suspicious{TResult}" />.</summary>
public sealed class ErrorCollection
{
	#region Instance State

	/// <summary>Category.</summary>
	private readonly ErrorCollectionCategory _category;

	/// <summary>Header.</summary>
	private readonly string _header;

	/// <summary>Errors.</summary>
	private readonly HashSet<Error> _errors;

	/// <summary>Inner error collection that caused this one.</summary>
	private ErrorCollection? _cause;

	/// <inheritdoc cref="ErrorCollection" />
	private ErrorCollection(ErrorCollectionCategory category, string header, IEnumerable<Error> errors, ErrorCollection? cause = null)
	{
		ArgumentExceptionExtensions.ThrowIfNull(category);
		ArgumentExceptionExtensions.ThrowIfNullOrEmptyOrWhiteSpace(header);
		ArgumentExceptionExtensions.ThrowIfNull(errors);

		this._category = category;
		this._header = header;
		this._errors = [..errors];
		this._cause = cause;
	}

	#endregion

	#region Instance API

	/// <summary>Category.</summary>
	public ErrorCollectionCategory Category => this._category;

	/// <summary>Header.</summary>
	public string Header => this._header;

	/// <summary>Errors.</summary>
	public IReadOnlySet<Error> Errors => this._errors;

	/// <summary>Inner error collection that caused this one.</summary>
	public ErrorCollection? Cause => this._cause;

	/// <summary>Flag that indicates whether any errors exist.</summary>
	public bool HasErrors => this._errors.Any() || this._cause is { HasErrors: true };

	/// <summary>Tries to add an <paramref name="error" />.</summary>
	/// <param name="error">The <see cref="Error" />.</param>
	/// <returns><c>true</c>, if the <paramref name="error" /> has been added; <c>false</c>, otherwise.</returns>
	public bool TryAddError(Error error)
	{
		return this._errors.Add(error);
	}

	/// <summary>Adds an <paramref name="error" />.</summary>
	/// <param name="error">The <see cref="Error" />.</param>
	/// <returns>This <see cref="ErrorCollection" />.</returns>
	/// <exception cref="InvalidOperationException">Thrown if identical <paramref name="error" /> has already been added.</exception>
	public ErrorCollection AddError(Error error)
	{
		if (!this._errors.Add(error)) throw new InvalidOperationException($"Identical error {error} has already been added");
		return this;
	}

	/// <summary>Sets an inner <see cref="ErrorCollection" /> that caused this one.</summary>
	/// <param name="errorCollection">The inner <see cref="ErrorCollection" />to set, or <c>null</c> to remove it.</param>
	/// <returns>This <see cref="ErrorCollection" />.</returns>
	/// <remarks><c>null</c> can be used to remove the existing inner <see cref="ErrorCollection" />.</remarks>
	public ErrorCollection SetCause(ErrorCollection? errorCollection)
	{
		if (errorCollection == this) ArgumentExceptionExtensions.Throw("Cause collection can't be the same as the collection for which the cause is being set");

		this._cause = errorCollection;
		return this;
	}

	/// <summary>Retrieves the most critical <see cref="Error" />.</summary>
	/// <returns>The <see cref="Error" />.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the collection doesn't have any errors.</exception>
	/// <exception cref="UnreachableException">Thrown if most critical error can't be retrieved.</exception>
	public Error GetTheMostCriticalError()
	{
		if (!this.HasErrors) throw new InvalidOperationException("The collection doesn't have any errors");

		var result = this.GetTheMostCriticalErrorInChain();
		if (result is null) throw new UnreachableException("The most critical error can't be retrieved");

		return result;
	}

	#endregion

	#region Instance Utilities

	/// <summary>Retrieves the most critical <see cref="Error" /> in the chain.</summary>
	/// <returns>The <see cref="Error" />.</returns>
	internal Error? GetTheMostCriticalErrorInChain()
	{
		return Error.GetTheMostCritical
		(
			this.GetTheMostCriticalErrorFromCurrent(),
			this._cause?.GetTheMostCriticalErrorInChain()
		);
	}

	/// <summary>Retrieves the most critical <see cref="Error" /> from the current <see cref="ErrorCollection" />.</summary>
	/// <returns>The <see cref="Error" />.</returns>
	internal Error? GetTheMostCriticalErrorFromCurrent()
	{
		var mostCriticalOverall = default(Error);
		foreach (var error in this._errors)
		{
			var mostCritical = error.GetTheMostCriticalErrorInChain();
			if (mostCritical.CompareTo(mostCriticalOverall) > 0)
			{
				mostCriticalOverall = mostCritical;
			}
		}

		return mostCriticalOverall;
	}

	/// <summary>Prints members.</summary>
	/// <param name="builder">The <see cref="StringBuilder" />.</param>
	/// <returns><c>true</c> if members should be printed; <c>false</c> otherwise.</returns>
	private bool PrintMembers(StringBuilder builder)
	{
		builder.Append($"Category = {this._category}");
		builder.Append($", Header = {this._header}");

		if (this._errors.Any())
		{
			builder.Append($", Errors = [ {this._errors.StringJoin(", ")} ]");
		}

		if (this._cause is not null)
		{
			builder.Append($", Cause = {this._cause}");
		}

		return true;
	}

	/// <summary>Prints members in redacted mode.</summary>
	/// <param name="builder">The <see cref="StringBuilder" />.</param>
	/// <returns><c>true</c> if members should be printed; <c>false</c> otherwise.</returns>
	private bool PrintMembersRedacted(StringBuilder builder)
	{
		builder.Append(this._header);

		if (this._errors.Any())
		{
			builder.Append($" [ {this._errors.Select(e => e.ToStringRedacted()).StringJoin(", ")} ]");
		}

		if (this._cause is not null)
		{
			builder.Append($" <== {this._cause.ToStringRedacted()}");
		}

		return true;
	}

	/// <summary>Creates a string that represents this instance.</summary>
	/// <returns>A string that represents this instance.</returns>
	public override string ToString()
	{
		var builder = new StringBuilder();

		builder.Append($"{nameof(ErrorCollection)} {{ ");
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

	/// <summary>Empty <see cref="ErrorCollection" />.</summary>
	public static ErrorCollection Empty(ErrorCollectionCategory category, string header) => new (category, header, []);

	/// <summary>Empty <see cref="ErrorCollection" />.</summary>
	public static ErrorCollection New(ErrorCollectionCategory category, string header, IEnumerable<Error> errors) => new (category, header, errors);

	#endregion
}