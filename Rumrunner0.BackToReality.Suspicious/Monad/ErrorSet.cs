using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rumrunner0.BackToReality.SharedExtensions.Extensions;
using Rumrunner0.BackToReality.Suspicious.Extensions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>
/// Set of <see cref="Error"/>s related to <see cref="Suspicious{TResult}" />.
/// </summary>
public sealed record class ErrorSet
{
	#region Instance State

	/// <summary>Category.</summary>
	private readonly ErrorSetCategory _category;

	/// <summary>Header.</summary>
	private readonly string _header;

	/// <summary>Errors.</summary>
	private readonly HashSet<Error> _errors;

	/// <summary>Inner set of errors.</summary>
	private ErrorSet? _innerSet;

	#endregion

	#region Instance API

	/// <inheritdoc cref="ErrorSet" />
	public ErrorSet(string category, string header, ErrorSet? innerSet = null) : this(ErrorSetCategory.Custom(category), header, innerSet) { }

	/// <inheritdoc cref="ErrorSet" />
	public ErrorSet(ErrorSetCategory category, string header, ErrorSet? innerSet = null) : this(category, header, [], innerSet) { }

	/// <inheritdoc cref="ErrorSet" />
	public ErrorSet(string category, string header, IEnumerable<Error> errors, ErrorSet? innerSet = null) : this(ErrorSetCategory.Custom(category), header, errors, innerSet) { }

	/// <inheritdoc cref="ErrorSet" />
	public ErrorSet(ErrorSetCategory category, string header, IEnumerable<Error> errors, ErrorSet? innerSet = null)
	{
		ArgumentNullExceptionHelper.ThrowIfNull(category);
		ArgumentExceptionHelper.ThrowIfNullOrEmptyOrWhiteSpace(header);
		ArgumentNullExceptionHelper.ThrowIfNull(errors);

		this._category = category;
		this._header = header;
		this._errors = [..errors];
		this._innerSet = innerSet;
	}

	/// <inheritdoc cref="_category" />
	public ErrorSetCategory Category => this._category;

	/// <inheritdoc cref="_header" />
	public string Header => this._header;

	/// <inheritdoc cref="_errors" />
	public HashSet<Error> Errors => this._errors;

	/// <inheritdoc cref="_innerSet" />
	public ErrorSet? InnerSet => this._innerSet;

	/// <summary>All errors, including those contained in <see cref="InnerSet" />.</summary>
	public IEnumerable<Error> AllErrors => this._innerSet is { } ? this._errors.Concat(this._innerSet.AllErrors) : this._errors;

	/// <summary>Flag that indicates whether any errors exist.</summary>
	public bool HasErrors => this._errors.Any() || this._innerSet is { HasErrors: true };

	/// <summary>Adds an error.</summary>
	/// <param name="error">The <see cref="Error" />.</param>
	/// <returns>This <see cref="ErrorSet" />.</returns>
	public ErrorSet WithError(Error error)
	{
		this._errors.Add(error);
		return this;
	}

	/// <summary>Adds errors.</summary>
	/// <param name="errors">The <see cref="Error" />s.</param>
	/// <returns>This <see cref="ErrorSet" />.</returns>
	public ErrorSet WithErrors(IEnumerable<Error> errors)
	{
		this._errors.UnionWith(errors);
		return this;
	}

	/// <summary>Sets an inner error set.</summary>
	/// <param name="inner">The inner <see cref="ErrorSet" />.</param>
	/// <returns>This <see cref="ErrorSet" />.</returns>
	public ErrorSet WithInnerSet(ErrorSet inner)
	{
		this._innerSet = inner;
		return this;
	}

	#endregion

	#region Instance Utilities

	/// <summary>Prints members.</summary>
	/// <param name="builder">The <see cref="StringBuilder" />.</param>
	/// <returns><c>true</c>, if members should be printed, <c>false</c>, otherwise.</returns>
	private bool PrintMembers(StringBuilder builder)
	{
		builder.Append($"Category = {this._category}");
		builder.Append($", Header = {this._header}");
		builder.Append($", Errors = [ {this._errors.StringJoin(", ")} ]");
		builder.Append($", InnerSet = {this._innerSet?.ToString() ?? "null"}");
		return true;
	}

	/// <summary>Prints members in redacted mode.</summary>
	/// <param name="builder">The <see cref="StringBuilder" />.</param>
	/// <returns><c>true</c>, if members should be printed, <c>false</c>, otherwise.</returns>
	private bool PrintMembersRedacted(StringBuilder builder)
	{
		builder.Append($"Category = {this._category}");
		builder.Append($", Header = {this._header}");

		if (this._errors.Any())
		{
			builder.Append($", Errors = [ {this._errors.Select(e => e.ToStringRedacted()).StringJoin(", ")} ]");
		}

		if (this._innerSet is not null)
		{
			builder.Append($", InnerSet = {this._innerSet.ToStringRedacted()}");
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

	/// <summary>Empty <see cref="ErrorSet" />.</summary>
	public static ErrorSet Empty(string category, string header) => new (category, header);

	/// <summary>Empty <see cref="ErrorSet" />.</summary>
	public static ErrorSet Empty(ErrorSetCategory category, string header) => new (category, header);

	#endregion
}