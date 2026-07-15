namespace Rumrunner0.BackToReality.Suspicious.Tests.Monad;

using System.Threading.Tasks;
using Rumrunner0.BackToReality.Suspicious.Monad;
using Xunit;

/// <summary>Tests for the async LINQ query-syntax extensions.</summary>
public sealed class SuspiciousAsyncLinqExtensionsTests
{
	/// <summary>Ensures that an all-async query composes values.</summary>
	[Fact]
	public async Task QuerySyntax_AsyncFirstAsyncSecond_ComposesValues()
	{
		var result = await
		(
			from name in Task.FromResult(Suspicious.Ok("Roman"))
			from count in Task.FromResult(Suspicious.Ok(3))
			select $"{name} has {count} order(s)"
		);

		Assert.Equal("Roman has 3 order(s)", result.Value);
	}

	/// <summary>Ensures that mixed sync/async <c>from</c> clauses compose in both directions.</summary>
	[Fact]
	public async Task QuerySyntax_MixedClauses_ComposeValues()
	{
		var asyncFirst = await
		(
			from name in Task.FromResult(Suspicious.Ok("Roman"))
			from count in Suspicious.Ok(3)
			select $"{name}: {count}"
		);

		Assert.Equal("Roman: 3", asyncFirst.Value);

		var syncFirst = await
		(
			from name in Suspicious.Ok("Roman")
			from count in Task.FromResult(Suspicious.Ok(3))
			select $"{name}: {count}"
		);

		Assert.Equal("Roman: 3", syncFirst.Value);
	}

	/// <summary>Ensures that the async query short-circuits on a failure and on a no-value success.</summary>
	[Fact]
	public async Task QuerySyntax_ShortCircuits()
	{
		var invocations = 0;

		Task<Suspicious<int>> Count()
		{
			invocations++;
			return Task.FromResult(Suspicious.Ok(3));
		}

		var fromFailure = await
		(
			from name in Task.FromResult(Suspicious.Invalid<string>("Name is required"))
			from count in Count()
			select $"{name}: {count}"
		);

		Assert.Equal(0, invocations);
		Assert.Equal(OutcomeKind.Invalid, fromFailure.Outcome);

		var fromNoValue = await
		(
			from name in Task.FromResult(Suspicious.NoValue<string>())
			from count in Count()
			select $"{name}: {count}"
		);

		Assert.Equal(0, invocations);
		Assert.True(fromNoValue.Is(OutcomeKind.NoValue));
		Assert.False(fromNoValue.HasValue);
	}
}