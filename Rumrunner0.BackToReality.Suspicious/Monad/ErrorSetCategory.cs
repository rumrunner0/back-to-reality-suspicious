namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>
/// A category of an <see cref="ErrorSet" />.
/// </summary>
public sealed record class ErrorSetCategory
{
	#region Instance State

	/// <summary>Value.</summary>
	private readonly string _value;

	/// <inheritdoc cref="ErrorSetCategory" />
	private ErrorSetCategory(string value) => this._value = value;

	#endregion

	#region Instance API

	/// <summary>Creates a string that represents this instance.</summary>
	/// <returns>A string that represents this instance.</returns>
	public override string ToString() => this._value;

	#endregion

	#region Static API

	/// <summary>Factory for custom <see cref="ErrorSetCategory" />.</summary>
	public static ErrorSetCategory Custom(string value) => new (value);

	/// <summary>Unspecified <see cref="ErrorSetCategory" />.</summary>
	public static ErrorSetCategory Unspecified { get; } = new (value: "unspecified");

	#endregion
}