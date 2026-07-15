using System.Text.Json;
using Rumrunner0.BackToReality.Suspicious.Monad;
using Xunit;

namespace Rumrunner0.BackToReality.Suspicious.Tests.Monad;

/// <summary>Tests for <see cref="OutcomeSide" />.</summary>
public sealed class OutcomeSideTests
{
	#region Members

	/// <summary>Ensures that the members expose the expected values.</summary>
	[Fact]
	public void Members_ExposeExpectedValues()
	{
		Assert.Equal("success", OutcomeSide.Success.Value);
		Assert.Equal("failure", OutcomeSide.Failure.Value);
		Assert.Equal("any", OutcomeSide.Any.Value);
	}

	/// <summary>Ensures that <see cref="OutcomeSide.AllowsSuccess" /> is <c>true</c> only for <see cref="OutcomeSide.Success" /> and <see cref="OutcomeSide.Any" />.</summary>
	[Fact]
	public void AllowsSuccess_IsTrueForSuccessAndAny()
	{
		Assert.True(OutcomeSide.Success.AllowsSuccess);
		Assert.True(OutcomeSide.Any.AllowsSuccess);
		Assert.False(OutcomeSide.Failure.AllowsSuccess);
	}

	/// <summary>Ensures that <see cref="OutcomeSide.AllowsFailure" /> is <c>true</c> only for <see cref="OutcomeSide.Failure" /> and <see cref="OutcomeSide.Any" />.</summary>
	[Fact]
	public void AllowsFailure_IsTrueForFailureAndAny()
	{
		Assert.True(OutcomeSide.Failure.AllowsFailure);
		Assert.True(OutcomeSide.Any.AllowsFailure);
		Assert.False(OutcomeSide.Success.AllowsFailure);
	}

	#endregion

	#region Serialization

	/// <summary>Ensures that an <see cref="OutcomeSide" /> serializes to its string value and deserializes back.</summary>
	[Fact]
	public void Json_RoundTripsAsStringValue()
	{
		Assert.Equal("\"any\"", JsonSerializer.Serialize(OutcomeSide.Any));
		Assert.Same(OutcomeSide.Failure, JsonSerializer.Deserialize<OutcomeSide>("\"failure\""));
	}

	#endregion
}