using Rumrunner0.BackToReality.Suspicious.Monad;

namespace Rumrunner0.BackToReality.Suspicious.Results;

/// <summary>Ok result for <see cref="Suspicious{TValue}" />.</summary>
/// <remarks>
/// <para>* It should be used to indicate that something is ok, up, has no errors or has required value.</para>
/// <para>* Access this result using <see cref="Factories.Suspicious.Ok" />.</para>
/// <para>* If you want to indicate successful completion of an action, use <see cref="Success" />.</para>
/// </remarks>
public sealed record class Ok
{
	/// <inheritdoc cref="Ok" />
	internal Ok() { }
}