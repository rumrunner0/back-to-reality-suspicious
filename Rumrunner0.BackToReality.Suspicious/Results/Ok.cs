using Rumrunner0.BackToReality.Suspicious.Factories;
using Rumrunner0.BackToReality.Suspicious.Monad;

namespace Rumrunner0.BackToReality.Suspicious.Results;

/// <summary>Ok result for <see cref="Suspicious{TValue}" />.</summary>
/// <remarks>
/// * It should be used to indicate that something is ok, up, has no errors or has required value. <br />
/// * Access this result using <see cref="Suspicious.Ok" />. <br /><br />
/// * If you want to indicate successful completion of an action, use <see cref="Success" />.
/// </remarks>
public sealed record class Ok
{
	/// <inheritdoc cref="Ok" />
	internal Ok() { }
}