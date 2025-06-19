using Rumrunner0.BackToReality.Suspicious.Monad;

namespace Rumrunner0.BackToReality.Suspicious.Results;

/// <summary>
/// Ok result for <see cref="Suspicious{TResult}" />. <br />
/// Used to indicate that something is ok, up, has no errors or has required value. <br />
/// To indicate successful completion of an action, use <see cref="Suspicious{TResult}" />.
/// </summary>
public sealed record class Ok { internal Ok() { } }