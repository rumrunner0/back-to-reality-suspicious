using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rumrunner0.BackToReality.SharedExtensions.Extensions;
using Rumrunner0.BackToReality.Suspicious.Extensions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>
/// Collection of <see cref="Error"/>s related to <see cref="Suspicious{TResult}" />.
/// </summary>
public sealed record class ErrorCollection
{
	#region Instance State

	/// <summary>Category.</summary>
	private readonly ErrorCollectionCategory _category;

	/// <summary>Header.</summary>
	private readonly string _header;

	/// <summary>Errors.</summary>
	private readonly List<Error> _errors;

	/// <summary>Inner collection of errors.</summary>
	private ErrorCollection? _innerCollection;

	/// <inheritdoc cref="ErrorCollection" />
	private ErrorCollection(ErrorCollectionCategory category, string header, IEnumerable<Error> errors, ErrorCollection? innerCollection = null)
	{
		ArgumentNullExceptionHelper.ThrowIfNull(category);
		ArgumentExceptionHelper.ThrowIfNullOrEmptyOrWhiteSpace(header);
		ArgumentNullExceptionHelper.ThrowIfNull(errors);

		this._category = category;
		this._header = header;
		this._errors = [..errors];
		this._innerCollection = innerCollection;
	}

	#endregion

	#region Instance API

	/// <inheritdoc cref="_category" />
	public ErrorCollectionCategory Category => this._category;

	/// <inheritdoc cref="_header" />
	public string Header => this._header;

	/// <inheritdoc cref="_errors" />
	public IReadOnlyList<Error> Errors => this._errors;

	/// <inheritdoc cref="_innerCollection" />
	public ErrorCollection? InnerCollection => this._innerCollection;

	/// <summary>All errors, including those contained in <see cref="InnerCollection" />.</summary>
	public IEnumerable<Error> AllErrors => this._innerCollection is { } ? this._errors.Concat(this._innerCollection.AllErrors) : this._errors;

	/// <summary>Flag that indicates whether any errors exist.</summary>
	public bool HasErrors => this._errors.Any() || this._innerCollection is { HasErrors: true };

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

	/// <summary>Sets an inner <see cref="ErrorCollection" />.</summary>
	/// <param name="inner">The inner <see cref="ErrorCollection" />.</param>
	/// <returns>This <see cref="ErrorCollection" />.</returns>
	public ErrorCollection SetInnerCollection(ErrorCollection inner)
	{
		this._innerCollection = inner;
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

		if (this._innerCollection is not null)
		{
			builder.Append($", InnerCollection = {this._innerCollection}");
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

		if (this._innerCollection is not null)
		{
			builder.Append($" <== {this._innerCollection.ToStringRedacted()}");
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

	/// <summary>Empty <see cref="ErrorCollection" />.</summary>
	public static ErrorCollection Empty(string category, string header) => new (ErrorCollectionCategory.Custom(category), header, []);

	/// <summary>Empty <see cref="ErrorCollection" />.</summary>
	public static ErrorCollection Empty(ErrorCollectionCategory category, string header) => new (category, header, []);

	/// <summary>Empty <see cref="ErrorCollection" />.</summary>
	public static ErrorCollection New(string category, string header, IEnumerable<Error> errors) => new (ErrorCollectionCategory.Custom(category), header, errors);

	/// <summary>Empty <see cref="ErrorCollection" />.</summary>
	public static ErrorCollection New(ErrorCollectionCategory category, string header, IEnumerable<Error> errors) => new (category, header, errors);

	#endregion
}