using System;
using System.Collections.Generic;
using System.Linq;
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
	public bool IsGreaterThan(ErrorKind other) => this._priority > other._priority;

	/// <summary>Determines whether this instance is greater than or equals to the <paramref name="other" />.</summary>
	/// <param name="other">The other.</param>
	/// <returns><c>true</c> if the condition is satisfied; <c>false</c> otherwise.</returns>
	public bool IsGreaterThanOrEqualsTo(ErrorKind other) => this._priority >= other._priority;

	/// <summary>Determines whether this instance is less than the <paramref name="other" />.</summary>
	/// <param name="other">The other.</param>
	/// <returns><c>true</c> if the condition is satisfied; <c>false</c> otherwise.</returns>
	public bool IsLessThan(ErrorKind other) => this._priority < other._priority;

	/// <summary>Determines whether this instance is less than or equals to the <paramref name="other" />.</summary>
	/// <param name="other">The other.</param>
	/// <returns><c>true</c> if the condition is satisfied; <c>false</c> otherwise.</returns>
	public bool IsLessThanOrEqualsTo(ErrorKind other) => this._priority <= other._priority;

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
	public static ErrorKind NoResult { get; } = new (name: "no-result", priority: 10_000);

	/// <summary>Failure <see cref="ErrorKind" />.</summary>
	public static ErrorKind Failure { get; } = new (name: "failure", priority: 100_000);

	/// <summary>Unexpected <see cref="ErrorKind" />.</summary>
	public static ErrorKind Unexpected { get; } = new (name: "unexpected", priority: int.MaxValue - 1);

	/// <summary>Unspecified <see cref="ErrorKind" />.</summary>
	public static ErrorKind Unspecified { get; } = new (name: "unspecified", priority: int.MaxValue);

	/// <summary>All predefined <see cref="ErrorKind" />s.</summary>
	private static readonly ErrorKind[] _allPredefined = [NoResult, Failure, Unexpected, Unspecified];



	/// <summary>Factory for a custom <see cref="ErrorKind" />.</summary>
	public static ErrorKind Custom(string name, int priority)
	{
		if (_allPredefined.FirstOrDefault(k => k._name == name || k._priority == priority) is { } conflict)
		{
			ArgumentExceptionExtensions.Throw($"Custom properties ({name}, {priority}) conflict with: {conflict}");
		}

		ArgumentExceptionExtensions.ThrowIfNullOrEmptyOrWhiteSpace(name);
		return new (name, priority);
	}



	/// <summary>Retrieves an <see cref="ErrorKind" /> with the highest priority.</summary>
	/// <param name="kinds">The <see cref="ErrorKind" />s.</param>
	/// <returns>The <see cref="ErrorKind" /> with the highest priority.</returns>
	internal static ErrorKind? GetWithHighestPriority(params IEnumerable<ErrorKind?> kinds)
	{
		return kinds.Max(_priorityComparer);
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