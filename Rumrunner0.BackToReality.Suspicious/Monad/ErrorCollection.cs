using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;
using Rumrunner0.BackToReality.SharedExtensions.Extensions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Collection of <see cref="Error"/>s related to <see cref="Suspicious{TResult}" />.</summary>
public sealed record class ErrorCollection
{
	#region Instance State

	/// <summary>Category.</summary>
	private readonly ErrorCollectionCategory _category;

	/// <summary>Header.</summary>
	private readonly string _header;

	/// <summary>Errors.</summary>
	private readonly List<Error> _errors;

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
	public IReadOnlyList<Error> Errors => this._errors;

	/// <summary>Inner error collection that caused this one.</summary>
	public ErrorCollection? Cause => this._cause;

	/// <summary>All errors, including those contained in <see cref="Cause" />.</summary>
	public IEnumerable<Error> AllErrors => this._cause is { } ? this._errors.Concat(this._cause.AllErrors) : this._errors;

	/// <summary>Flag that indicates whether any errors exist.</summary>
	public bool HasErrors => this._errors.Any() || this._cause is { HasErrors: true };

	/// <summary>Adds an error.</summary>
	/// <param name="error">The <see cref="Error" />.</param>
	/// <returns>This <see cref="ErrorCollection" />.</returns>
	public ErrorCollection AddError(Error error)
	{
		this._errors.Add(error);
		return this;
	}

	/// <summary>Adds errors.</summary>
	/// <param name="errors">The <see cref="Error" />s.</param>
	/// <returns>This <see cref="ErrorCollection" />.</returns>
	public ErrorCollection AddErrors(IEnumerable<Error> errors)
	{
		this._errors.AddRange(errors);
		return this;
	}

	/// <summary>Sets an inner <see cref="ErrorCollection" /> that caused this one.</summary>
	/// <param name="errorCollection">The inner <see cref="ErrorCollection" />.</param>
	/// <returns>This <see cref="ErrorCollection" />.</returns>
	public ErrorCollection SetCause(ErrorCollection errorCollection)
	{
		// TODO: Add check to prevent inner be the same and current.
		// I need to find a way to prevent all kinds of circular dependency.

		this._cause = errorCollection;
		return this;
	}

	#endregion

	#region Instance Utilities

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