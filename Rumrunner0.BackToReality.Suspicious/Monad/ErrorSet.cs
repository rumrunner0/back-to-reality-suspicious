using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;
using Rumrunner0.BackToReality.SharedExtensions.Collections;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Set of <see cref="Error"/>s.</summary>
public sealed class ErrorSet
{
	#region Instance State

	/// <summary>Header.</summary>
	private readonly string _header;

	/// <summary>Errors.</summary>
	private readonly List<Error> _errors;

	/// <summary>Cause.</summary>
	private ErrorSet? _cause;

	/// <inheritdoc cref="ErrorSet" />
	private ErrorSet(string header, IEnumerable<Error> errors, ErrorSet? cause = null)
	{
		ArgumentExceptionExtensions.ThrowIfNullOrEmptyOrWhiteSpace(header);
		ArgumentExceptionExtensions.ThrowIfNull(errors);
		this.EnsureCauseDoesNotCreateCycle(cause);

		this._header = header;
		this._errors = [..errors];
		this._cause = cause;
	}

	#endregion

	#region Instance API

	/// <summary>Header.</summary>
	public string Header => this._header;

	/// <summary>Errors.</summary>
	public IReadOnlyList<Error> Errors => this._errors;

	/// <summary>Cause.</summary>
	public ErrorSet? Cause => this._cause;

	/// <summary>Flag that indicates whether the current <see cref="ErrorSet" /> contains any errors.</summary>
	public bool ContainsErrors => this._errors.Any();

	/// <summary>Flag that indicates whether the <see cref="ErrorSet" /> contains any errors in the cause chain, including self.</summary>
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

	// /// <summary>Tries to add an <paramref name="error" />.</summary>
	// /// <param name="error">The <see cref="Error" />.</param>
	// /// <returns><c>true</c>, if the <paramref name="error" /> has been added; <c>false</c>, otherwise.</returns>
	// public bool TryAddError(Error error)
	// {
	// 	return this._errors.Add(error);
	// }

	// /// <summary>Adds an <paramref name="error" />.</summary>
	// /// <param name="error">The <see cref="Error" />.</param>
	// /// <returns>This <see cref="ErrorSet" />.</returns>
	// /// <exception cref="InvalidOperationException">Thrown if identical <paramref name="error" /> has already been added.</exception>
	// public ErrorSet AddError(Error error)
	// {
	// 	if (!this.TryAddError(error)) throw new InvalidOperationException($"Identical error {error} has already been added");
	// 	return this;
	// }

	/// <summary>Adds an <paramref name="error" />.</summary>
	/// <param name="error">The <see cref="Error" />.</param>
	/// <returns>This <see cref="ErrorSet" />.</returns>
	/// <exception cref="InvalidOperationException">Thrown if identical <paramref name="error" /> has already been added.</exception>
	public ErrorSet AddError(Error error)
	{
		this._errors.Add(error);
		return this;
	}

	/// <summary>Sets an inner <see cref="ErrorSet" /> that caused this one.</summary>
	/// <param name="cause">The inner <see cref="ErrorSet" />to set, or <c>null</c> to remove it.</param>
	/// <returns>This <see cref="ErrorSet" />.</returns>
	/// <remarks><c>null</c> can be used to remove the existing inner <see cref="ErrorSet" />.</remarks>
	public ErrorSet SetCause(ErrorSet? cause)
	{
		this.EnsureCauseDoesNotCreateCycle(cause);
		this._cause = cause;
		return this;
	}

	/// <summary>Searches for the first <see cref="Error" /> with the provided <paramref name="kind" /> among errors only in the current <see cref="ErrorSet" />.</summary>
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

	/// <summary>Ensures that <paramref name="cause" /> doesn't create a cycle.</summary>
	/// <param name="cause">The cause.</param>
	/// <exception cref="ArgumentException">Thrown if the <paramref name="cause" /> already contains a cycle or would create a cycle.</exception>
	private void EnsureCauseDoesNotCreateCycle(ErrorSet? cause)
	{
		if (cause is null) return;
		if (ReferenceEquals(cause, this)) ArgumentExceptionExtensions.Throw("An instance cannot be its own cause", nameof(cause));

		var visited = HashSetFactory.ReferenceEquality<ErrorSet>();

		for (var current = cause; current is not null; current = current._cause)
		{
			if (!visited.Add(current)) ArgumentExceptionExtensions.Throw("Cause chain already contains a cycle", nameof(cause));
			if (ReferenceEquals(current, this)) ArgumentExceptionExtensions.Throw("Setting the cause would create a cycle", nameof(cause));
		}
	}

	#endregion

	#region Display

	/// <summary>Prints members.</summary>
	/// <param name="builder">The <see cref="StringBuilder" />.</param>
	/// <returns><c>true</c> if members should be printed; <c>false</c> otherwise.</returns>
	private bool PrintMembers(StringBuilder builder)
	{
		builder.Append($"Header = {this._header}");

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

	/// <summary>Creates a string that represents this instance.</summary>
	/// <returns>A string that represents this instance.</returns>
	public override string ToString()
	{
		var builder = new StringBuilder();

		builder.Append($"{nameof(ErrorSet)} {{ ");
		if (this.PrintMembers(builder)) builder.Append(' ');
		builder.Append('}');

		return builder.ToString();
	}

	#endregion

	#region Creation

	/// <summary>Empty <see cref="ErrorSet" />.</summary>
	public static ErrorSet Empty
	(
		[CallerMemberName] string member = "",
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int line = 0
	)
	{
		return New(errors: [], member, filePath, line);
	}

	/// <summary>Empty <see cref="ErrorSet" />.</summary>
	public static ErrorSet New
	(
		IEnumerable<Error> errors,
		[CallerMemberName] string member = "",
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int line = 0
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(errors);
		ArgumentExceptionExtensions.ThrowIfNullOrEmptyOrWhiteSpace(member);
		ArgumentExceptionExtensions.ThrowIfNullOrEmptyOrWhiteSpace(filePath);

		var file = Path.GetFileName(filePath);
		var header = $"Something went wrong in {member} (file {file}, line {line})";
		return new (header, errors, cause: null);
	}

	#endregion
}