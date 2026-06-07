using Rumrunner0.BackToReality.Suspicious.Monad;

namespace Rumrunner0.BackToReality.Suspicious.Results;

/// <summary>Success result for <see cref="Suspicious{TValue}" />.</summary>
/// <remarks>
/// <para>* It should be used to indicate successful completion of an operation.</para>
/// <para>* Access this result using <see cref="Factories.Suspicious.Success" /></para>
/// <para>* If you want to indicate that something is ok, up or has no errors, use <see cref="Ok" />.</para>
/// </remarks>
public sealed record class Success
{
	/// <inheritdoc cref="Success" />
	internal Success() { }
}