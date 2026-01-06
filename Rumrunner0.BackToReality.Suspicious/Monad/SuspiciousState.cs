using Rumrunner0.BackToReality.SharedExtensions.Enums;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>State of <see cref="Suspicious{TValue}" />.</summary>
public sealed record class SuspiciousState : StringEnumeration<SuspiciousState>
{
	#region Instance State

	/// <inheritdoc cref="SuspiciousState" />
	private SuspiciousState(string value) : base(value) { }

	#endregion

	#region Static API

	/// <summary>Value.</summary>
	public new static SuspiciousState Value { get; } = new ("value");

	/// <summary>Error.</summary>
	public static SuspiciousState Error { get; } = new ("error");

	#endregion
}