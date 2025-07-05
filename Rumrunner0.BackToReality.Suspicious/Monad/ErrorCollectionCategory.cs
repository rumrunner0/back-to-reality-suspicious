using Rumrunner0.BackToReality.Suspicious.Extensions;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Category of an <see cref="ErrorCollection" />.</summary>
public sealed record class ErrorCollectionCategory
{
	#region Instance State

	/// <summary>Name.</summary>
	private readonly string _name;

	/// <inheritdoc cref="ErrorCollectionCategory" />
	private ErrorCollectionCategory(string name) => this._name = name;

	#endregion

	#region Instance API

	/// <summary>Name.</summary>
	public string Name => this._name;

	/// <summary>Creates a string that represents this instance.</summary>
	/// <returns>A string that represents this instance.</returns>
	public override string ToString() => this._name;

	#endregion

	#region Static API

	/// <summary>Unspecified <see cref="ErrorCollectionCategory" />.</summary>
	public static ErrorCollectionCategory Unspecified { get; } = new (name: "unspecified");

	/// <summary>Factory for a custom <see cref="ErrorCollectionCategory" />.</summary>
	public static ErrorCollectionCategory Custom(string name)
	{
		ArgumentExceptionHelper.ThrowIfNullOrEmptyOrWhiteSpace(name);
		return new (name);
	}

	#endregion
}