using System;
using System.Collections.Generic;
using System.Text;
using Rumrunner0.BackToReality.SharedExtensions.Collections;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;
using Rumrunner0.BackToReality.SharedExtensions.Extensions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Error.</summary>
public sealed record class Error : IComparable<Error>
{
	#region Insance State

	/// <summary>Kind.</summary>
	private readonly ErrorKind _kind;

	/// <summary>Description.</summary>
	private readonly string _description;

	/// <summary>Details.</summary>
	private readonly string? _details;

	/// <summary>Inner error that caused this one.</summary>
	private Error? _cause;

	/// <inheritdoc cref="Error" />
	private Error(ErrorKind kind, string description, string? details = null, Error? cause = null)
	{
		ArgumentExceptionExtensions.ThrowIfNull(kind);
		ArgumentExceptionExtensions.ThrowIfNullOrEmptyOrWhiteSpace(description);
		this.EnsureCauseDoesNotCreateCycle(cause);

		this._kind = kind;
		this._description = description;
		this._details = details;
		this._cause = cause;
	}

	#endregion

	#region Instance API

	/// <summary>Kind.</summary>
	public ErrorKind Kind => this._kind;

	/// <summary>Description.</summary>
	public string Description => this._description;

	/// <summary>Details.</summary>
	public string? Details => this._details;

	/// <summary>Inner error that caused this one.</summary>
	public Error? Cause => this._cause;

	/// <summary>Sets an inner <see cref="Error" /> that caused this one.</summary>
	/// <param name="cause">The inner <see cref="Error" /> to set, or <c>null</c> to remove it.</param>
	/// <returns>This <see cref="Error" />.</returns>
	/// <remarks><c>null</c> can be used to remove the existing inner <see cref="Error" />.</remarks>
	public Error SetCause(Error? cause)
	{
		this.EnsureCauseDoesNotCreateCycle(cause);
		this._cause = cause;
		return this;
	}

	/// <summary>Searches for the first <see cref="Error" /> with the provided <paramref name="kind" /> among all errors in the cause, chain including self.</summary>
	/// <returns>The <see cref="Error" />.</returns>
	public Error? Find(ErrorKind kind)
	{
		for (var current = this; current is not null; current = current._cause)
		{
			if (current.Kind == kind) return current;
		}

		return null;
	}

	/// <summary>Searches for the most critical <see cref="Error" /> among all errors in the cause chain, including self.</summary>
	/// <returns>The <see cref="Error" />.</returns>
	public Error FindMostCritical()
	{
		var target = this;

		for (var current = this; current is not null; current = current._cause)
		{
			if (current.CompareTo(target) > 0) target = current;
		}

		return target;
	}

	/// <inheritdoc />
	public int CompareTo(Error? other)
	{
		return _kindComparer.Compare(this, other);
	}

	#endregion

	#region Instance Utilities

	/// <summary>Ensures that <paramref name="cause" /> doesn't create a cycle.</summary>
	/// <param name="cause">The cause.</param>
	/// <exception cref="ArgumentException">Thrown if the <paramref name="cause" /> already contains a cycle or would create a cycle.</exception>
	private void EnsureCauseDoesNotCreateCycle(Error? cause)
	{
		if (cause is null) return;
		if (ReferenceEquals(cause, this)) ArgumentExceptionExtensions.Throw("An instance cannot be its own cause", cause);

		var visited = HashSetFactory.ReferenceEquality<Error>();

		for (var current = cause; current is not null; current = current._cause)
		{
			if (!visited.Add(current)) ArgumentExceptionExtensions.Throw("Cause chain already contains a cycle", cause);
			if (ReferenceEquals(current, this)) ArgumentExceptionExtensions.Throw("Setting the cause would create a cycle", cause);
		}
	}

	/// <summary>Creates a string that represents this instance.</summary>
	/// <returns>A string that represents this instance.</returns>
	public override string ToString()
	{
		var builder = new StringBuilder();

		builder.Append("{ ");
		if (this.PrintMembers(builder)) builder.Append(' ');
		builder.Append('}');

		return builder.ToString();
	}

	/// <summary>Prints members.</summary>
	/// <param name="builder">The <see cref="StringBuilder" />.</param>
	/// <returns><c>true</c> if members should be printed; <c>false</c> otherwise.</returns>
	private bool PrintMembers(StringBuilder builder)
	{
		builder.Append($"Kind = {this._kind}");
		builder.Append($", Description = {this._description}");

		if (this._details is not null)
		{
			builder.Append($", Details = {this._details}");
		}

		if (this._cause is not null)
		{
			builder.Append($", Cause = {this._cause}");
		}

		return true;
	}

	/// <summary>Creates a string that represents this instance in redacted mode.</summary>
	/// <returns>A string that represents this instance in redacted mode.</returns>
	public string ToStringRedacted()
	{
		var builder = new StringBuilder();

		this.PrintMembersRedacted(builder);

		return builder.ToString();
	}

	/// <summary>Prints members in redacted mode.</summary>
	/// <param name="builder">The <see cref="StringBuilder" />.</param>
	/// <returns><c>true</c> if members should be printed; <c>false</c> otherwise.</returns>
	private bool PrintMembersRedacted(StringBuilder builder)
	{
		builder.Append(this._description);

		if (this._cause is not null)
		{
			builder.Append($" <-- {this._cause.ToStringRedacted()}");
		}

		return true;
	}

	#endregion

	#region Static State

	/// <inheritdoc cref="KindComparer" />
	private static readonly KindComparer _kindComparer = new ();

	/// <inheritdoc />
	private sealed class KindComparer : IComparer<Error?>
	{
		/// <inheritdoc />
		public int Compare(Error? x, Error? y)
		{
			if (x is null) return -1;
			if (y is null) return 1;
			return x._kind.CompareTo(y._kind);
		}
	}

	#endregion

	#region Static API

	/// <summary>Creates a <see cref="ErrorKind.NoValue" /> <see cref="Error" />.</summary>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <returns>A new <see cref="ErrorKind.NoValue" /> error.</returns>
	public static Error NoValue(string description, Error? cause = null)
	{
		ArgumentExceptionExtensions.ThrowIfNullOrEmptyOrWhiteSpace(description);

		return new
		(
			kind: ErrorKind.NoValue,
			description: description,
			details: null,
			cause: cause
		);
	}

	/// <summary>Creates a <see cref="ErrorKind.Failure" /> <see cref="Error" />.</summary>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <returns>A new <see cref="ErrorKind.Failure" /> error.</returns>
	public static Error Failure(string description, Error? cause = null)
	{
		ArgumentExceptionExtensions.ThrowIfNullOrEmptyOrWhiteSpace(description);

		return new
		(
			kind: ErrorKind.Failure,
			description: description,
			details: null,
			cause: cause
		);
	}

	/// <summary>Creates a <see cref="ErrorKind.Failure" /> <see cref="Error" />.</summary>
	/// <param name="e">The exception that caused this <see cref="Error" />.</param>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <returns>A new <see cref="ErrorKind.Failure" /> error.</returns>
	public static Error Failure(Exception e, string? description = null, Error? cause = null)
	{
		ArgumentExceptionExtensions.ThrowIfNull(e);

		return new
		(
			kind: ErrorKind.Failure,
			description: !description.IsNullOrEmptyOrWhitespace() ? description : $"Failure error has occured: {e.JoinMessages(" <-- ")}",
			details: e.ToString(),
			cause: cause
		);
	}

	/// <summary>Creates an <see cref="ErrorKind.Unexpected" /> <see cref="Error" />.</summary>
	/// <param name="e">The exception that caused this <see cref="Error" />.</param>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <returns>A new <see cref="ErrorKind.Unexpected" /> error.</returns>
	public static Error Unexpected(Exception e, string? description = null, Error? cause = null)
	{
		ArgumentExceptionExtensions.ThrowIfNull(e);

		return new
		(
			kind: ErrorKind.Unexpected,
			description: !description.IsNullOrEmptyOrWhitespace() ? description : $"Unexpected error has occured: {e.JoinMessages(" <-- ")}",
			details: e.ToString(),
			cause: cause
		);
	}

	/// <summary>Creates an <see cref="ErrorKind.Unexpected" /> <see cref="Error" />.</summary>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <returns>A new <see cref="ErrorKind.Unexpected" /> error.</returns>
	public static Error Unexpected(string description, Error? cause = null)
	{
		ArgumentExceptionExtensions.ThrowIfNullOrEmptyOrWhiteSpace(description);

		return new
		(
			kind: ErrorKind.Unexpected,
			description: description,
			details: null,
			cause: cause
		);
	}

	/// <summary>Creates a custom <see cref="Error" />.</summary>
	/// <param name="kind">The kind.</param>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <returns>A new custom error.</returns>
	public static Error Custom(ErrorKind kind, string description, Error? cause = null)
	{
		ArgumentExceptionExtensions.ThrowIfNull(kind);
		ArgumentExceptionExtensions.ThrowIfNullOrEmptyOrWhiteSpace(description);

		return new
		(
			kind: kind,
			description: description,
			details: null,
			cause: cause
		);
	}

	#endregion
}