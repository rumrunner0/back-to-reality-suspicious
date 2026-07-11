using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using Rumrunner0.BackToReality.SharedExtensions.Collections;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;
using Rumrunner0.BackToReality.Suspicious.Serialization;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Error of a failed <see cref="Suspicious" /> or <see cref="Suspicious{TValue}" />.</summary>
/// <remarks>Immutable. The <see cref="Cause" /> chain is the causal axis (like <see cref="System.Exception.InnerException" />); <see cref="Details" /> is the sibling axis used only by aggregation.</remarks>
[JsonConverter(typeof(ErrorJsonConverter))]
public sealed record class Error : IEquatable<Error>
{
	#region Instance State

	/// <summary>Kind.</summary>
	private readonly OutcomeKind _kind;

	/// <summary>Description.</summary>
	private readonly string? _description;

	/// <summary>Exception.</summary>
	private readonly Exception? _exception;

	/// <summary>Call site where this <see cref="Error" /> was created.</summary>
	private readonly CallSite? _site;

	/// <summary>Inner <see cref="Error" /> that caused this one.</summary>
	private readonly Error? _cause;

	/// <summary>Child <see cref="Error" />s of an aggregate.</summary>
	private readonly IReadOnlyList<Error> _details;

	/// <inheritdoc cref="Error" />
	private Error
	(
		OutcomeKind kind,
		string? description,
		Exception? exception,
		CallSite? site,
		Error? cause,
		IReadOnlyList<Error>? details
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(kind);
		if (!kind.Side.AllowsFailure) ArgumentExceptionExtensions.Throw($"The kind {kind} doesn't allow the failure side", nameof(kind));

		this._kind = kind;
		this._description = description;
		this._exception = exception;
		this._site = site;
		this._cause = cause;
		this._details = details is not null ? [..details] : [];
	}

	#endregion

	#region Instance API

	/// <summary>Kind.</summary>
	public OutcomeKind Kind => this._kind;

	/// <summary>Description.</summary>
	public string? Description => this._description;

	/// <summary>Exception.</summary>
	public Exception? Exception => this._exception;

	/// <summary>Call site where this <see cref="Error" /> was created.</summary>
	public CallSite? Site => this._site;

	/// <summary>Inner <see cref="Error" /> that caused this one.</summary>
	public Error? Cause => this._cause;

	/// <summary>Child <see cref="Error" />s of an aggregate.</summary>
	/// <remarks>Empty unless this <see cref="Error" /> was created by <see cref="Aggregate" />.</remarks>
	public IReadOnlyList<Error> Details => this._details;

	/// <summary>Creates a copy of this <see cref="Error" /> with the provided <paramref name="cause" />.</summary>
	/// <param name="cause">The inner <see cref="Error" /> to set, or <c>null</c> to remove it.</param>
	/// <returns>A new <see cref="Error" />.</returns>
	public Error WithCause(Error? cause)
	{
		return new
		(
			this._kind,
			this._description,
			this._exception,
			this._site,
			cause,
			this._details
		);
	}

	/// <summary>Searches for the first <see cref="Error" /> with the provided <paramref name="kind" /> among self, the <see cref="Details" /> (recursively) and the <see cref="Cause" /> chain.</summary>
	/// <param name="kind">The kind.</param>
	/// <returns>An <see cref="Error" /> or <c>null</c>.</returns>
	public Error? Find(OutcomeKind kind)
	{
		ArgumentExceptionExtensions.ThrowIfNull(kind);

		if (this._kind == kind) return this;

		foreach (var detail in this._details)
		{
			if (detail.Find(kind) is { } target) return target;
		}

		return this._cause?.Find(kind);
	}

	/// <summary>Determines whether an <see cref="Error" /> with the provided <paramref name="kind" /> exists among self, the <see cref="Details" /> (recursively) and the <see cref="Cause" /> chain.</summary>
	/// <param name="kind">The kind.</param>
	/// <returns><c>true</c>, if an <see cref="Error" /> exists; <c>false</c>, otherwise.</returns>
	public bool Contains(OutcomeKind kind) => this.Find(kind) is not null;

	#endregion

	#region Equality

	/// <inheritdoc />
	/// <remarks>Structural on <see cref="Kind" />, <see cref="Description" /> and <see cref="Site" />; recursive on <see cref="Cause" />; sequential on <see cref="Details" />; REFERENCE equality on <see cref="Exception" /> (exceptions have no value semantics).</remarks>
	public bool Equals(Error? other)
	{
		if (object.ReferenceEquals(this, other)) return true;

		return
			other is not null &&
			this.EqualityContract == other.EqualityContract &&
			this._kind == other._kind &&
			this._description == other._description &&
			object.Equals(this._site, other._site) &&
			object.ReferenceEquals(this._exception, other._exception) &&
			object.Equals(this._cause, other._cause) &&
			this._details.SequenceEqual(other._details);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		var hash = new HashCode();

		hash.Add(this.EqualityContract);
		hash.Add(this._kind);
		hash.Add(this._description);
		hash.Add(this._site);
		hash.Add(this._cause);
		foreach (var detail in this._details) hash.Add(detail);

		return hash.ToHashCode();
	}

	#endregion

	#region Display

	/// <summary>Prints members.</summary>
	/// <param name="builder">The <see cref="StringBuilder" />.</param>
	/// <returns><c>true</c> if members should be printed; <c>false</c> otherwise.</returns>
	private bool PrintMembers(StringBuilder builder)
	{
		builder.Append($"Kind = {this._kind}");

		if (this._description is not null) builder.Append($", Description = {this._description}");
		if (this._site is not null) builder.Append($", Site = {this._site}");
		if (this._exception is not null) builder.Append($", Exception = {this._exception}");
		if (this._cause is not null) builder.Append($", Cause = {this._cause}");
		if (this._details.Count > 0) builder.Append($", Details = [ {this._details.StringJoin(", ")} ]");

		return true;
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

	#endregion

	#region Creation

	/// <summary>Creates an <see cref="OutcomeKind.NoValue" /> <see cref="Error" /> — a miss the producer treats as a failure.</summary>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <param name="callerMember">The caller member.</param>
	/// <param name="callerFilePath">The caller file path.</param>
	/// <param name="callerLine">The caller line.</param>
	/// <returns>A new <see cref="OutcomeKind.NoValue" /> <see cref="Error" />.</returns>
	public static Error NoValue
	(
		string? description = null,
		Error? cause = null,
		[CallerMemberName] string callerMember = "",
		[CallerFilePath] string callerFilePath = "",
		[CallerLineNumber] int callerLine = 0
	)
	{
		return new
		(
			OutcomeKind.NoValue,
			description,
			exception: null,
			CallSite.From(callerMember, callerFilePath, callerLine),
			cause,
			details: null
		);
	}

	/// <summary>Creates an <see cref="OutcomeKind.Invalid" /> <see cref="Error" />.</summary>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <param name="callerMember">The caller member.</param>
	/// <param name="callerFilePath">The caller file path.</param>
	/// <param name="callerLine">The caller line.</param>
	/// <returns>A new <see cref="OutcomeKind.Invalid" /> <see cref="Error" />.</returns>
	public static Error Invalid
	(
		string description,
		Error? cause = null,
		[CallerMemberName] string callerMember = "",
		[CallerFilePath] string callerFilePath = "",
		[CallerLineNumber] int callerLine = 0
	)
	{
		ArgumentExceptionExtensions.ThrowIfNullOrEmptyOrWhiteSpace(description);

		return new
		(
			OutcomeKind.Invalid,
			description,
			exception: null,
			CallSite.From(callerMember, callerFilePath, callerLine),
			cause,
			details: null
		);
	}

	/// <summary>Creates an <see cref="OutcomeKind.Conflict" /> <see cref="Error" />.</summary>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <param name="callerMember">The caller member.</param>
	/// <param name="callerFilePath">The caller file path.</param>
	/// <param name="callerLine">The caller line.</param>
	/// <returns>A new <see cref="OutcomeKind.Conflict" /> <see cref="Error" />.</returns>
	public static Error Conflict
	(
		string description,
		Error? cause = null,
		[CallerMemberName] string callerMember = "",
		[CallerFilePath] string callerFilePath = "",
		[CallerLineNumber] int callerLine = 0
	)
	{
		ArgumentExceptionExtensions.ThrowIfNullOrEmptyOrWhiteSpace(description);

		return new
		(
			OutcomeKind.Conflict,
			description,
			exception: null,
			CallSite.From(callerMember, callerFilePath, callerLine),
			cause,
			details: null
		);
	}

	/// <summary>Creates an <see cref="OutcomeKind.Failure" /> <see cref="Error" /> — the general expected failure.</summary>
	/// <param name="description">The description.</param>
	/// <param name="exception">The exception.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <param name="callerMember">The caller member.</param>
	/// <param name="callerFilePath">The caller file path.</param>
	/// <param name="callerLine">The caller line.</param>
	/// <returns>A new <see cref="OutcomeKind.Failure" /> <see cref="Error" />.</returns>
	public static Error Failure
	(
		string? description = null,
		Exception? exception = null,
		Error? cause = null,
		[CallerMemberName] string callerMember = "",
		[CallerFilePath] string callerFilePath = "",
		[CallerLineNumber] int callerLine = 0
	)
	{
		return new
		(
			OutcomeKind.Failure,
			description,
			exception,
			CallSite.From(callerMember, callerFilePath, callerLine),
			cause,
			details: null
		);
	}

	/// <summary>Creates an <see cref="OutcomeKind.Unavailable" /> <see cref="Error" />.</summary>
	/// <param name="description">The description.</param>
	/// <param name="exception">The exception.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <param name="callerMember">The caller member.</param>
	/// <param name="callerFilePath">The caller file path.</param>
	/// <param name="callerLine">The caller line.</param>
	/// <returns>A new <see cref="OutcomeKind.Unavailable" /> <see cref="Error" />.</returns>
	public static Error Unavailable
	(
		string? description = null,
		Exception? exception = null,
		Error? cause = null,
		[CallerMemberName] string callerMember = "",
		[CallerFilePath] string callerFilePath = "",
		[CallerLineNumber] int callerLine = 0
	)
	{
		return new
		(
			OutcomeKind.Unavailable,
			description,
			exception,
			CallSite.From(callerMember, callerFilePath, callerLine),
			cause,
			details: null
		);
	}

	/// <summary>Creates an <see cref="OutcomeKind.Unexpected" /> <see cref="Error" />.</summary>
	/// <param name="exception">The exception.</param>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <param name="callerMember">The caller member.</param>
	/// <param name="callerFilePath">The caller file path.</param>
	/// <param name="callerLine">The caller line.</param>
	/// <returns>A new <see cref="OutcomeKind.Unexpected" /> <see cref="Error" />.</returns>
	public static Error Unexpected
	(
		Exception exception,
		string? description = null,
		Error? cause = null,
		[CallerMemberName] string callerMember = "",
		[CallerFilePath] string callerFilePath = "",
		[CallerLineNumber] int callerLine = 0
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(exception);

		return new
		(
			OutcomeKind.Unexpected,
			description,
			exception,
			CallSite.From(callerMember, callerFilePath, callerLine),
			cause,
			details: null
		);
	}

	/// <summary>Creates an <see cref="OutcomeKind.Unexpected" /> <see cref="Error" />.</summary>
	/// <param name="description">The description.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <param name="callerMember">The caller member.</param>
	/// <param name="callerFilePath">The caller file path.</param>
	/// <param name="callerLine">The caller line.</param>
	/// <returns>A new <see cref="OutcomeKind.Unexpected" /> <see cref="Error" />.</returns>
	public static Error Unexpected
	(
		string? description = null,
		Error? cause = null,
		[CallerMemberName] string callerMember = "",
		[CallerFilePath] string callerFilePath = "",
		[CallerLineNumber] int callerLine = 0
	)
	{
		return new
		(
			OutcomeKind.Unexpected,
			description,
			exception: null,
			CallSite.From(callerMember, callerFilePath, callerLine),
			cause,
			details: null
		);
	}

	/// <summary>Creates an <see cref="Error" /> of a custom <see cref="OutcomeKind" />.</summary>
	/// <param name="kind">The kind; its <see cref="OutcomeKind.Side" /> must allow the failure side.</param>
	/// <param name="description">The description.</param>
	/// <param name="exception">The exception.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <param name="callerMember">The caller member.</param>
	/// <param name="callerFilePath">The caller file path.</param>
	/// <param name="callerLine">The caller line.</param>
	/// <returns>A new <see cref="Error" /> of the provided <paramref name="kind" />.</returns>
	public static Error Custom
	(
		OutcomeKind kind,
		string? description = null,
		Exception? exception = null,
		Error? cause = null,
		[CallerMemberName] string callerMember = "",
		[CallerFilePath] string callerFilePath = "",
		[CallerLineNumber] int callerLine = 0
	)
	{
		return new
		(
			kind,
			description,
			exception,
			CallSite.From(callerMember, callerFilePath, callerLine),
			cause,
			details: null
		);
	}

	/// <summary>Creates an aggregate <see cref="Error" /> from the provided <paramref name="details" />.</summary>
	/// <param name="details">The child <see cref="Error" />s; must be non-empty.</param>
	/// <param name="description">The description; defaults to <c>"{N} error(s) occurred"</c>.</param>
	/// <param name="callerMember">The caller member.</param>
	/// <param name="callerFilePath">The caller file path.</param>
	/// <param name="callerLine">The caller line.</param>
	/// <returns>A new aggregate <see cref="Error" /> whose <see cref="Kind" /> is the most critical (highest code) among the <paramref name="details" />.</returns>
	public static Error Aggregate
	(
		IReadOnlyList<Error> details,
		string? description = null,
		[CallerMemberName] string callerMember = "",
		[CallerFilePath] string callerFilePath = "",
		[CallerLineNumber] int callerLine = 0
	)
	{
		ArgumentExceptionExtensions.ThrowIfNull(details);
		if (details.Count == 0) ArgumentExceptionExtensions.Throw("At least one detail is required", nameof(details));

		return new
		(
			details.MaxBy(static d => d.Kind)!.Kind,
			description ?? $"{details.Count} error(s) occurred",
			exception: null,
			CallSite.From(callerMember, callerFilePath, callerLine),
			cause: null,
			details
		);
	}

	/// <summary>Creates an <see cref="Error" /> from already-materialized parts — used by deserialization.</summary>
	/// <param name="kind">The kind.</param>
	/// <param name="description">The description.</param>
	/// <param name="site">The call site.</param>
	/// <param name="cause">The inner <see cref="Error" />.</param>
	/// <param name="details">The child <see cref="Error" />s.</param>
	/// <returns>A new <see cref="Error" />.</returns>
	internal static Error From
	(
		OutcomeKind kind,
		string? description,
		CallSite? site,
		Error? cause,
		IReadOnlyList<Error>? details
	)
	{
		return new
		(
			kind,
			description,
			exception: null,
			site,
			cause,
			details
		);
	}

	#endregion
}