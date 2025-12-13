using Rumrunner0.BackToReality.SharedExtensions.ValueObjects;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Category of <see cref="ErrorCollection" />.</summary>
public sealed record class ErrorCollectionCategory : StringValueObject
{
	#region Instance State

	/// <inheritdoc cref="ErrorCollectionCategory" />
	private ErrorCollectionCategory(string value) : base(value) { }

	#endregion

	#region Static API

	/// <summary>Unspecified.</summary>
	public static ErrorCollectionCategory Unspecified { get; } = new ("unspecified");

	/// <summary>Custom.</summary>
	public static ErrorCollectionCategory Custom(string value) => new (value);

	/// <inheritdoc cref="Custom" />
	public static implicit operator ErrorCollectionCategory(string value) => Custom(value);

	#endregion
}