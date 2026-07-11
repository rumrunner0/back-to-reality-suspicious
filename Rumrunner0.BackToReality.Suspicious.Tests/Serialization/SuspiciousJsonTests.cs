namespace Rumrunner0.BackToReality.Suspicious.Tests.Serialization;

using System;
using System.Text.Json;
using Rumrunner0.BackToReality.Suspicious.Monad;
using Xunit;

/// <summary>Tests for the JSON serialization of the monad types.</summary>
public sealed class SuspiciousJsonTests
{
	#region Contract

	/// <summary>Ensures that a success with a value serializes to the documented contract.</summary>
	[Fact]
	public void Serialize_SuccessWithValue_MatchesContract()
	{
		var json = JsonSerializer.Serialize(Suspicious.Ok("The actual result"));

		Assert.Equal("""{"outcome":{"name":"ok","code":0,"side":"success"},"value":"The actual result"}""", json);
	}

	/// <summary>Ensures that a success without a value serializes to the outcome only.</summary>
	[Fact]
	public void Serialize_SuccessWithoutValue_MatchesContract()
	{
		Assert.Equal("""{"outcome":{"name":"no_value","code":10,"side":"any"}}""", JsonSerializer.Serialize(Suspicious.NoValue<string>()));
		Assert.Equal("""{"outcome":{"name":"ok","code":0,"side":"success"}}""", JsonSerializer.Serialize(Suspicious.Ok()));
	}

	/// <summary>Ensures that a failure serializes with a self-contained error whose kind repeats the outcome.</summary>
	[Fact]
	public void Serialize_Failure_ContainsSelfContainedError()
	{
		var json = JsonSerializer.Serialize(Suspicious.Invalid<int>("Value is out of range"));

		Assert.StartsWith("""{"outcome":{"name":"invalid","code":1000,"side":"failure"},"error":{"kind":{"name":"invalid","code":1000,"side":"failure"}""", json);
		Assert.Contains("\"description\":\"Value is out of range\"", json);
		Assert.Contains("\"site\":{\"member\":", json);
	}

	#endregion

	#region Round-Trips

	/// <summary>Ensures that a success with a value round-trips.</summary>
	[Fact]
	public void RoundTrip_SuccessWithValue_IsPreserved()
	{
		var deserialized = JsonSerializer.Deserialize<Suspicious<string>>(JsonSerializer.Serialize(Suspicious.Ok("The actual result")))!;

		Assert.True(deserialized.HasValue);
		Assert.Equal("The actual result", deserialized.Value);
		Assert.Same(OutcomeKind.Ok, deserialized.Outcome);
	}

	/// <summary>Ensures that a success without a value round-trips to the preset kind instance.</summary>
	[Fact]
	public void RoundTrip_SuccessWithoutValue_IsPreserved()
	{
		var deserialized = JsonSerializer.Deserialize<Suspicious<string>>(JsonSerializer.Serialize(Suspicious.NoValue<string>()))!;

		Assert.True(deserialized.IsSuccess);
		Assert.False(deserialized.HasValue);
		Assert.Same(OutcomeKind.NoValue, deserialized.Outcome);
	}

	/// <summary>Ensures that a failure round-trips with a structurally equal error.</summary>
	[Fact]
	public void RoundTrip_Failure_PreservesError()
	{
		var original = Suspicious.Fail<int>(Error.Invalid("Value is out of range", cause: Error.Unavailable("Storage is down")));
		var deserialized = JsonSerializer.Deserialize<Suspicious<int>>(JsonSerializer.Serialize(original))!;

		Assert.True(deserialized.IsFailure);
		Assert.Equal(original.Error, deserialized.Error);
	}

	/// <summary>Ensures that an aggregate error round-trips with its details.</summary>
	[Fact]
	public void RoundTrip_AggregateError_PreservesDetails()
	{
		var original = Suspicious.Fail(Error.Aggregate([Error.Invalid("Name is required"), Error.Unexpected()]));
		var deserialized = JsonSerializer.Deserialize<Suspicious>(JsonSerializer.Serialize(original))!;

		Assert.Equal(original.Error, deserialized.Error);
		Assert.Equal(2, deserialized.Error!.Details.Count);
		Assert.Same(OutcomeKind.Unexpected, deserialized.Outcome);
	}

	/// <summary>Ensures that a custom kind round-trips through <see cref="OutcomeKind.Custom" />.</summary>
	[Fact]
	public void RoundTrip_CustomKind_IsPreserved()
	{
		var kind = OutcomeKind.Custom("payment_declined", 1200, OutcomeSide.Failure);
		var original = Suspicious.Fail<int>(Error.Custom(kind, description: "Card was declined"));
		var deserialized = JsonSerializer.Deserialize<Suspicious<int>>(JsonSerializer.Serialize(original))!;

		Assert.Equal(kind, deserialized.Outcome);
		Assert.True(deserialized.Is(kind));
	}

	/// <summary>Ensures that the exception round-trip is lossy by design: written as type and message, read back as <c>null</c>.</summary>
	[Fact]
	public void RoundTrip_Exception_IsLossy()
	{
		var original = Suspicious.Unexpected<int>(new ApplicationException("App exception occurred"));
		var json = JsonSerializer.Serialize(original);

		Assert.Contains("\"exception\":{\"type\":\"System.ApplicationException\",\"message\":\"App exception occurred\"}", json);
		Assert.Null(JsonSerializer.Deserialize<Suspicious<int>>(json)!.Error!.Exception);
	}

	#endregion

	#region Rejection

	/// <summary>Ensures that a payload whose outcome doesn't match the error kind is rejected.</summary>
	[Fact]
	public void Deserialize_WithOutcomeErrorKindMismatch_Throws()
	{
		const string json = """{"outcome":{"name":"ok","code":0,"side":"success"},"error":{"kind":{"name":"invalid","code":1000,"side":"failure"}}}""";

		Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Suspicious>(json));
		Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Suspicious<int>>(json));
	}

	/// <summary>Ensures that a failure-only outcome without an error is rejected.</summary>
	[Fact]
	public void Deserialize_FailureOutcomeWithoutError_Throws()
	{
		const string json = """{"outcome":{"name":"invalid","code":1000,"side":"failure"}}""";

		Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Suspicious>(json));
		Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Suspicious<int>>(json));
	}

	/// <summary>Ensures that a payload with both a value and an error is rejected.</summary>
	[Fact]
	public void Deserialize_WithBothValueAndError_Throws()
	{
		const string json = """{"outcome":{"name":"invalid","code":1000,"side":"failure"},"value":42,"error":{"kind":{"name":"invalid","code":1000,"side":"failure"}}}""";

		Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Suspicious<int>>(json));
	}

	/// <summary>Ensures that a kind with a reserved code that isn't an exact preset is rejected.</summary>
	[Fact]
	public void Deserialize_WithReservedNonPresetKind_Throws()
	{
		Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<OutcomeKind>("""{"name":"weird","code":950,"side":"failure"}"""));
		Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<OutcomeKind>("""{"name":"ok","code":0,"side":"failure"}"""));
	}

	/// <summary>Ensures that a kind with an unknown side is rejected.</summary>
	[Fact]
	public void Deserialize_WithUnknownSide_Throws()
	{
		Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<OutcomeKind>("""{"name":"custom","code":200,"side":"sideways"}"""));
	}

	#endregion
}