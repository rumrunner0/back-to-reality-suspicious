using System;
using Rumrunner0.BackToReality.Suspicious.Monad;
using Xunit;

namespace Rumrunner0.BackToReality.Suspicious.Tests.Monad;

/// <summary>Tests for <see cref="Error" />.</summary>
public sealed class ErrorTests
{
	#region Creation

	/// <summary>Ensures that the per-kind factories create errors of the expected kinds.</summary>
	[Fact]
	public void Factories_CreateErrorsOfExpectedKinds()
	{
		Assert.Equal(OutcomeKind.NoValue, Error.NoValue().Kind);
		Assert.Equal(OutcomeKind.Invalid, Error.Invalid("Name is required").Kind);
		Assert.Equal(OutcomeKind.Conflict, Error.Conflict("Entity already exists").Kind);
		Assert.Equal(OutcomeKind.Failure, Error.Failure().Kind);
		Assert.Equal(OutcomeKind.Unavailable, Error.Unavailable().Kind);
		Assert.Equal(OutcomeKind.Unexpected, Error.Unexpected().Kind);
	}

	/// <summary>Ensures that <see cref="Error.Unexpected(Exception, string?, Error?, string, string, int)" /> exposes the exception.</summary>
	[Fact]
	public void Unexpected_WithException_ExposesException()
	{
		var exception = new ApplicationException("App exception occurred");
		var error = Error.Unexpected(exception: exception);

		Assert.Equal(OutcomeKind.Unexpected, error.Kind);
		Assert.Same(exception, error.Exception);
	}

	/// <summary>Ensures that a <c>null</c> exception is rejected if an overload requires one.</summary>
	[Fact]
	public void Unexpected_WithNullException_Throws()
	{
		Assert.ThrowsAny<ArgumentException>(() => Error.Unexpected(exception: null!));
	}

	/// <summary>Ensures that a description that is <c>null</c>, empty, or whitespace is rejected if a factory requires one.</summary>
	[Fact]
	public void Invalid_WithInvalidDescription_Throws()
	{
		Assert.ThrowsAny<ArgumentException>(() => Error.Invalid(description: null!));
		Assert.ThrowsAny<ArgumentException>(() => Error.Invalid(description: ""));
		Assert.ThrowsAny<ArgumentException>(() => Error.Invalid(description: "   "));
	}

	/// <summary>Ensures that <see cref="Error.Custom" /> creates an error of a custom failure-side kind.</summary>
	[Fact]
	public void Custom_WithFailureSideKind_CreatesError()
	{
		var kind = OutcomeKind.Custom("payment_declined", 1200, OutcomeSide.Failure);
		var error = Error.Custom(kind, description: "Card was declined");

		Assert.Equal(kind, error.Kind);
		Assert.Equal("Card was declined", error.Description);
	}

	/// <summary>Ensures that an error can't be created with a kind whose side doesn't allow the failure rail.</summary>
	[Fact]
	public void Custom_WithSuccessOnlyKind_Throws()
	{
		Assert.ThrowsAny<ArgumentException>(() => Error.Custom(OutcomeKind.Ok));
	}

	/// <summary>Ensures that an error can be created with an any-side kind — the failure-rail miss.</summary>
	[Fact]
	public void Custom_WithAnySideKind_CreatesError()
	{
		Assert.Equal(OutcomeKind.NoValue, Error.Custom(OutcomeKind.NoValue, description: "Required entity is missing").Kind);
	}

	#endregion

	#region Description and Site

	/// <summary>Ensures that the description stays pure text and the caller details live in <see cref="Error.Site" />.</summary>
	[Fact]
	public void Description_StaysPure_WhileSiteCapturesCallerDetails()
	{
		var error = Error.Failure(description: "Something failed");

		Assert.Equal("Something failed", error.Description);
		Assert.NotNull(error.Site);
		Assert.Equal(nameof(this.Description_StaysPure_WhileSiteCapturesCallerDetails), error.Site!.Member);
		Assert.Equal($"{nameof(ErrorTests)}.cs", error.Site.FileName);
		Assert.True(error.Site.Line > 0);
	}

	/// <summary>Ensures that the description is <c>null</c> if not provided.</summary>
	[Fact]
	public void Description_WithoutProvidedText_IsNull()
	{
		Assert.Null(Error.Failure().Description);
	}

	#endregion

	#region Cause

	/// <summary>Ensures that <see cref="Error.WithCause" /> creates a copy and leaves the original unchanged.</summary>
	[Fact]
	public void WithCause_CreatesCopy_AndLeavesOriginalUnchanged()
	{
		var original = Error.Failure(description: "Something failed");
		var cause = Error.Unexpected();

		var copy = original.WithCause(cause);

		Assert.NotSame(original, copy);
		Assert.Null(original.Cause);
		Assert.Same(cause, copy.Cause);
		Assert.Equal(original.Description, copy.Description);
		Assert.Equal(original.Kind, copy.Kind);
		Assert.Equal(original.Site, copy.Site);
	}

	/// <summary>Ensures that <see cref="Error.WithCause" /> with <c>null</c> removes the cause on the copy.</summary>
	[Fact]
	public void WithCause_WithNull_RemovesCauseOnCopy()
	{
		var error = Error.Failure(cause: Error.Unexpected());

		Assert.NotNull(error.Cause);
		Assert.Null(error.WithCause(null).Cause);
	}

	#endregion

	#region Search

	/// <summary>Ensures that <see cref="Error.Find" /> searches self and the cause chain.</summary>
	[Fact]
	public void Find_SearchesSelfAndCauseChain()
	{
		var cause = Error.Unexpected();
		var error = Error.Failure(cause: cause);

		Assert.Same(error, error.Find(OutcomeKind.Failure));
		Assert.Same(cause, error.Find(OutcomeKind.Unexpected));
		Assert.Null(error.Find(OutcomeKind.Invalid));
		Assert.True(error.Contains(OutcomeKind.Unexpected));
		Assert.False(error.Contains(OutcomeKind.Conflict));
	}

	/// <summary>Ensures that <see cref="Error.Find" /> searches the details BEFORE self — the kind an aggregate escalated to resolves to the concrete child, not the synthetic aggregate.</summary>
	[Fact]
	public void Find_SearchesDetailsBeforeSelf()
	{
		var invalid = Error.Invalid("Name is required");
		var conflict = Error.Conflict("Entity already exists", cause: Error.Unavailable());
		var aggregate = Error.Aggregate([invalid, conflict]);

		Assert.Equal(OutcomeKind.Conflict, aggregate.Kind);
		Assert.Same(conflict, aggregate.Find(OutcomeKind.Conflict));
		Assert.Same(invalid, aggregate.Find(OutcomeKind.Invalid));
		Assert.NotNull(aggregate.Find(OutcomeKind.Unavailable));
		Assert.True(aggregate.Contains(OutcomeKind.Conflict));
	}

	/// <summary>Ensures that <see cref="Error.Find" /> and <see cref="Error.Contains" /> reject a kind that can never appear in an error — the same rule the constructor enforces.</summary>
	[Fact]
	public void Find_WithSuccessOnlyKind_Throws()
	{
		var error = Error.Failure(description: "Something failed");
		var custom = OutcomeKind.Custom("promoted", 200, OutcomeSide.Success);

		Assert.ThrowsAny<ArgumentException>(() => error.Find(OutcomeKind.Ok));
		Assert.ThrowsAny<ArgumentException>(() => error.Contains(OutcomeKind.Ok));
		Assert.ThrowsAny<ArgumentException>(() => error.Find(custom));
		Assert.ThrowsAny<ArgumentException>(() => error.Contains(custom));
	}

	/// <summary>Ensures that <see cref="Error.Find" /> accepts an any-side kind — a failure-rail miss is legal in an error tree.</summary>
	[Fact]
	public void Find_WithAnySideKind_Searches()
	{
		var miss = Error.NoValue();
		var error = Error.Failure(description: "Something failed", cause: miss);

		Assert.Same(miss, error.Find(OutcomeKind.NoValue));
		Assert.True(error.Contains(OutcomeKind.NoValue));
	}

	#endregion

	#region Aggregation

	/// <summary>Ensures that <see cref="Error.Aggregate" /> exposes the details and escalates the kind to the most critical child.</summary>
	[Fact]
	public void Aggregate_ExposesDetails_AndEscalatesKind()
	{
		var details = new[] { Error.Invalid("Name is required"), Error.Invalid("Email is malformed") };
		var aggregate = Error.Aggregate(details);

		Assert.Equal(OutcomeKind.Invalid, aggregate.Kind);
		Assert.Equal(2, aggregate.Details.Count);
		Assert.Equal("2 error(s) occurred", aggregate.Description);

		var escalated = Error.Aggregate([Error.Invalid("Name is required"), Error.Unexpected()]);

		Assert.Equal(OutcomeKind.Unexpected, escalated.Kind);
	}

	/// <summary>Ensures that <see cref="Error.Aggregate" /> accepts a custom description.</summary>
	[Fact]
	public void Aggregate_WithProvidedDescription_UsesIt()
	{
		Assert.Equal("Validation failed", Error.Aggregate([Error.Invalid("Name is required")], description: "Validation failed").Description);
	}

	/// <summary>Ensures that <see cref="Error.Aggregate" /> rejects <c>null</c> or empty details.</summary>
	[Fact]
	public void Aggregate_WithoutDetails_Throws()
	{
		Assert.ThrowsAny<ArgumentException>(() => Error.Aggregate(details: null!));
		Assert.ThrowsAny<ArgumentException>(() => Error.Aggregate(details: []));
	}

	#endregion

	#region Equality

	/// <summary>Ensures that equality is structural and stable.</summary>
	[Fact]
	public void Equality_IsStructural()
	{
		var exception = new ApplicationException("App exception occurred");
		var error = Error.Failure(description: "Something failed", exception: exception, cause: Error.Unexpected());

		Assert.Equal(error, error.WithCause(error.Cause));
		Assert.NotEqual(error, error.WithCause(null));

		var first = Error.Failure(description: "Something failed");
		var second = Error.Failure(description: "Something failed");

		Assert.NotEqual(first, second);
	}

	#endregion

	#region Display

	/// <summary>Ensures that <see cref="Error.ToString" /> contains the kind, the description, the site and the details.</summary>
	[Fact]
	public void ToString_ContainsKindDescriptionSiteAndDetails()
	{
		var error = Error.Aggregate([Error.Invalid("Name is required")], description: "Validation failed");
		var text = error.ToString();

		Assert.Contains("Kind = invalid (1000)", text);
		Assert.Contains("Description = Validation failed", text);
		Assert.Contains("Site = at", text);
		Assert.Contains("Details = [", text);
	}

	#endregion
}