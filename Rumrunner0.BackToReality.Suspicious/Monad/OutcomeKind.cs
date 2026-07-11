using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;
using Rumrunner0.BackToReality.Suspicious.Serialization;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Kind of an outcome — the domain identity of a result (e.g. ok, no_value, invalid).</summary>
/// <remarks>The <see cref="Code" /> orders kinds by severity; aggregate escalation picks the highest code.</remarks>
[JsonConverter(typeof(OutcomeKindJsonConverter))]
public sealed record class OutcomeKind : IEquatable<OutcomeKind>, IComparable<OutcomeKind>
{
	#region Instance State

	/// <summary>Name.</summary>
	private readonly string _name;

	/// <summary>Code.</summary>
	private readonly int _code;

	/// <summary>Side.</summary>
	private readonly OutcomeSide _side;

	/// <inheritdoc cref="OutcomeKind" />
	private OutcomeKind(string name, int code, OutcomeSide side)
	{
		ArgumentExceptionExtensions.ThrowIfNullOrEmptyOrWhiteSpace(name);
		ArgumentExceptionExtensions.ThrowIfNull(side);
		if (code is < _minCode or >= _maxCode) ArgumentExceptionExtensions.Throw($"Code must be in [{_minCode}, {_maxCode})", nameof(code));

		this._name = name;
		this._code = code;
		this._side = side;
	}

	#endregion

	#region Instance API

	/// <summary>Name.</summary>
	public string Name => this._name;

	/// <summary>Code that identifies this <see cref="OutcomeKind" /> and orders it by severity.</summary>
	public int Code => this._code;

	/// <summary>Side that declares on which rails this <see cref="OutcomeKind" /> can be constructed.</summary>
	public OutcomeSide Side => this._side;

	#endregion

	#region Display

	/// <summary>Creates a string that represents this instance.</summary>
	/// <returns>A string that represents this instance.</returns>
	public override string ToString() => $"{this._name} ({this._code})";

	#endregion

	#region Equality

	/// <inheritdoc />
	/// <remarks>Indicates whether the <see cref="Name" />, <see cref="Code" /> and <see cref="Side" /> of the current instance are equal to those of another instance.</remarks>
	public bool Equals(OutcomeKind? other)
	{
		if (object.ReferenceEquals(this, other)) return true;

		return
			other is not null &&
			this.EqualityContract == other.EqualityContract &&
			this._name == other._name &&
			this._code == other._code &&
			this._side == other._side;
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return HashCode.Combine(this.EqualityContract, this._name, this._code, this._side);
	}

	#endregion

	#region Comparison

	/// <inheritdoc />
	/// <remarks>Compares the codes of the current instance and another instance.</remarks>
	public int CompareTo(OutcomeKind? other) => _codeComparer.Compare(this, other);

	/// <summary>Determines whether <paramref name="left" /> has a higher code than <paramref name="right" />.</summary>
	/// <param name="left">The first <see cref="OutcomeKind" /> to compare.</param>
	/// <param name="right">The second <see cref="OutcomeKind" /> to compare.</param>
	/// <returns><c>true</c> if <paramref name="left" /> has a higher code; <c>false</c> otherwise.</returns>
	public static bool operator >(OutcomeKind left, OutcomeKind right) => _codeComparer.Compare(left, right) > 0;

	/// <summary>Determines whether <paramref name="left" /> has a higher or equal code than <paramref name="right" />.</summary>
	/// <param name="left">The first <see cref="OutcomeKind" /> to compare.</param>
	/// <param name="right">The second <see cref="OutcomeKind" /> to compare.</param>
	/// <returns><c>true</c> if <paramref name="left" /> has a higher or equal code; <c>false</c> otherwise.</returns>
	public static bool operator >=(OutcomeKind left, OutcomeKind right) => _codeComparer.Compare(left, right) >= 0;

	/// <summary>Determines whether <paramref name="left" /> has a lower code than <paramref name="right" />.</summary>
	/// <param name="left">The first <see cref="OutcomeKind" /> to compare.</param>
	/// <param name="right">The second <see cref="OutcomeKind" /> to compare.</param>
	/// <returns><c>true</c> if <paramref name="left" /> has a lower code; <c>false</c> otherwise.</returns>
	public static bool operator <(OutcomeKind left, OutcomeKind right) => _codeComparer.Compare(left, right) < 0;

	/// <summary>Determines whether <paramref name="left" /> has a lower or equal code than <paramref name="right" />.</summary>
	/// <param name="left">The first <see cref="OutcomeKind" /> to compare.</param>
	/// <param name="right">The second <see cref="OutcomeKind" /> to compare.</param>
	/// <returns><c>true</c> if <paramref name="left" /> has a lower or equal code; <c>false</c> otherwise.</returns>
	public static bool operator <=(OutcomeKind left, OutcomeKind right) => _codeComparer.Compare(left, right) <= 0;

	/// <inheritdoc cref="CodeComparer" />
	private static readonly CodeComparer _codeComparer = new ();

	/// <inheritdoc />
	private sealed class CodeComparer : IComparer<OutcomeKind?>
	{
		/// <inheritdoc />
		public int Compare(OutcomeKind? x, OutcomeKind? y)
		{
			if (x is null) return -1;
			if (y is null) return 1;
			return x._code.CompareTo(y._code);
		}
	}

	#endregion

	#region Static State

	/// <summary>Minimum code (inclusive).</summary>
	private const int _minCode = 0;

	/// <summary>Maximum code (exclusive).</summary>
	private const int _maxCode = 2000;

	/// <summary>Minimum custom code of the lower range (inclusive).</summary>
	private const int _customLowerMinCode = 100;

	/// <summary>Maximum custom code of the lower range (exclusive).</summary>
	private const int _customLowerMaxCode = 900;

	/// <summary>Minimum custom code of the upper range (inclusive).</summary>
	private const int _customUpperMinCode = 1100;

	/// <summary>Maximum custom code of the upper range (exclusive).</summary>
	private const int _customUpperMaxCode = 1900;

	#endregion

	#region Creation

	/// <summary>Ok <see cref="OutcomeKind" />.</summary>
	/// <remarks>The operation succeeded.</remarks>
	public static OutcomeKind Ok { get; } = new ("ok", code: 0, OutcomeSide.Success);

	/// <summary>No-value <see cref="OutcomeKind" />.</summary>
	/// <remarks>Nothing to return (e.g. a repository miss). Can ride either rail: a success without a value, or an <see cref="Error" /> if the producer treats the absence as a failure.</remarks>
	public static OutcomeKind NoValue { get; } = new ("no_value", code: 10, OutcomeSide.Any);

	/// <summary>Invalid <see cref="OutcomeKind" />.</summary>
	/// <remarks>Input or state was rejected by domain rules (validation).</remarks>
	public static OutcomeKind Invalid { get; } = new ("invalid", code: 1000, OutcomeSide.Failure);

	/// <summary>Conflict <see cref="OutcomeKind" />.</summary>
	/// <remarks>State collision (e.g. concurrency, already-exists).</remarks>
	public static OutcomeKind Conflict { get; } = new ("conflict", code: 1010, OutcomeSide.Failure);

	/// <summary>Failure <see cref="OutcomeKind" />.</summary>
	/// <remarks>General expected failure — the fallback when no more specific kind fits; prefer a specific kind where one exists.</remarks>
	public static OutcomeKind Failure { get; } = new ("failure", code: 1020, OutcomeSide.Failure);

	/// <summary>Unavailable <see cref="OutcomeKind" />.</summary>
	/// <remarks>A dependency is down, timed out or otherwise unreachable.</remarks>
	public static OutcomeKind Unavailable { get; } = new ("unavailable", code: 1030, OutcomeSide.Failure);

	/// <summary>Unexpected <see cref="OutcomeKind" />.</summary>
	/// <remarks>Unhandled or exceptional failure — the severity cap; no custom kind can outrank it.</remarks>
	public static OutcomeKind Unexpected { get; } = new ("unexpected", code: 1999, OutcomeSide.Failure);

	/// <summary>Factory for a custom <see cref="OutcomeKind" />.</summary>
	/// <param name="name">The name.</param>
	/// <param name="code">The code; must be in [100, 900) or [1100, 1900).</param>
	/// <param name="side">The side.</param>
	/// <returns>A new custom <see cref="OutcomeKind" />.</returns>
	/// <exception cref="ArgumentException">Thrown if the <paramref name="code" /> is outside the custom ranges.</exception>
	public static OutcomeKind Custom(string name, int code, OutcomeSide side)
	{
		if (code is not ((>= _customLowerMinCode and < _customLowerMaxCode) or (>= _customUpperMinCode and < _customUpperMaxCode)))
		{
			ArgumentExceptionExtensions.Throw($"Code must be in [{_customLowerMinCode}, {_customLowerMaxCode}) or [{_customUpperMinCode}, {_customUpperMaxCode}) for custom kinds", nameof(code));
		}

		return new (name, code, side);
	}

	/// <summary>Preset <see cref="OutcomeKind" />s — used by deserialization to return singleton instances.</summary>
	internal static IReadOnlyList<OutcomeKind> Presets { get; } = [Ok, NoValue, Invalid, Conflict, Failure, Unavailable, Unexpected];

	#endregion
}