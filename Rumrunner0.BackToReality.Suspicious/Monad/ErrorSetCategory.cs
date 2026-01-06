using Rumrunner0.BackToReality.SharedExtensions.Exceptions;
using Rumrunner0.BackToReality.SharedExtensions.ValueObjects;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Category of <see cref="ErrorSet" />.</summary>
public readonly record struct ErrorSetCategory : IValueObject<ErrorSetCategory, string>
{
	/// <inheritdoc cref="ErrorSetCategory" />
	public ErrorSetCategory()
	{
		this.Value = "Unspecified";
	}

	/// <inheritdoc cref="ErrorSetCategory" />
	public ErrorSetCategory(string value)
	{
		ArgumentExceptionExtensions.ThrowIfNullOrEmptyOrWhiteSpace(value);
		this.Value = value;
	}

	/// <inheritdoc />
	public string Value { get; }

	/// <inheritdoc />
	public static ErrorSetCategory From(string value) => new (value);

	/// <summary>Implicitly converts <see cref="ErrorSetCategory" /> to <see cref="string" />.</summary>
	/// <param name="source">The source.</param>
	/// <returns>The value.</returns>
	public static implicit operator string(ErrorSetCategory source) => source.Value;

	/// <summary>Explicitly converts <see cref="string" /> to <see cref="ErrorSetCategory" />.</summary>
	/// <param name="source">The source.</param>
	/// <returns>The <see cref="ErrorSetCategory" />.</returns>
	public static explicit operator ErrorSetCategory(string source) => From(source);
}