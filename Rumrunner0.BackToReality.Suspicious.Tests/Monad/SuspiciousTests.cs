namespace Rumrunner0.BackToReality.Suspicious.Tests.Monad;

using System;
using System.Collections.Generic;
using Rumrunner0.BackToReality.Suspicious.Monad;
using Xunit;

/// <summary>Tests for the unit <c>Suspicious</c>.</summary>
public sealed class SuspiciousTests
{
	#region Creation

	/// <summary>Ensures that <c>Ok</c> is a cached success without an error.</summary>
	[Fact]
	public void Ok_IsCachedSuccess()
	{
		var result = Suspicious.Ok();

		Assert.Same(Suspicious.Ok(), result);
		Assert.True(result.IsSuccess);
		Assert.False(result.IsFailure);
		Assert.Equal(OutcomeKind.Ok, result.Outcome);
		Assert.Null(result.Error);
	}

	/// <summary>Ensures that <c>Success</c> creates a success with a custom success-side kind.</summary>
	[Fact]
	public void Success_WithCustomSuccessKind_CreatesSuccess()
	{
		var kind = OutcomeKind.Custom("warmed_up", 200, OutcomeSide.Success);
		var result = Suspicious.Success(kind);

		Assert.True(result.IsSuccess);
		Assert.Equal(kind, result.Outcome);
	}

	/// <summary>Ensures that <c>Success</c> accepts an any-side kind.</summary>
	[Fact]
	public void Success_WithAnySideKind_CreatesSuccess()
	{
		Assert.True(Suspicious.Success(OutcomeKind.NoValue).IsSuccess);
	}

	/// <summary>Ensures that <c>Success</c> rejects a kind whose side doesn't allow the success rail.</summary>
	[Fact]
	public void Success_WithFailureOnlyKind_Throws()
	{
		Assert.ThrowsAny<ArgumentException>(() => Suspicious.Success(OutcomeKind.Invalid));
	}

	/// <summary>Ensures that <c>Fail</c> creates a failure whose outcome is the kind of the error.</summary>
	[Fact]
	public void Fail_CreatesFailure_WithOutcomeOfError()
	{
		var error = Error.Conflict("Entity already exists");
		var result = Suspicious.Fail(error);

		Assert.True(result.IsFailure);
		Assert.False(result.IsSuccess);
		Assert.Same(error, result.Error);
		Assert.Equal(OutcomeKind.Conflict, result.Outcome);
	}

	/// <summary>Ensures that <c>Fail</c> rejects a <c>null</c> error — a failure without an error is unrepresentable.</summary>
	[Fact]
	public void Fail_WithNull_Throws()
	{
		Assert.ThrowsAny<ArgumentException>(() => Suspicious.Fail(null!));
	}

	/// <summary>Ensures that the per-kind shorthands create failures of the expected kinds.</summary>
	[Fact]
	public void Shorthands_CreateFailuresOfExpectedKinds()
	{
		Assert.Equal(OutcomeKind.Invalid, Suspicious.Invalid("Name is required").Outcome);
		Assert.Equal(OutcomeKind.Conflict, Suspicious.Conflict("Entity already exists").Outcome);
		Assert.Equal(OutcomeKind.Failure, Suspicious.Failure().Outcome);
		Assert.Equal(OutcomeKind.Unavailable, Suspicious.Unavailable().Outcome);
		Assert.Equal(OutcomeKind.Unexpected, Suspicious.Unexpected().Outcome);
		Assert.Equal(OutcomeKind.Unexpected, Suspicious.Unexpected(new ApplicationException("App exception occurred")).Outcome);
	}

	/// <summary>Ensures that an <see cref="Error" /> is implicitly converted to a failure.</summary>
	[Fact]
	public void ImplicitConversionFromError_CreatesFailure()
	{
		Suspicious result = Error.Failure(description: "Something failed");

		Assert.True(result.IsFailure);
		Assert.Equal(OutcomeKind.Failure, result.Outcome);
	}

	#endregion

	#region Instance API

	/// <summary>Ensures that <c>Is</c> compares the outcome.</summary>
	[Fact]
	public void Is_ComparesOutcome()
	{
		var result = Suspicious.Invalid("Name is required");

		Assert.True(result.Is(OutcomeKind.Invalid));
		Assert.False(result.Is(OutcomeKind.Failure));
		Assert.ThrowsAny<ArgumentException>(() => result.Is(null!));
	}

	/// <summary>Ensures that <c>Match</c> invokes the proper handler.</summary>
	[Fact]
	public void Match_InvokesProperHandler()
	{
		var error = Error.Failure(description: "Something failed");

		Assert.Equal("success", Suspicious.Ok().Match(onSuccess: static () => "success", onError: static _ => "error"));
		Assert.Same(error, Suspicious.Fail(error).Match<object>(onSuccess: static () => "success", onError: static e => e));
	}

	/// <summary>Ensures that <c>Switch</c> invokes the proper handler.</summary>
	[Fact]
	public void Switch_InvokesProperHandler()
	{
		var successCount = 0;
		var errorCount = 0;

		Suspicious.Ok().Switch(onSuccess: () => successCount++, onError: _ => errorCount++);
		Suspicious.Failure().Switch(onSuccess: () => successCount++, onError: _ => errorCount++);

		Assert.Equal(1, successCount);
		Assert.Equal(1, errorCount);
	}

	/// <summary>Ensures that <c>MapError</c> maps only a failure and returns a success unchanged.</summary>
	[Fact]
	public void MapError_MapsOnlyFailure()
	{
		var success = Suspicious.Ok();
		var failure = Suspicious.Failure(description: "Something failed");

		Assert.Same(success, success.MapError(static e => Error.Unexpected(cause: e)));

		var mapped = failure.MapError(static e => Error.Unexpected(cause: e));

		Assert.Equal(OutcomeKind.Unexpected, mapped.Outcome);
		Assert.Same(failure.Error, mapped.Error!.Cause);
	}

	/// <summary>Ensures that <c>AsFailure</c> carries the error into a failed generic result, and throws on a success.</summary>
	[Fact]
	public void AsFailure_CarriesError_AndThrowsOnSuccess()
	{
		var failure = Suspicious.Conflict("Entity already exists");
		var converted = failure.AsFailure<int>();

		Assert.True(converted.IsFailure);
		Assert.Same(failure.Error, converted.Error);
		Assert.Equal(OutcomeKind.Conflict, converted.Outcome);

		Assert.Throws<InvalidOperationException>(static () => Suspicious.Ok().AsFailure<int>());
	}

	#endregion

	#region Aggregation

	/// <summary>Ensures that <c>Combine</c> of successes only is <c>Ok</c>; a no-value success counts as a success.</summary>
	[Fact]
	public void Combine_WithSuccessesOnly_IsOk()
	{
		var result = Suspicious.Combine
		(
			Suspicious.Ok(),
			Suspicious.Success(OutcomeKind.NoValue)
		);

		Assert.True(result.IsSuccess);
		Assert.Equal(OutcomeKind.Ok, result.Outcome);
	}

	/// <summary>Ensures that <c>Combine</c> with exactly one failure carries that error without an aggregate.</summary>
	[Fact]
	public void Combine_WithSingleFailure_CarriesThatError()
	{
		var error = Error.Invalid("Name is required");
		var result = Suspicious.Combine
		(
			Suspicious.Ok(),
			Suspicious.Fail(error)
		);

		Assert.True(result.IsFailure);
		Assert.Same(error, result.Error);
		Assert.Empty(result.Error!.Details);
	}

	/// <summary>Ensures that <c>Combine</c> with multiple failures aggregates them and escalates the kind.</summary>
	[Fact]
	public void Combine_WithMultipleFailures_Aggregates()
	{
		var result = Suspicious.Combine
		(
			Suspicious.Invalid("Name is required"),
			Suspicious.Invalid("Email is malformed"),
			Suspicious.Unexpected()
		);

		Assert.True(result.IsFailure);
		Assert.Equal(OutcomeKind.Unexpected, result.Outcome);
		Assert.Equal(3, result.Error!.Details.Count);
		Assert.Equal("3 error(s) occurred", result.Error.Description);
	}

	/// <summary>Ensures that <c>Combine</c> over <c>Suspicious&lt;TValue&gt;</c> discards values and answers whether all succeeded.</summary>
	[Fact]
	public void Combine_OverGenericResults_DiscardsValues()
	{
		var allOk = Suspicious.Combine
		(
			Suspicious.Ok(42),
			Suspicious.NoValue<int>()
		);

		Assert.True(allOk.IsSuccess);

		var failed = Suspicious.Combine
		(
			Suspicious.Ok(42),
			Suspicious.Invalid<int>("Value is out of range")
		);

		Assert.True(failed.IsFailure);
		Assert.Equal(OutcomeKind.Invalid, failed.Outcome);
	}

	/// <summary>Ensures that <c>Combine</c> rejects <c>null</c> and empty inputs.</summary>
	[Fact]
	public void Combine_WithoutResults_Throws()
	{
		Assert.ThrowsAny<ArgumentException>(() => Suspicious.Combine((IEnumerable<Suspicious>)null!));
		Assert.ThrowsAny<ArgumentException>(() => Suspicious.Combine());
	}

	#endregion

	#region Display

	/// <summary>Ensures that <c>ToString</c> contains the outcome and the error.</summary>
	[Fact]
	public void ToString_ContainsOutcomeAndError()
	{
		Assert.Equal("Suspicious { Outcome = ok (0) }", Suspicious.Ok().ToString());

		var text = Suspicious.Invalid("Name is required").ToString();

		Assert.StartsWith("Suspicious { Outcome = invalid (1000), Error = {", text);
		Assert.Contains("Name is required", text);
	}

	#endregion
}