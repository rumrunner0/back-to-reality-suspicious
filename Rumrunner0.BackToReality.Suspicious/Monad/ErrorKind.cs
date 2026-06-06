using System;
using System.Collections.Generic;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Kind of <see cref="Error" />.</summary>
public sealed record class ErrorKind : IEquatable<ErrorKind>, IComparable<ErrorKind>
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

	/// <summary>Gets the priority adjusted by the specified <paramref name="offset" />.</summary>
	/// <param name="offset">The offset applied to the current priority.</param>
	/// <returns>The adjusted priority.</returns>
	public int GetAdjustedPriority(int offset) => this._priority + offset;

	#endregion

	#region Display

	/// <summary>Creates a string that represents this instance.</summary>
	/// <returns>A string that represents this instance.</returns>
	public override string ToString() => $"{this._name} ({this._priority})";

	#endregion

	#region Equality

	/// <inheritdoc />
	/// <remarks>Indicates whether the <see cref="Name" /> and <see cref="Priority" /> of the current instance is equal to the <see cref="Name" /> and <see cref="Priority" /> of another instance.</remarks>
	public bool Equals(ErrorKind? other)
	{
		if (object.ReferenceEquals(this, other)) return true;

		return
			other is not null &&
			this.EqualityContract == other.EqualityContract &&
			this._name == other._name &&
			this._priority == other._priority;
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return HashCode.Combine(this.EqualityContract, this._name, this._priority);
	}

	#endregion

	#region Comparison

	/// <inheritdoc />
	/// <remarks>Compares the priorities of the current instance and another instance.</remarks>
	public int CompareTo(ErrorKind? other) => _priorityComparer.Compare(this, other);

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

	#region Creation

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

	#endregion
}