namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>State of a <see cref="Suspicious{TResult}" />.</summary>
public sealed record class SuspiciousState
{
	#region Instance State

	/// <summary>Value.</summary>
	private readonly string _value;

	/// <inheritdoc cref="SuspiciousState" />
	private SuspiciousState(string value) => this._value = value;

	#endregion

	#region Instance Utilities

	/// <summary>Creates a string that represents this instance.</summary>
	/// <returns>A string that represents this instance.</returns>
	public override string ToString() => this._value;

	#endregion

	#region Static API

	/// <summary>Result <see cref="SuspiciousState" />.</summary>
	public static SuspiciousState Result { get; } = new (value: "result");

	/// <summary>Error <see cref="SuspiciousState" />.</summary>
	/// <remarks>
	/// This state indicates that a <see cref="Suspicious{TResult}" /> was created
	/// from an <see cref="ErrorCollection" /> and it contains actual <see cref="Error" />s.
	/// </remarks>
	public static SuspiciousState Error { get; } = new (value: "error");

	/// <summary>Empty error collection <see cref="SuspiciousState" />.</summary>
	/// <remarks>
	/// This state indicates that a <see cref="Suspicious{TResult}" /> was created
	/// from an <see cref="ErrorCollection" /> but it DOESN'T contain any <see cref="Error" />s.
	/// While this is technically possible, it usually indicates that errors were not added to the collection.
	/// </remarks>
	public static SuspiciousState EmptyErrorCollection { get; } = new (value: "empty-error-collection");

	/// <summary>Unexpected <see cref="SuspiciousState" />.</summary>
	public static SuspiciousState Unexpected { get; } = new (value: "unexpected");

	/// <summary>Collection of error states.</summary>
	public static SuspiciousState[] ErrorStates { get; } =
	[
		SuspiciousState.Error,
		SuspiciousState.EmptyErrorCollection
	];

	#endregion
}