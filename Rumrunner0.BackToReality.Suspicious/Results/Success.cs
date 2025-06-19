using Rumrunner0.BackToReality.Suspicious.Monad;

namespace Rumrunner0.BackToReality.Suspicious.Results;

/// <summary>
/// Success result for <see cref="Suspicious{TResult}" />. <br />
/// Used to indicate successful completion of an action. <br />
/// To indicate that something is ok, up, has no errors or has required value, use <see cref="Suspicious{TResult}" />.
/// </summary>
public sealed record class Success { internal Success() { } }