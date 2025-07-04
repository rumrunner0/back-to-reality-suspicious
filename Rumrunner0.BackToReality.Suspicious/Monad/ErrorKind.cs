using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rumrunner0.BackToReality.Suspicious.Extensions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>
/// Kind of an <see cref="Error" />.
/// </summary>
public sealed record class ErrorKind
{
	#region Instance State

	/// <summary>Name.</summary>
	private readonly string _name;

	/// <summary>Priority.</summary>
	private readonly int _priority;

	/// <inheritdoc cref="ErrorKind" />
	private ErrorKind(string name, int priority)
	{
		this._name = name;
		this._priority = priority;
	}

	#endregion

	#region Instance API

	/// <summary>
	/// Determines if this instance is greater than the <paramref name="other" />.
	/// </summary>
	/// <param name="other">The other.</param>
	/// <returns><c>true</c>, if the condition is satisfied, <c>false</c>, otherwise.</returns>
	public bool IsGreaterThan(ErrorKind other) => this._priority > other._priority;

	/// <summary>
	/// Determines if this instance is greater than or equals to the <paramref name="other" />.
	/// </summary>
	/// <param name="other">The other.</param>
	/// <returns><c>true</c>, if the condition is satisfied, <c>false</c>, otherwise.</returns>
	public bool IsGreaterThanOrEqualsTo(ErrorKind other) => this._priority >= other._priority;

	/// <summary>
	/// Determines if this instance is less than the <paramref name="other" />.
	/// </summary>
	/// <param name="other">The other.</param>
	/// <returns><c>true</c>, if the condition is satisfied, <c>false</c>, otherwise.</returns>
	public bool IsLessThan(ErrorKind other) => this._priority < other._priority;

	/// <summary>
	/// Determines if this instance is less than or equals to the <paramref name="other" />.
	/// </summary>
	/// <param name="other">The other.</param>
	/// <returns><c>true</c>, if the condition is satisfied, <c>false</c>, otherwise.</returns>
	public bool IsLessThanOrEqualsTo(ErrorKind other) => this._priority <= other._priority;

	#endregion

	#region Instance Utilities

	/// <summary>Prints members.</summary>
	/// <param name="builder">The <see cref="StringBuilder" />.</param>
	/// <returns><c>true</c>, if members should be printed, <c>false</c>, otherwise.</returns>
	private bool PrintMembers(StringBuilder builder)
	{
		builder.Append(this._name);
		builder.Append($", Priority = {this._priority}");
		return true;
	}

	/// <summary>Creates a string that represents this instance in redacted mode.</summary>
	/// <returns>A string that represents this instance in redacted mode.</returns>
	public override string ToString() => $"{this._name} ({this._priority})";

	/// <summary>Creates a string that represents this instance in redacted mode.</summary>
	/// <returns>A string that represents this instance in redacted mode.</returns>
	public string ToStringRedacted() => this._name;

	#endregion

	#region Static State

	/// <inheritdoc cref="PriorityComparer" />
	private static readonly PriorityComparer _priorityComparer;

	/// <inheritdoc cref="ErrorKind" />
	static ErrorKind()
	{
		ErrorKind._priorityComparer = new ();
	}

	/// <inheritdoc />
	private sealed class PriorityComparer : IComparer<ErrorKind>
	{
		/// <inheritdoc />
		public int Compare(ErrorKind? x, ErrorKind? y) => x is null ? -1 : y is null ? 1 : x._priority.CompareTo(y._priority);
	}

	#endregion

	#region Static API

	/// <summary>Failure <see cref="ErrorKind" />.</summary>
	public static ErrorKind Failure { get; } = new (name: "failure", priority: 0);

	/// <summary>Unexpected <see cref="ErrorKind" />.</summary>
	public static ErrorKind Unexpected { get; } = new (name: "unexpected", priority: int.MaxValue - 1);

	/// <summary>Unspecified <see cref="ErrorKind" />.</summary>
	public static ErrorKind Unspecified { get; } = new (name: "unspecified", priority: int.MaxValue);

	/// <summary>Factory for a custom <see cref="ErrorKind" />.</summary>
	public static ErrorKind Custom(string name, int priority)
	{
		// TODO: Add check for reserved priorities like int.MaxValue and int.MaxValue - 1.
		// I think, 0 should remain available.

		ArgumentExceptionHelper.ThrowIfNullOrEmptyOrWhiteSpace(name);
		return new (name, priority);
	}

	/// <summary>
	/// Finds the <see cref="ErrorKind" /> with the highest priority.
	/// </summary>
	/// <param name="kinds">The <see cref="ErrorKind" />s.</param>
	/// <returns>The <see cref="ErrorKind" /> with the highest priority.</returns>
	public static ErrorKind FindWithHighestPriority(IEnumerable<ErrorKind> kinds)
	{
		return kinds.MaxBy(k => k, ErrorKind._priorityComparer)!;
	}

	#endregion
}