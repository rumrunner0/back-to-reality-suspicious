using System;
using System.Collections.Generic;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Kind of <see cref="Error" />.</summary>
public sealed record class ErrorKind : IComparable<ErrorKind>
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

	/// <summary>Name.</summary>
	public string Name => this._name;

	/// <summary>Priority.</summary>
	public int Priority => this._priority;

	/// <summary>Determines whether this instance is greater than the <paramref name="other" />.</summary>
	/// <param name="other">The other.</param>
	/// <returns><c>true</c> if the condition is satisfied; <c>false</c> otherwise.</returns>
	public bool IsGreaterThan(ErrorKind other) => _priorityComparer.Compare(this, other) > 0;

	/// <summary>Determines whether this instance is greater than or equals to the <paramref name="other" />.</summary>
	/// <param name="other">The other.</param>
	/// <returns><c>true</c> if the condition is satisfied; <c>false</c> otherwise.</returns>
	public bool IsGreaterThanOrEqualsTo(ErrorKind other) => _priorityComparer.Compare(this, other) >= 0;

	/// <summary>Determines whether this instance is less than the <paramref name="other" />.</summary>
	/// <param name="other">The other.</param>
	/// <returns><c>true</c> if the condition is satisfied; <c>false</c> otherwise.</returns>
	public bool IsLessThan(ErrorKind other) => _priorityComparer.Compare(this, other) < 0;

	/// <summary>Determines whether this instance is less than or equals to the <paramref name="other" />.</summary>
	/// <param name="other">The other.</param>
	/// <returns><c>true</c> if the condition is satisfied; <c>false</c> otherwise.</returns>
	public bool IsLessThanOrEqualsTo(ErrorKind other) => _priorityComparer.Compare(this, other) <= 0;

	/// <inheritdoc />
	public int CompareTo(ErrorKind? other) => _priorityComparer.Compare(this, other);

	#endregion

	#region Instance Utilities

	/// <summary>Creates a string that represents this instance in redacted mode.</summary>
	/// <returns>A string that represents this instance in redacted mode.</returns>
	public override string ToString() => $"{this._name} ({this._priority})";

	/// <summary>Creates a string that represents this instance in redacted mode.</summary>
	/// <returns>A string that represents this instance in redacted mode.</returns>
	public string ToStringRedacted() => this._name;

	#endregion

	#region Static State

	/// <inheritdoc cref="PriorityComparer" />
	private static readonly PriorityComparer _priorityComparer = new ();

	/// <inheritdoc />
	private sealed class PriorityComparer : IComparer<ErrorKind?>
	{
		/// <inheritdoc />
		public int Compare(ErrorKind? x, ErrorKind? y)
		{
			if (x is null) return -1;
			if (y is null) return 1;
			return x._priority.CompareTo(y._priority);
		}
	}

	#endregion

	#region Static API

	/// <summary>Failure <see cref="ErrorKind" />.</summary>
	public static ErrorKind NoValue { get; } = new ("no_value", priority: 0);

	/// <summary>Failure <see cref="ErrorKind" />.</summary>
	public static ErrorKind Failure { get; } = new ("failure", priority: 1);

	/// <summary>Unexpected <see cref="ErrorKind" />.</summary>
	public static ErrorKind Unexpected { get; } = new ("unexpected", priority: int.MaxValue - 1);

	/// <summary>Unspecified <see cref="ErrorKind" />.</summary>
	public static ErrorKind Unspecified { get; } = new ("unspecified", priority: int.MaxValue);

	/// <summary>Factory for a custom <see cref="ErrorKind" />.</summary>
	public static ErrorKind Custom(string name, int priority)
	{
		ArgumentExceptionExtensions.ThrowIfNullOrEmptyOrWhiteSpace(name);
		return new (name, priority);
	}



	/// <summary>Determines whether <paramref name="left" /> has higher priority than <paramref name="right" />.</summary>
	/// <param name="left">The first <see cref="ErrorKind" /> to compare.</param>
	/// <param name="right">The second <see cref="ErrorKind" /> to compare.</param>
	/// <returns><c>true</c> if <paramref name="left" /> has higher priority; <c>false</c> otherwise.</returns>
	public static bool operator >(ErrorKind left, ErrorKind right) => left.IsGreaterThan(right);

	/// <summary>Determines whether <paramref name="left" /> has higher or equal priority than <paramref name="right" />.</summary>
	/// <param name="left">The first <see cref="ErrorKind" /> to compare.</param>
	/// <param name="right">The second <see cref="ErrorKind" /> to compare.</param>
	/// <returns><c>true</c> if <paramref name="left" /> has higher or equal priority; <c>false</c> otherwise.</returns>
	public static bool operator >=(ErrorKind left, ErrorKind right) => left.IsGreaterThanOrEqualsTo(right);

	/// <summary>Determines whether <paramref name="left" /> has lower priority than <paramref name="right"/>.</summary>
	/// <param name="left">The first <see cref="ErrorKind" /> to compare.</param>
	/// <param name="right">The second <see cref="ErrorKind" /> to compare.</param>
	/// <returns><c>true</c> if <paramref name="left" /> has lower priority; <c>false</c> otherwise.</returns>
	public static bool operator <(ErrorKind left, ErrorKind right) => left.IsLessThan(right);

	/// <summary>Determines whether <paramref name="left" /> has lower or equal priority than <paramref name="right" />.</summary>
	/// <param name="left">The first <see cref="ErrorKind" /> to compare.</param>
	/// <param name="right">The second <see cref="ErrorKind" /> to compare.</param>
	/// <returns><c>true</c> if <paramref name="left" /> has lower or equal priority; <c>false</c> otherwise.</returns>
	public static bool operator <=(ErrorKind left, ErrorKind right) => left.IsLessThanOrEqualsTo(right);

	#endregion
}