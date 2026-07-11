namespace Rumrunner0.BackToReality.Suspicious.Tests.Monad;

using Rumrunner0.BackToReality.Suspicious.Monad;
using Xunit;

/// <summary>Tests for <see cref="SuspiciousLinqExtensions" />.</summary>
public sealed class SuspiciousLinqExtensionsTests
{
	#region Query Syntax

	/// <summary>Ensures that a single-clause query maps the value.</summary>
	[Fact]
	public void QuerySyntax_WithSingleClause_MapsValue()
	{
		var result =
			from value in Suspicious.Ok(21)
			select value * 2;

		Assert.Equal(42, result.Value);
		Assert.Equal(OutcomeKind.Ok, result.Outcome);
	}

	/// <summary>Ensures that a multi-clause query composes values and can reuse earlier bindings.</summary>
	[Fact]
	public void QuerySyntax_WithMultipleClauses_ComposesValues()
	{
		var result =
			from first in Suspicious.Ok(2)
			from second in Suspicious.Ok(first * 10)
			select first + second;

		Assert.Equal(22, result.Value);
	}

	/// <summary>Ensures that a query short-circuits on a failure.</summary>
	[Fact]
	public void QuerySyntax_ShortCircuitsOnFailure()
	{
		var invocations = 0;

		Suspicious<int> Track(int value)
		{
			invocations++;
			return Suspicious.Ok(value);
		}

		var error = Error.Unavailable("Storage is down");
		var result =
			from first in Suspicious.Ok(1)
			from second in Suspicious.Fail<int>(error)
			from third in Track(second)
			select first + second + third;

		Assert.True(result.IsFailure);
		Assert.Same(error, result.Error);
		Assert.Equal(0, invocations);
	}

	/// <summary>Ensures that a query short-circuits on a success without a value.</summary>
	[Fact]
	public void QuerySyntax_ShortCircuitsOnNoValue()
	{
		var invocations = 0;

		Suspicious<int> Track(int value)
		{
			invocations++;
			return Suspicious.Ok(value);
		}

		var result =
			from first in Suspicious.Ok(1)
			from second in Suspicious.NoValue<int>()
			from third in Track(second)
			select first + second + third;

		Assert.True(result.IsSuccess);
		Assert.False(result.HasValue);
		Assert.True(result.Is(OutcomeKind.NoValue));
		Assert.Equal(0, invocations);
	}

	#endregion
}