namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>
/// State of <see cref="Suspicious{TResult}" />.
/// </summary>
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
	/// This state indicates that the <see cref="Suspicious{TResult}" /> was created
	/// from an <see cref="ErrorSet" /> and it contains actual <see cref="Error" />s.
	/// </remarks>
	public static SuspiciousState Error { get; } = new (value: "error");

	/// <summary>Empty error set <see cref="SuspiciousState" />.</summary>
	/// <remarks>
	/// This state indicates that the <see cref="Suspicious{TResult}" /> was created
	/// from an <see cref="ErrorSet" /> but it DOESN'T contain any <see cref="Error" />s.
	/// While this is technically possible, it usually means that errors were not just added to the set.
	/// </remarks>
	public static SuspiciousState EmptyErrorSet { get; } = new (value: "empty-error-set");

	/// <summary>Unexpected <see cref="SuspiciousState" />.</summary>
	public static SuspiciousState Unexpected { get; } = new (value: "unexpected");

	/// <summary>Collection of error states.</summary>
	public static SuspiciousState[] ErrorStates { get; } =
	[
		SuspiciousState.Error,
		SuspiciousState.EmptyErrorSet
	];

	#endregion
}