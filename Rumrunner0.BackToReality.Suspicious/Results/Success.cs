using Rumrunner0.BackToReality.Suspicious.Monad;

namespace Rumrunner0.BackToReality.Suspicious.Results;

/// <summary>Success result for <see cref="Suspicious{TValue}" />.</summary>
/// <remarks>
/// * It should be used to indicate successful completion of an operation.<br />
/// * Access this result using <see cref="Factories.Suspicious.Success" />.<br /><br />
/// * If you want to indicate that something is ok, up, has no errors or has required value, use <see cref="Ok" />.
/// </remarks>
public sealed record class Success
{
	/// <inheritdoc cref="Success" />
	internal Success() { }
}