using Rumrunner0.BackToReality.SharedExtensions.ValueObjects;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>State of <see cref="Suspicious{TResult}" />.</summary>
public sealed record class SuspiciousState : StringValueObject
{
	#region Instance State

	/// <inheritdoc cref="SuspiciousState" />
	private SuspiciousState(string value) : base(value) { }

	#endregion

	#region Static API

	/// <summary>Result.</summary>
	public static SuspiciousState Result { get; } = new ("result");

	/// <summary>Error.</summary>
	/// <remarks>
	/// This state indicates that a <see cref="Suspicious{TResult}" /> was created
	/// from an <see cref="ErrorCollection" /> and it contains actual <see cref="Error" />s.
	/// </remarks>
	public static SuspiciousState Error { get; } = new ("error");

	/// <summary>Empty error collection.</summary>
	/// <remarks>
	/// This state indicates that a <see cref="Suspicious{TResult}" /> was created
	/// from an <see cref="ErrorCollection" /> but it DOESN'T contain any <see cref="Error" />s.
	/// While this is technically possible, it usually indicates that errors were not added to the collection.
	/// </remarks>
	public static SuspiciousState EmptyErrorCollection { get; } = new ("empty_error_collection");

	/// <summary>Unexpected.</summary>
	public static SuspiciousState Unexpected { get; } = new ("unexpected");

	/// <summary>All error states.</summary>
	public static SuspiciousState[] ErrorStates { get; } = [Error, EmptyErrorCollection];

	#endregion
}