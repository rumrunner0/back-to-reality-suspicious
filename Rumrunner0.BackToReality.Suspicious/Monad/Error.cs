using System;
using System.Text;
using Rumrunner0.BackToReality.SharedExtensions.Extensions;
using Rumrunner0.BackToReality.Suspicious.Extensions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>
/// Error related to <see cref="ErrorSet" />.
/// </summary>
public sealed record class Error
{
	#region Insance State

	/// <summary>Kind.</summary>
	private readonly ErrorKind _kind;

	/// <summary>Description.</summary>
	private readonly string _description;

	/// <summary>Details.</summary>
	private readonly string? _details;

	/// <summary>Inner error.</summary>
	private Error? _innerError;

	/// <inheritdoc cref="Error" />
	private Error(ErrorKind kind, string description)
	{
		ArgumentNullExceptionHelper.ThrowIfNull(kind);
		ArgumentExceptionHelper.ThrowIfNullOrEmptyOrWhiteSpace(description);

		this._kind = kind;
		this._description = description;
	}

	#endregion

	#region Instance API

	/// <inheritdoc cref="_kind" />
	public ErrorKind Kind => this._kind;

	/// <inheritdoc cref="_description" />
	public string Description => this._description;

	/// <inheritdoc cref="_details" />
	public string? Details { get => this._details; private init => this._details = value; }

	/// <inheritdoc cref="_innerError" />
	public Error? InnerError { get => this._innerError; set => this._innerError = value; }

	/// <summary>Sets an inner <see cref="Error" /> for this <see cref="Error" />.</summary>
	/// <remarks><c>null</c> is a valid value that can be used to clear the inner <see cref="Error" />.</remarks>
	/// <param name="error">The inner <see cref="Error" />.</param>
	/// <returns>This <see cref="Error" />.</returns>
	public Error SetInnerError(Error? error)
	{
		this._innerError = error;
		return this;
	}

	#endregion

	#region Instance Utilities

	/// <summary>Prints members.</summary>
	/// <param name="builder">The <see cref="StringBuilder" />.</param>
	/// <returns><c>true</c>, if members should be printed, <c>false</c>, otherwise.</returns>
	private bool PrintMembers(StringBuilder builder)
	{
		builder.Append($"Kind = {this._kind}");
		builder.Append($", Description = {this._description}");

		if (this._details is not null)
		{
			builder.Append($", Details = {this._details}");
		}

		if (this._innerError is not null)
		{
			builder.Append($", InnerError = {this._innerError}");
		}

		return true;
	}

	/// <summary>Prints members in redacted mode.</summary>
	/// <param name="builder">The <see cref="StringBuilder" />.</param>
	/// <returns><c>true</c>, if members should be printed, <c>false</c>, otherwise.</returns>
	private bool PrintMembersRedacted(StringBuilder builder)
	{
		builder.Append($"Kind = {this._kind.ToStringRedacted()}");
		builder.Append($", Description = {this._description}");

		if (this._innerError is not null)
		{
			builder.Append($", InnerError = {this._innerError.ToStringRedacted()}");
		}

		return true;
	}

	/// <summary>Creates a string that represents this instance in redacted mode.</summary>
	/// <returns>A string that represents this instance in redacted mode.</returns>
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

		builder.Append("{ ");
		if (this.PrintMembersRedacted(builder)) builder.Append(' ');
		builder.Append('}');

		return builder.ToString();
	}

	#endregion

	#region Static API

	/// <summary>Creates a <see cref="ErrorKind.Failure" /> error.</summary>
	/// <param name="description">The description.</param>
	/// <param name="innerError">The inner <see cref="Error" />.</param>
	/// <returns>A new <see cref="ErrorKind.Failure" /> error.</returns>
	public static Error Failure(string description, Error? innerError = null)
	{
		return new
		(
			ErrorKind.Failure,
			description
		)
		{
			InnerError = innerError
		};
	}

	/// <summary>Creates an <see cref="ErrorKind.Unexpected" /> error.</summary>
	/// <param name="e">The exception that caused this error.</param>
	/// <param name="description">The description.</param>
	/// <param name="innerError">The inner <see cref="Error" />.</param>
	/// <returns>A new <see cref="ErrorKind.Unexpected" /> error.</returns>
	public static Error Unexpected(Exception e, string? description = null, Error? innerError = null)
	{
		ArgumentNullExceptionHelper.ThrowIfNull(e);

		return new
		(
			ErrorKind.Unexpected,
			description: $"Unexpected error has occured: {description ?? e.JoinMessages(" <-- ")}"
		)
		{
			Details = e.ToString(),
			InnerError = innerError
		};
	}

	#endregion
}