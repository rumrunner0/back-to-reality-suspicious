using Rumrunner0.BackToReality.SharedExtensions.ValueObjects;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Category of <see cref="ErrorSet" />.</summary>
public sealed record class ErrorSetCategory : StringValueObject
{
	#region Instance State

	/// <inheritdoc cref="ErrorSetCategory" />
	private ErrorSetCategory(string value) : base(value) { }

	#endregion

	#region Static API

	/// <summary>Custom.</summary>
	public static ErrorSetCategory Custom(string value) => new (value);

	/// <inheritdoc cref="Custom" />
	public static implicit operator ErrorSetCategory(string value) => Custom(value);

	#endregion
}