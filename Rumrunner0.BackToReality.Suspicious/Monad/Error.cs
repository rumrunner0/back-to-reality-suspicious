using System;
using System.Text;
using Rumrunner0.BackToReality.Suspicious.Extensions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>
/// An error that is part of an <see cref="ErrorSet" />.
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
	private readonly Error? _innerError;

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
	public Error? InnerError { get => this._innerError; private init => this._innerError = value; }

	#endregion

	#region Instance Utilities

	/// <summary>Prints members.</summary>
	/// <param name="builder">The <see cref="StringBuilder" />.</param>
	/// <returns><c>true</c>, if members should be printed, <c>false</c>, otherwise.</returns>
	private bool PrintMembers(StringBuilder builder)
	{
		builder.Append($"Kind = {this._kind}");
		builder.Append($", Description = {this._description}");
		builder.Append($", Details = {this._details ?? "null"}");
		builder.Append($", InnerError = {this._innerError?.ToString() ?? "null"}");
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

	/// <summary>Creates <see cref="ErrorKind.Failure" /> error.</summary>
	/// <param name="description">Description.</param>
	/// <param name="innerError">The inner error.</param>
	/// <returns>A new <see cref="ErrorKind.Failure" /> error.</returns>
	public static Error Failure(string description, Error? innerError = null)
	{
		return new (ErrorKind.Failure, description)
		{
			InnerError = innerError
		};
	}

	/// <summary>Creates <see cref="ErrorKind.Unexpected" /> error.</summary>
	/// <param name="description">Description.</param>
	/// <param name="e">Exception that caused this error.</param>
	/// <returns>A new <see cref="ErrorKind.Unexpected" /> error.</returns>
	public static Error Unexpected(Exception e, string? description = null)
	{
		ArgumentNullExceptionHelper.ThrowIfNull(e);
		return new (ErrorKind.Unexpected, description: $"Unexpected error has occured: {description ?? e.Message}")
		{
			Details = e.ToString()
		};
	}

	#endregion
}