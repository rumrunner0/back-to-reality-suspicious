using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;
using Rumrunner0.BackToReality.SharedExtensions.Collections;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Collection of <see cref="Error"/>s.</summary>
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
		this.EnsureCauseDoesNotCreateCycle(cause);

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

	/// <summary>Flag that indicates whether the collection contains any errors.</summary>
	public bool ContainsErrors => this._errors.Any();

	/// <summary>Flag that indicates whether the collection contains any errors in the cause chain, including self.</summary>
	public bool ContainsErrorsDeep
	{
		get
		{
			for (var current = this; current is not null; current = current._cause)
			{
				if (current.ContainsErrors) return true;
			}

			return false;
		}
	}

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
		if (!this.TryAddError(error)) throw new InvalidOperationException($"Identical error {error} has already been added");
		return this;
	}

	/// <summary>Sets an inner <see cref="ErrorCollection" /> that caused this one.</summary>
	/// <param name="cause">The inner <see cref="ErrorCollection" />to set, or <c>null</c> to remove it.</param>
	/// <returns>This <see cref="ErrorCollection" />.</returns>
	/// <remarks><c>null</c> can be used to remove the existing inner <see cref="ErrorCollection" />.</remarks>
	public ErrorCollection SetCause(ErrorCollection? cause)
	{
		this.EnsureCauseDoesNotCreateCycle(cause);
		this._cause = cause;
		return this;
	}

	/// <summary>Searches for the first <see cref="Error" /> with the provided <paramref name="kind" /> among errors only in the current collection.</summary>
	/// <param name="kind">The kind.</param>
	/// <returns>An <see cref="Error" /> or <c>null</c>.</returns>
	public Error? FindError(ErrorKind kind)
	{
		return this._errors.FirstOrDefault(e => e.Find(kind) is not null);
	}

	/// <summary>Searches for the first <see cref="Error" /> with the provided <paramref name="kind" /> among all errors in the cause chain, including self.</summary>
	/// <param name="kind">The kind.</param>
	/// <returns>An <see cref="Error" /> or <c>null</c>.</returns>
	public Error? FindErrorDeep(ErrorKind kind)
	{
		for (var current = this; current is not null; current = current._cause)
		{
			foreach (var error in current._errors)
			{
				if (error.Find(kind) is { } target) return target;
			}
		}

		return null;
	}

	/// <summary>Searches for the most critical <see cref="Error" /> among all errors in the cause chain, including self.</summary>
	/// <returns>The <see cref="Error" />.</returns>
	public Error? FindMostCriticalErrorDeep()
	{
		var target = default(Error?);

		for (var current = this; current is not null; current = current._cause)
		{
			foreach (var error in current._errors)
			{
				var candidate = error.FindMostCritical();
				if (candidate.CompareTo(target) > 0) target = candidate;
			}
		}

		return target;
	}

	#endregion

	#region Instance Utilities

	/// <summary>Ensures that <paramref name="cause" /> doesn't create a cycle.</summary>
	/// <param name="cause">The cause.</param>
	/// <exception cref="ArgumentException">Thrown if the <paramref name="cause" /> already contains a cycle or would create a cycle.</exception>
	private void EnsureCauseDoesNotCreateCycle(ErrorCollection? cause)
	{
		if (cause is null) return;
		if (ReferenceEquals(cause, this)) ArgumentExceptionExtensions.Throw("An instance cannot be its own cause", cause);

		var visited = HashSetFactory.ReferenceEquality<ErrorCollection>();

		for (var current = cause; current is not null; current = current._cause)
		{
			if (!visited.Add(current)) ArgumentExceptionExtensions.Throw("Cause chain already contains a cycle", cause);
			if (ReferenceEquals(current, this)) ArgumentExceptionExtensions.Throw("Setting the cause would create a cycle", cause);
		}
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