using Rumrunner0.BackToReality.Suspicious.Factories;
using Rumrunner0.BackToReality.Suspicious.Monad;

namespace Rumrunner0.BackToReality.Suspicious.Results;

/// <summary>Ok result for <see cref="Suspicious{TResult}" />.</summary>
/// <remarks>
/// * It should be used to indicate that something is ok, up, has no errors or has required value. <br />
/// * If you want to indicate successful completion of an action, use <see cref="Success" />. <br />
/// * While it's possible to create new instances of this class, it's better to use <see cref="Suspicious.Ok" /> property of <see cref="Suspicious"/> factory.
/// </remarks>
public sealed record class Ok;