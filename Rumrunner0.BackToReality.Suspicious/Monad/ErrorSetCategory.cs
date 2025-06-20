using Rumrunner0.BackToReality.Suspicious.Extensions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>
/// Category of an <see cref="ErrorSet" />.
/// </summary>
public sealed record class ErrorSetCategory
{
	#region Instance State

	/// <summary>Name.</summary>
	private readonly string _name;

	/// <inheritdoc cref="ErrorSetCategory" />
	private ErrorSetCategory(string name) => this._name = name;

	#endregion

	#region Instance API

	/// <summary>Creates a string that represents this instance.</summary>
	/// <returns>A string that represents this instance.</returns>
	public override string ToString() => this._name;

	#endregion

	#region Static API

	/// <summary>Factory for a custom <see cref="ErrorSetCategory" />.</summary>
	public static ErrorSetCategory Custom(string name)
	{
		ArgumentExceptionHelper.ThrowIfNullOrEmptyOrWhiteSpace(name);
		return new (name);
	}

	/// <summary>Unspecified <see cref="ErrorSetCategory" />.</summary>
	public static ErrorSetCategory Unspecified { get; } = new (name: "unspecified");

	#endregion
}