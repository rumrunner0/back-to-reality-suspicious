using System;
using Rumrunner0.BackToReality.Suspicious.Monad;
using Xunit;

namespace Rumrunner0.BackToReality.Suspicious.Tests.Monad;

/// <summary>Tests for <see cref="OutcomeKind" />.</summary>
public sealed class OutcomeKindTests
{
	#region Creation

	/// <summary>Ensures that the preset kinds expose the expected names, codes and sides.</summary>
	[Fact]
	public void Presets_ExposeExpectedNamesCodesAndSides()
	{
		Assert.Equal("ok", OutcomeKind.Ok.Name);
		Assert.Equal(0, OutcomeKind.Ok.Code);
		Assert.Same(OutcomeSide.Success, OutcomeKind.Ok.Side);

		Assert.Equal("no_value", OutcomeKind.NoValue.Name);
		Assert.Equal(10, OutcomeKind.NoValue.Code);
		Assert.Same(OutcomeSide.Any, OutcomeKind.NoValue.Side);

		Assert.Equal("invalid", OutcomeKind.Invalid.Name);
		Assert.Equal(1000, OutcomeKind.Invalid.Code);
		Assert.Same(OutcomeSide.Failure, OutcomeKind.Invalid.Side);

		Assert.Equal("conflict", OutcomeKind.Conflict.Name);
		Assert.Equal(1010, OutcomeKind.Conflict.Code);
		Assert.Same(OutcomeSide.Failure, OutcomeKind.Conflict.Side);

		Assert.Equal("failure", OutcomeKind.Failure.Name);
		Assert.Equal(1020, OutcomeKind.Failure.Code);
		Assert.Same(OutcomeSide.Failure, OutcomeKind.Failure.Side);

		Assert.Equal("unavailable", OutcomeKind.Unavailable.Name);
		Assert.Equal(1030, OutcomeKind.Unavailable.Code);
		Assert.Same(OutcomeSide.Failure, OutcomeKind.Unavailable.Side);

		Assert.Equal("unexpected", OutcomeKind.Unexpected.Name);
		Assert.Equal(1999, OutcomeKind.Unexpected.Code);
		Assert.Same(OutcomeSide.Failure, OutcomeKind.Unexpected.Side);
	}

	/// <summary>Ensures that <see cref="OutcomeKind.Custom" /> creates a kind with the provided name, code and side.</summary>
	[Fact]
	public void Custom_CreatesKind()
	{
		var kind = OutcomeKind.Custom(name: "payment_declined", code: 1200, OutcomeSide.Failure);

		Assert.Equal("payment_declined", kind.Name);
		Assert.Equal(1200, kind.Code);
		Assert.Same(OutcomeSide.Failure, kind.Side);
	}

	/// <summary>Ensures that a name that is <c>null</c>, empty, or whitespace is rejected.</summary>
	[Fact]
	public void Custom_WithInvalidName_Throws()
	{
		Assert.ThrowsAny<ArgumentException>(() => OutcomeKind.Custom(name: null!, code: 200, OutcomeSide.Success));
		Assert.ThrowsAny<ArgumentException>(() => OutcomeKind.Custom(name: "", code: 200, OutcomeSide.Success));
		Assert.ThrowsAny<ArgumentException>(() => OutcomeKind.Custom(name: "   ", code: 200, OutcomeSide.Success));
	}

	/// <summary>Ensures that a <c>null</c> side is rejected.</summary>
	[Fact]
	public void Custom_WithNullSide_Throws()
	{
		Assert.ThrowsAny<ArgumentException>(() => OutcomeKind.Custom(name: "custom", code: 200, side: null!));
	}

	/// <summary>Ensures that codes outside the custom ranges [100, 900) and [1100, 1900) are rejected.</summary>
	[Fact]
	public void Custom_WithCodeOutsideCustomRanges_Throws()
	{
		Assert.ThrowsAny<ArgumentException>(() => OutcomeKind.Custom("custom", code: -1, OutcomeSide.Failure));
		Assert.ThrowsAny<ArgumentException>(() => OutcomeKind.Custom("custom", code: 0, OutcomeSide.Failure));
		Assert.ThrowsAny<ArgumentException>(() => OutcomeKind.Custom("custom", code: 99, OutcomeSide.Failure));
		Assert.ThrowsAny<ArgumentException>(() => OutcomeKind.Custom("custom", code: 900, OutcomeSide.Failure));
		Assert.ThrowsAny<ArgumentException>(() => OutcomeKind.Custom("custom", code: 1000, OutcomeSide.Failure));
		Assert.ThrowsAny<ArgumentException>(() => OutcomeKind.Custom("custom", code: 1900, OutcomeSide.Failure));
		Assert.ThrowsAny<ArgumentException>(() => OutcomeKind.Custom("custom", code: 1999, OutcomeSide.Failure));
		Assert.ThrowsAny<ArgumentException>(() => OutcomeKind.Custom("custom", code: 2000, OutcomeSide.Failure));
	}

	/// <summary>Ensures that the custom range boundaries are accepted.</summary>
	[Fact]
	public void Custom_WithCodeOnCustomRangeBoundaries_Creates()
	{
		Assert.Equal(100, OutcomeKind.Custom("custom", code: 100, OutcomeSide.Success).Code);
		Assert.Equal(899, OutcomeKind.Custom("custom", code: 899, OutcomeSide.Success).Code);
		Assert.Equal(1100, OutcomeKind.Custom("custom", code: 1100, OutcomeSide.Failure).Code);
		Assert.Equal(1899, OutcomeKind.Custom("custom", code: 1899, OutcomeSide.Failure).Code);
	}

	/// <summary>Ensures that no custom kind can outrank <see cref="OutcomeKind.Unexpected" /> or underrank <see cref="OutcomeKind.Ok" />.</summary>
	[Fact]
	public void Custom_CanNeverOutrankUnexpectedOrUnderrankOk()
	{
		Assert.True(OutcomeKind.Custom("custom", code: 1899, OutcomeSide.Failure) < OutcomeKind.Unexpected);
		Assert.True(OutcomeKind.Custom("custom", code: 100, OutcomeSide.Success) > OutcomeKind.Ok);
	}

	#endregion

	#region Equality

	/// <summary>Ensures that equality compares the name, the code and the side.</summary>
	[Fact]
	public void Equality_ComparesNameCodeAndSide()
	{
		Assert.Equal(OutcomeKind.Custom("custom", 200, OutcomeSide.Success), OutcomeKind.Custom("custom", 200, OutcomeSide.Success));
		Assert.NotEqual(OutcomeKind.Custom("custom", 200, OutcomeSide.Success), OutcomeKind.Custom("other", 200, OutcomeSide.Success));
		Assert.NotEqual(OutcomeKind.Custom("custom", 200, OutcomeSide.Success), OutcomeKind.Custom("custom", 300, OutcomeSide.Success));
		Assert.NotEqual(OutcomeKind.Custom("custom", 200, OutcomeSide.Success), OutcomeKind.Custom("custom", 200, OutcomeSide.Any));
	}

	#endregion

	#region Comparison

	/// <summary>Ensures that <see cref="OutcomeKind.CompareTo" /> and the comparison operators compare codes.</summary>
	[Fact]
	public void Comparison_UsesCodes()
	{
		Assert.True(OutcomeKind.Ok.CompareTo(OutcomeKind.NoValue) < 0);
		Assert.True(OutcomeKind.NoValue < OutcomeKind.Invalid);
		Assert.True(OutcomeKind.Invalid < OutcomeKind.Conflict);
		Assert.True(OutcomeKind.Conflict < OutcomeKind.Failure);
		Assert.True(OutcomeKind.Failure < OutcomeKind.Unavailable);
		Assert.True(OutcomeKind.Unavailable < OutcomeKind.Unexpected);
		Assert.True(OutcomeKind.Unexpected > OutcomeKind.Ok);
	}

	/// <summary>Ensures that kinds with equal codes compare as equal in ordering.</summary>
	[Fact]
	public void Comparison_WithEqualCodes_IsEqualOrdering()
	{
		var first = OutcomeKind.Custom("first", 1200, OutcomeSide.Failure);
		var second = OutcomeKind.Custom("second", 1200, OutcomeSide.Failure);

		Assert.Equal(0, first.CompareTo(second));
		Assert.True(first >= second);
		Assert.True(first <= second);
		Assert.False(first > second);
		Assert.False(first < second);
	}

	#endregion

	#region Display

	/// <summary>Ensures that <see cref="OutcomeKind.ToString" /> contains the name and the code.</summary>
	[Fact]
	public void ToString_ContainsNameAndCode()
	{
		Assert.Equal("invalid (1000)", OutcomeKind.Invalid.ToString());
		Assert.Equal("payment_declined (1200)", OutcomeKind.Custom("payment_declined", 1200, OutcomeSide.Failure).ToString());
	}

	#endregion
}