using System;
using System.Text;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;
using Rumrunner0.BackToReality.SharedExtensions.Extensions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Error related to <see cref="ErrorCollection" />.</summary>
public sealed record class Error
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
	private Error(ErrorKind kind, string description, string? details = null)
	{
		ArgumentExceptionExtensions.ThrowIfNull(kind);
		ArgumentExceptionExtensions.ThrowIfNullOrEmptyOrWhiteSpace(description);

		this._kind = kind;
		this._description = description;
		this._details = details;
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
	public Error? Cause
	{
		get => this._cause;
		private set => this._cause = value;
	}

	/// <summary>Sets an inner <see cref="Error" /> that caused this one.</summary>
	/// <remarks><c>null</c> can be used to remove the existing inner <see cref="Error" />.</remarks>
	/// <param name="error">The inner <see cref="Error" /> to set, or <c>null</c> to remove it.</param>
	/// <returns>This <see cref="Error" />.</returns>
	public Error SetCause(Error? error)
	{
		this._cause = error;
		return this;
	}

	#endregion

	#region Instance Utilities

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

	/// <summary>Creates a string that represents this instance.</summary>
	/// <returns>A string that represents this instance.</returns>
	public override string ToString()
	{
		// TODO: ADD MULTILINE FORMATTING??? just formatted JSON I think, please!

		var builder = new StringBuilder();

		builder.Append("{ ");
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

	/// <summary>Creates a custom <see cref="Error" />.</summary>
	/// <param name="kind">The kind.</param>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <returns>A new custom error.</returns>
	public static Error Custom(ErrorKind kind, string description, Error? cause = null)
	{
		return new
		(
			kind,
			description
		)
		{
			Cause = cause
		};
	}

	/// <summary>Creates a <see cref="ErrorKind.NoResult" /> <see cref="Error" />.</summary>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <returns>A new <see cref="ErrorKind.Failure" /> error.</returns>
	public static Error NoResult(string description, Error? cause = null)
	{
		return new
		(
			ErrorKind.NoResult,
			description
		)
		{
			Cause = cause
		};
	}

	/// <summary>Creates a <see cref="ErrorKind.Failure" /> <see cref="Error" />.</summary>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <returns>A new <see cref="ErrorKind.Failure" /> error.</returns>
	public static Error Failure(string description, Error? cause = null)
	{
		return new
		(
			ErrorKind.Failure,
			description
		)
		{
			Cause = cause
		};
	}

	/// <summary>Creates a <see cref="ErrorKind.Failure" /> <see cref="Error" />.</summary>
	/// <param name="e">The exception that caused this <see cref="Error" />.</param>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <returns>A new <see cref="ErrorKind.Failure" /> error.</returns>
	public static Error Failure(Exception e, string description, Error? cause = null)
	{
		ArgumentExceptionExtensions.ThrowIfNull(e);
		ArgumentExceptionExtensions.ThrowIfNullOrEmptyOrWhiteSpace(description);

		return new
		(
			ErrorKind.Failure,
			description: $"{description}. {e.JoinMessages(" <-- ")}",
			details: e.ToString()
		)
		{
			Cause = cause
		};
	}

	/// <summary>Creates an <see cref="ErrorKind.Unexpected" /> <see cref="Error" />.</summary>
	/// <param name="e">The exception that caused this <see cref="Error" />.</param>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <returns>A new <see cref="ErrorKind.Unexpected" /> error.</returns>
	public static Error Unexpected(Exception e, string? description = null, Error? cause = null)
	{
		ArgumentExceptionExtensions.ThrowIfNull(e);

		var richDescription = new StringBuilder();
		richDescription.Append("Unexpected error has occured.");
		if (!description.IsNullOrEmptyOrWhitespace()) richDescription.Append($" {description}.");
		richDescription.Append($" {e.JoinMessages(" <-- ")}");

		return new
		(
			ErrorKind.Unexpected,
			description: richDescription.ToString(),
			details: e.ToString()
		)
		{
			Cause = cause
		};
	}

	/// <summary>Creates an <see cref="ErrorKind.Unexpected" /> <see cref="Error" />.</summary>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <returns>A new <see cref="ErrorKind.Unexpected" /> error.</returns>
	public static Error Unexpected(string description, Error? cause = null)
	{
		var richDescription = new StringBuilder();
		richDescription.Append("Unexpected error has occured.");
		if (!description.IsNullOrEmptyOrWhitespace()) richDescription.Append($" {description}.");

		return new
		(
			ErrorKind.Unexpected,
			description: richDescription.ToString()
		)
		{
			Cause = cause
		};
	}

	#endregion
}