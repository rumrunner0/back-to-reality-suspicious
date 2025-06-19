using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>
/// A kind of <see cref="Error" />.
/// </summary>
public sealed record class ErrorKind
{
	#region Instance State

	/// <summary>Value.</summary>
	private readonly string _value;

	/// <summary>Priority.</summary>
	private readonly int _priority;

	/// <inheritdoc cref="ErrorKind" />
	private ErrorKind(string value, int priority)
	{
		this._value = value;
		this._priority = priority;
	}

	#endregion

	#region Instance API

	/// <summary>
	/// Determines if the source is greater than the other.
	/// </summary>
	/// <param name="other">The other.</param>
	/// <returns><c>true</c>, if the source satisfies the condition, <c>false</c>, otherwise.</returns>
	public bool IsGreaterThan(ErrorKind other) => this._priority > other._priority;

	/// <summary>
	/// Determines if the source is greater than or equals to the other.
	/// </summary>
	/// <param name="other">The other.</param>
	/// <returns><c>true</c>, if the source satisfies the condition, <c>false</c>, otherwise.</returns>
	public bool IsGreaterThanOrEqualsTo(ErrorKind other) => this._priority >= other._priority;

	/// <summary>
	/// Determines if the source is less than the other.
	/// </summary>
	/// <param name="other">The other.</param>
	/// <returns><c>true</c>, if the source satisfies the condition, <c>false</c>, otherwise.</returns>
	public bool IsLessThan(ErrorKind other) => this._priority < other._priority;

	/// <summary>
	/// Determines if the source is less than or equals to the other.
	/// </summary>
	/// <param name="other">The other.</param>
	/// <returns><c>true</c>, if the source satisfies the condition, <c>false</c>, otherwise.</returns>
	public bool IsLessThanOrEqualsTo(ErrorKind other) => this._priority <= other._priority;

	#endregion

	#region Instance Utilities

	/// <summary>Prints members.</summary>
	/// <param name="builder">The <see cref="StringBuilder" />.</param>
	/// <returns><c>true</c>, if members should be printed, <c>false</c>, otherwise.</returns>
	private bool PrintMembers(StringBuilder builder)
	{
		builder.Append($"Value = {this._value}");
		builder.Append($", Priority = {this._priority}");
		return true;
	}

	/// <summary>Prints members in redacted mode.</summary>
	/// <param name="builder">The <see cref="StringBuilder" />.</param>
	/// <returns><c>true</c>, if members should be printed, <c>false</c>, otherwise.</returns>
	private bool PrintMembersRedacted(StringBuilder builder)
	{
		builder.Append($"Value = {this._value}");
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

	#region Static State

	/// <inheritdoc cref="PriorityComparer" />
	private static readonly PriorityComparer _priorityComparer;

	/// <inheritdoc cref="ErrorKind" />
	static ErrorKind() => ErrorKind._priorityComparer = new ();

	/// <inheritdoc />
	private sealed class PriorityComparer : IComparer<ErrorKind>
	{
		/// <inheritdoc />
		public int Compare(ErrorKind? x, ErrorKind? y) => x is null ? -1 : y is null ? 1 : x._priority.CompareTo(y._priority);
	}

	#endregion

	#region Static API

	/// <summary>Failure <see cref="ErrorKind" />.</summary>
	public static ErrorKind Failure { get; } = new (value: "failure", priority: 0);

	/// <summary>Unexpected <see cref="ErrorKind" />.</summary>
	public static ErrorKind Unexpected { get; } = new (value: "unexpected", priority: int.MaxValue - 1);

	/// <summary>Unspecified <see cref="ErrorKind" />.</summary>
	public static ErrorKind Unspecified { get; } = new (value: "unspecified", priority: int.MaxValue);

	/// <inheritdoc cref="PriorityComparer" />
	public static ErrorKind WithHighestPriority(IEnumerable<ErrorKind> kinds) => kinds.MaxBy(k => k, ErrorKind._priorityComparer)!;

	#endregion
}