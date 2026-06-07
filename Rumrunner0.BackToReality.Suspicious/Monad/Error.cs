using System;
using System.Collections.Generic;
using System.Text;
using Rumrunner0.BackToReality.SharedExtensions.Collections;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;
using Rumrunner0.BackToReality.SharedExtensions.Extensions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Error.</summary>
public sealed record class Error : IEquatable<Error>, IComparable<Error>
{
	#region Insance State

	/// <summary>Kind.</summary>
	private readonly ErrorKind _kind;

	/// <summary>Description.</summary>
	private readonly string? _description;

	/// <summary>Details.</summary>
	private readonly Exception? _exception;

	/// <summary>Inner error that caused this one.</summary>
	private Error? _cause;

	/// <inheritdoc cref="Error" />
	private Error(ErrorKind kind, string? description = null, Exception? exception = null, Error? cause = null)
	{
		ArgumentExceptionExtensions.ThrowIfNull(kind);
		this.EnsureCauseDoesNotCreateCycle(cause);

		this._kind = kind;
		this._description = !description.IsNullOrEmptyOrWhitespace() ? description : null;
		this._exception = exception;
		this._cause = cause;
	}

	#endregion

	#region Instance API

	/// <summary>Kind.</summary>
	public ErrorKind Kind => this._kind;

	/// <summary>Description.</summary>
	public string? Description => this._description;

	/// <summary>Exception.</summary>
	public Exception? Exception => this._exception;

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
			if (current._kind == kind) return current;
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

	#endregion

	#region Consistency

	/// <summary>Ensures that <paramref name="cause" /> doesn't create a cycle.</summary>
	/// <param name="cause">The cause.</param>
	/// <exception cref="ArgumentException">Thrown if the <paramref name="cause" /> already contains a cycle or would create a cycle.</exception>
	private void EnsureCauseDoesNotCreateCycle(Error? cause)
	{
		if (cause is null) return;
		if (ReferenceEquals(cause, this)) ArgumentExceptionExtensions.Throw("An instance cannot be its own cause", nameof(cause));

		var visited = HashSetFactory.ReferenceEquality<Error>();

		for (var current = cause; current is not null; current = current._cause)
		{
			if (!visited.Add(current)) ArgumentExceptionExtensions.Throw("Cause chain already contains a cycle", nameof(cause));
			if (ReferenceEquals(current, this)) ArgumentExceptionExtensions.Throw("Setting the cause would create a cycle", nameof(cause));
		}
	}

	#endregion

	#region Display

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

		if (this._description is not null) builder.Append($", Description = {this._description}");
		if (this._exception is not null) builder.Append($", Exception = {this._exception}");
		if (this._cause is not null) builder.Append($", Cause = {this._cause}");

		return true;
	}

	#endregion

	// #region Equality
	//
	// /// <inheritdoc />
	// /// <remarks>Indicates whether the <see cref="Kind" /> of the current instance is equal to the <see cref="Kind" /> of another instance.</remarks>
	// public bool Equals(Error? other)
	// {
	// 	if (object.ReferenceEquals(this, other)) return true;
	//
	// 	return
	// 		other is not null &&
	// 		this.EqualityContract == other.EqualityContract &&
	// 		this._kind == other._kind;
	// }
	//
	// /// <inheritdoc />
	// public override int GetHashCode()
	// {
	// 	return HashCode.Combine(this.EqualityContract, this._kind);
	// }
	//
	// #endregion

	#region Comparison

	/// <inheritdoc />
	/// <remarks>Compares the <see cref="Kind" /> of the current instance to the <see cref="Kind" /> of another instance.</remarks>
	public int CompareTo(Error? other) => _kindComparer.Compare(this, other);

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

	#region Creation

	/// <summary>Creates a <see cref="ErrorKind.NoValue" /> <see cref="Error" />.</summary>
	/// <param name="description">The description.</param>
	/// <param name="exception">The exception.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <returns>A new <see cref="ErrorKind.NoValue" /> error.</returns>
	public static Error NoValue(string? description = null, Exception? exception = null, Error? cause = null)
	{
		return new (ErrorKind.NoValue, description, exception, cause);
	}

	/// <summary>Creates a <see cref="ErrorKind.Failure" /> <see cref="Error" />.</summary>
	/// <param name="exception">The exception.</param>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <returns>A new <see cref="ErrorKind.Failure" /> <see cref="Error" />.</returns>
	public static Error Failure(Exception exception, string? description = null, Error? cause = null)
	{
		ArgumentExceptionExtensions.ThrowIfNull(exception);
		return new (ErrorKind.Failure, description, exception, cause);
	}

	/// <summary>Creates a <see cref="ErrorKind.Failure" /> <see cref="Error" />.</summary>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <returns>A new <see cref="ErrorKind.Failure" /> <see cref="Error" />.</returns>
	public static Error Failure(string? description = null, Error? cause = null)
	{
		return new (ErrorKind.Failure, description, exception: null, cause);
	}

	/// <summary>Creates an <see cref="ErrorKind.Unexpected" /> <see cref="Error" />.</summary>
	/// <param name="exception">The exception.</param>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <returns>A new <see cref="ErrorKind.Unexpected" /> <see cref="Error" />.</returns>
	public static Error Unexpected(Exception exception, string? description = null, Error? cause = null)
	{
		ArgumentExceptionExtensions.ThrowIfNull(exception);
		return new (ErrorKind.Unexpected, description, exception, cause);
	}

	/// <summary>Creates an <see cref="ErrorKind.Unexpected" /> <see cref="Error" />.</summary>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <returns>A new <see cref="ErrorKind.Unexpected" /> <see cref="Error" />.</returns>
	public static Error Unexpected(string? description = null, Error? cause = null)
	{
		return new (ErrorKind.Unexpected, description, exception: null, cause);
	}

	/// <summary>Creates a <see cref="ErrorKind.Custom" /> <see cref="Error" />.</summary>
	/// <param name="name">The name.</param>
	/// <param name="priority">The priority.</param>
	/// <param name="exception">The exception.</param>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <returns>A new <see cref="ErrorKind.Custom" /> <see cref="Error" />.</returns>
	public static Error Custom(string name, int priority, Exception exception, string? description = null, Error? cause = null)
	{
		ArgumentExceptionExtensions.ThrowIfNull(exception);
		return Custom(ErrorKind.Custom(name, priority), exception, description, cause);
	}

	/// <summary>Creates a <see cref="ErrorKind.Custom" /> <see cref="Error" />.</summary>
	/// <param name="name">The name.</param>
	/// <param name="priority">The priority.</param>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <returns>A new <see cref="ErrorKind.Custom" /> <see cref="Error" />.</returns>
	public static Error Custom(string name, int priority, string? description = null, Error? cause = null)
	{
		return Custom(ErrorKind.Custom(name, priority), description, cause);
	}

	/// <summary>Creates a <see cref="ErrorKind.Custom" /> <see cref="Error" />.</summary>
	/// <param name="kind">The kind.</param>
	/// <param name="exception">The exception.</param>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <returns>A new <see cref="ErrorKind.Custom" /> <see cref="Error" />.</returns>
	public static Error Custom(ErrorKind kind, Exception exception, string? description = null, Error? cause = null)
	{
		ArgumentExceptionExtensions.ThrowIfNull(exception);
		return new (kind, description, exception, cause);
	}

	/// <summary>Creates a <see cref="ErrorKind.Custom" /> <see cref="Error" />.</summary>
	/// <param name="kind">The kind.</param>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <returns>A new <see cref="ErrorKind.Custom" /> <see cref="Error" />.</returns>
	public static Error Custom(ErrorKind kind, string? description = null, Error? cause = null)
	{
		ArgumentExceptionExtensions.ThrowIfNull(kind);
		return new (kind, description, exception: null, cause);
	}

	#endregion
}