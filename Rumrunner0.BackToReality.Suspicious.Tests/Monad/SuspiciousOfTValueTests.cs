namespace Rumrunner0.BackToReality.Suspicious.Tests.Monad;

using System;
using Rumrunner0.BackToReality.Suspicious.Monad;
using Xunit;

/// <summary>Tests for <see cref="Suspicious{TValue}" />.</summary>
public sealed class SuspiciousOfTValueTests
{
	#region Creation

	/// <summary>Ensures that <c>Ok</c> creates a success with a value.</summary>
	[Fact]
	public void Ok_CreatesSuccessWithValue()
	{
		var result = Suspicious.Ok("The actual result");

		Assert.True(result.IsSuccess);
		Assert.False(result.IsFailure);
		Assert.True(result.HasValue);
		Assert.Equal("The actual result", result.Value);
		Assert.Equal(OutcomeKind.Ok, result.Outcome);
		Assert.Null(result.Error);
	}

	/// <summary>Ensures that a <c>null</c> value is rejected.</summary>
	[Fact]
	public void Ok_WithNullValue_Throws()
	{
		Assert.Throws<ArgumentNullException>(() => Suspicious.Ok<string>(null!));
	}

	/// <summary>Ensures that <c>NoValue</c> is a cached success without a value.</summary>
	[Fact]
	public void NoValue_IsCachedSuccessWithoutValue()
	{
		var result = Suspicious.NoValue<string>();

		Assert.Same(Suspicious.NoValue<string>(), result);
		Assert.True(result.IsSuccess);
		Assert.False(result.HasValue);
		Assert.Null(result.Error);
		Assert.True(result.Is(OutcomeKind.NoValue));
	}

	/// <summary>Ensures that <c>Success</c> creates successes with a custom success-side kind, with and without a value.</summary>
	[Fact]
	public void Success_WithCustomSuccessKind_CreatesSuccess()
	{
		var kind = OutcomeKind.Custom("from_cache", 200, OutcomeSide.Success);

		var withValue = Suspicious.Success(kind, "The cached result");

		Assert.True(withValue.HasValue);
		Assert.Equal(kind, withValue.Outcome);

		var withoutValue = Suspicious.Success<string>(kind);

		Assert.True(withoutValue.IsSuccess);
		Assert.False(withoutValue.HasValue);
		Assert.Equal(kind, withoutValue.Outcome);
	}

	/// <summary>Ensures that <c>Success</c> rejects a kind whose side doesn't allow the success rail.</summary>
	[Fact]
	public void Success_WithFailureOnlyKind_Throws()
	{
		Assert.ThrowsAny<ArgumentException>(() => Suspicious.Success(OutcomeKind.Invalid, "The actual result"));
		Assert.ThrowsAny<ArgumentException>(() => Suspicious.Success<string>(OutcomeKind.Unexpected));
	}

	/// <summary>Ensures that <c>Fail</c> creates a failure whose outcome is the kind of the error, and that both rails of a miss carry the same outcome identity.</summary>
	[Fact]
	public void Fail_CreatesFailure_AndNoValueRidesEitherRail()
	{
		var error = Error.NoValue("Required entity is missing");
		var failureRail = Suspicious.Fail<string>(error);

		Assert.True(failureRail.IsFailure);
		Assert.Same(error, failureRail.Error);
		Assert.True(failureRail.Is(OutcomeKind.NoValue));

		var successRail = Suspicious.NoValue<string>();

		Assert.True(successRail.IsSuccess);
		Assert.True(successRail.Is(OutcomeKind.NoValue));
	}

	/// <summary>Ensures that the per-kind shorthands create failures of the expected kinds.</summary>
	[Fact]
	public void Shorthands_CreateFailuresOfExpectedKinds()
	{
		Assert.Equal(OutcomeKind.Invalid, Suspicious.Invalid<int>("Value is out of range").Outcome);
		Assert.Equal(OutcomeKind.Conflict, Suspicious.Conflict<int>("Entity already exists").Outcome);
		Assert.Equal(OutcomeKind.Failure, Suspicious.Failure<int>().Outcome);
		Assert.Equal(OutcomeKind.Unavailable, Suspicious.Unavailable<int>().Outcome);
		Assert.Equal(OutcomeKind.Unexpected, Suspicious.Unexpected<int>().Outcome);
	}

	#endregion

	#region Conversion

	/// <summary>Ensures that a value is implicitly converted to an <c>Ok</c> success.</summary>
	[Fact]
	public void ImplicitConversionFromValue_CreatesOkSuccess()
	{
		Suspicious<string> result = "The actual result";

		Assert.True(result.HasValue);
		Assert.Equal(OutcomeKind.Ok, result.Outcome);
		Assert.Equal("The actual result", result.Value);
	}

	/// <summary>Ensures that an <see cref="Error" /> is implicitly converted to a failure.</summary>
	[Fact]
	public void ImplicitConversionFromError_CreatesFailure()
	{
		Suspicious<string> result = Error.Invalid("Name is required");

		Assert.True(result.IsFailure);
		Assert.Equal(OutcomeKind.Invalid, result.Outcome);
	}

	#endregion

	#region Value Access

	/// <summary>Ensures that <c>Value</c> throws on a valueless result instead of silently returning <c>default</c>.</summary>
	[Fact]
	public void Value_OnValuelessResult_Throws()
	{
		Assert.Throws<InvalidOperationException>(() => _ = Suspicious.NoValue<int>().Value);
		Assert.Throws<InvalidOperationException>(() => _ = Suspicious.Failure<int>().Value);
	}

	/// <summary>Ensures that <c>TryGetValue</c> reports the value presence.</summary>
	[Fact]
	public void TryGetValue_ReportsValuePresence()
	{
		Assert.True(Suspicious.Ok(42).TryGetValue(out var value));
		Assert.Equal(42, value);
		Assert.False(Suspicious.NoValue<int>().TryGetValue(out _));
		Assert.False(Suspicious.Failure<int>().TryGetValue(out _));
	}

	/// <summary>Ensures that <c>GetValueOr</c> falls back only on valueless results, and rejects a null fallback — eager or produced by the factory.</summary>
	[Fact]
	public void GetValueOr_FallsBackOnlyOnValuelessResults()
	{
		Assert.Equal(42, Suspicious.Ok(42).GetValueOr(0));
		Assert.Equal(7, Suspicious.NoValue<int>().GetValueOr(7));
		Assert.Equal(7, Suspicious.Failure<int>().GetValueOr(static () => 7));
		Assert.Equal("value", Suspicious.Ok("value").GetValueOr(static () => null!));
		Assert.Throws<ArgumentNullException>(() => Suspicious.NoValue<string>().GetValueOr(fallback: null!));
		Assert.Throws<ArgumentNullException>(() => Suspicious.NoValue<string>().GetValueOr(static () => null!));
	}

	#endregion

	#region Match and Switch

	/// <summary>Ensures that the two-way <c>Match</c> handles a value and an error, and throws on an undeclared no-value success.</summary>
	[Fact]
	public void Match_TwoWay_HandlesValueAndError_AndThrowsOnNoValue()
	{
		var error = Error.Failure(description: "Something failed");

		Assert.Equal(84, Suspicious.Ok(42).Match(onValue: static v => v * 2, onError: static _ => 0));
		Assert.Same(error, Suspicious.Fail<int>(error).Match(onValue: static _ => Error.Failure(description: "Unreachable"), onError: static e => e));
		Assert.Throws<InvalidOperationException>(() => Suspicious.NoValue<int>().Match(onValue: static v => v, onError: static _ => 0));
	}

	/// <summary>Ensures that the three-way <c>Match</c> is total.</summary>
	[Fact]
	public void Match_ThreeWay_IsTotal()
	{
		static string Render(Suspicious<int> result)
		{
			return result.Match
			(
				onValue: static v => $"value: {v}",
				onNoValue: static () => "no value",
				onError: static e => $"error: {e.Kind.Name}"
			);
		}

		Assert.Equal("value: 42", Render(Suspicious.Ok(42)));
		Assert.Equal("no value", Render(Suspicious.NoValue<int>()));
		Assert.Equal("error: invalid", Render(Suspicious.Invalid<int>("Value is out of range")));
	}

	/// <summary>Ensures that <c>Switch</c> invokes the proper handler and its two-way form throws on an undeclared no-value success.</summary>
	[Fact]
	public void Switch_InvokesProperHandler()
	{
		var valueCount = 0;
		var noValueCount = 0;
		var errorCount = 0;

		Suspicious.Ok(42).Switch(onValue: _ => valueCount++, onNoValue: () => noValueCount++, onError: _ => errorCount++);
		Suspicious.NoValue<int>().Switch(onValue: _ => valueCount++, onNoValue: () => noValueCount++, onError: _ => errorCount++);
		Suspicious.Failure<int>().Switch(onValue: _ => valueCount++, onNoValue: () => noValueCount++, onError: _ => errorCount++);

		Assert.Equal(1, valueCount);
		Assert.Equal(1, noValueCount);
		Assert.Equal(1, errorCount);
		Assert.Throws<InvalidOperationException>(() => Suspicious.NoValue<int>().Switch(onValue: static _ => { }, onError: static _ => { }));
	}

	#endregion

	#region Composition

	/// <summary>Ensures that <c>Map</c> transforms the value and preserves a custom success kind.</summary>
	[Fact]
	public void Map_TransformsValue_AndPreservesCustomSuccessKind()
	{
		var kind = OutcomeKind.Custom("from_cache", 200, OutcomeSide.Success);
		var mapped = Suspicious.Success(kind, 21).Map(static v => v * 2);

		Assert.Equal(42, mapped.Value);
		Assert.Equal(kind, mapped.Outcome);
	}

	/// <summary>Ensures that <c>Map</c> propagates valueless results without invoking the mapper.</summary>
	[Fact]
	public void Map_PropagatesValuelessResults()
	{
		var invoked = false;
		var error = Error.Invalid("Name is required");

		var noValue = Suspicious.NoValue<int>().Map(v => { invoked = true; return v.ToString(); });

		Assert.False(invoked);
		Assert.True(noValue.IsSuccess);
		Assert.False(noValue.HasValue);
		Assert.True(noValue.Is(OutcomeKind.NoValue));

		var failure = Suspicious.Fail<int>(error).Map(v => { invoked = true; return v.ToString(); });

		Assert.False(invoked);
		Assert.Same(error, failure.Error);
	}

	/// <summary>Ensures that <c>Then</c> runs the binder only when a value is present and short-circuits otherwise.</summary>
	[Fact]
	public void Then_RunsBinderOnlyOnValue_AndShortCircuitsOtherwise()
	{
		var invocations = 0;

		Suspicious<string> Describe(int value)
		{
			invocations++;
			return Suspicious.Ok($"value: {value}");
		}

		var chained = Suspicious.Ok(42).Then(Describe);

		Assert.Equal(1, invocations);
		Assert.Equal("value: 42", chained.Value);

		var noValue = Suspicious.NoValue<int>().Then(Describe);

		Assert.Equal(1, invocations);
		Assert.False(noValue.HasValue);
		Assert.True(noValue.Is(OutcomeKind.NoValue));

		var error = Error.Unavailable("Storage is down");
		var failure = Suspicious.Fail<int>(error).Then(Describe);

		Assert.Equal(1, invocations);
		Assert.Same(error, failure.Error);
	}

	/// <summary>Ensures that the unit-returning <c>Then</c> mirrors the generic one — the binder runs only on a value; valueless results short-circuit.</summary>
	[Fact]
	public void Then_ToUnit_RunsBinderOnlyOnValue_AndShortCircuitsOtherwise()
	{
		Assert.True(Suspicious.Ok(42).Then(static _ => Suspicious.Ok()).IsSuccess);

		var invoked = false;
		var noValue = Suspicious.NoValue<int>().Then(_ => { invoked = true; return Suspicious.Ok(); });

		Assert.False(invoked);
		Assert.True(noValue.IsSuccess);
		Assert.True(noValue.Is(OutcomeKind.NoValue));

		var error = Error.Unavailable("Storage is down");
		var failure = Suspicious.Fail<int>(error).Then(static _ => Suspicious.Ok());

		Assert.True(failure.IsFailure);
		Assert.Same(error, failure.Error);
	}

	/// <summary>Ensures that <c>Tap</c> observes only a value, lets a result-returning effect veto, and skips valueless results by reference.</summary>
	[Fact]
	public void Tap_ObservesValue_VetoesOnEffectFailure_AndSkipsValuelessResults()
	{
		var observed = 0;
		var ok = Suspicious.Ok(42);

		Assert.Same(ok, ok.Tap(_ => observed++));
		Assert.Equal(1, observed);

		var vetoed = ok.Tap(static _ => Suspicious.Unavailable("Storage is down"));

		Assert.Equal(OutcomeKind.Unavailable, vetoed.Outcome);
		Assert.Same(ok, ok.Tap(static _ => Suspicious.Ok()));
		Assert.Throws<ArgumentNullException>(() => ok.Tap(static _ => (Suspicious)null!));

		var partial = OutcomeKind.Custom("partial", 150, OutcomeSide.Any);
		var kept = Suspicious.Success(partial, 7).Tap(static _ => Suspicious.Ok());

		Assert.Equal(partial, kept.Outcome);

		var miss = Suspicious.NoValue<int>();
		var failure = Suspicious.Invalid<int>("Value is out of range");

		Assert.Same(miss, miss.Tap(_ => observed++));
		Assert.Same(miss, miss.Tap(static _ => Suspicious.Invalid("Never runs")));
		Assert.Same(failure, failure.Tap(_ => observed++));
		Assert.Equal(1, observed);
	}

	/// <summary>Ensures that <c>TapError</c> observes only a failure and flows the instance through.</summary>
	[Fact]
	public void TapError_ObservesOnlyFailure()
	{
		var observed = default(Error);
		var failure = Suspicious.Invalid<int>("Value is out of range");

		Assert.Same(failure, failure.TapError(e => observed = e));
		Assert.Same(failure.Error, observed);

		var ok = Suspicious.Ok(42);

		Assert.Same(ok, ok.TapError(e => observed = null));
		Assert.NotNull(observed);
	}

	/// <summary>Ensures that <c>MapError</c> maps only a failure and returns a success unchanged.</summary>
	[Fact]
	public void MapError_MapsOnlyFailure()
	{
		var success = Suspicious.Ok(42);

		Assert.Same(success, success.MapError(static e => Error.Unexpected(cause: e)));

		var failure = Suspicious.Invalid<int>("Value is out of range");
		var mapped = failure.MapError(static e => Error.Unexpected(cause: e));

		Assert.Equal(OutcomeKind.Unexpected, mapped.Outcome);
		Assert.Same(failure.Error, mapped.Error!.Cause);
	}

	/// <summary>Ensures that <c>AsUnit</c> drops the value axis and keeps the outcome and the error.</summary>
	[Fact]
	public void AsUnit_DropsValueAxis()
	{
		var fromValue = Suspicious.Ok(42).AsUnit();

		Assert.True(fromValue.IsSuccess);
		Assert.Equal(OutcomeKind.Ok, fromValue.Outcome);

		var fromNoValue = Suspicious.NoValue<int>().AsUnit();

		Assert.True(fromNoValue.IsSuccess);
		Assert.True(fromNoValue.Is(OutcomeKind.NoValue));

		var error = Error.Failure(description: "Something failed");
		var fromFailure = Suspicious.Fail<int>(error).AsUnit();

		Assert.True(fromFailure.IsFailure);
		Assert.Same(error, fromFailure.Error);
	}

	/// <summary>Ensures that <c>AsFailure</c> re-types a failure, and throws on any success — valued or not.</summary>
	[Fact]
	public void AsFailure_RetypesFailure_AndThrowsOnSuccess()
	{
		var failure = Suspicious.Invalid<int>("Value is out of range");
		var converted = failure.AsFailure<string>();

		Assert.True(converted.IsFailure);
		Assert.Same(failure.Error, converted.Error);
		Assert.Equal(OutcomeKind.Invalid, converted.Outcome);

		Assert.Throws<InvalidOperationException>(static () => Suspicious.Ok(42).AsFailure<string>());
		Assert.Throws<InvalidOperationException>(static () => Suspicious.NoValue<int>().AsFailure<string>());
	}

	#endregion

	#region Monad Laws

	/// <summary>Ensures the left identity law: lifting a value and binding <c>f</c> equals applying <c>f</c> directly.</summary>
	[Fact]
	public void MonadLaw_LeftIdentity_Holds()
	{
		static Suspicious<string> F(int value) => Suspicious.Ok($"value: {value}");

		AssertEquivalent(F(42), Suspicious.Ok(42).Then(F));
	}

	/// <summary>Ensures the right identity law: binding the lift leaves the monad unchanged.</summary>
	[Fact]
	public void MonadLaw_RightIdentity_Holds()
	{
		static Suspicious<int> Lift(int value) => Suspicious.Ok(value);

		var fromValue = Suspicious.Ok(42);
		var fromNoValue = Suspicious.NoValue<int>();
		var fromError = Suspicious.Invalid<int>("Value is out of range");

		AssertEquivalent(fromValue, fromValue.Then(Lift));
		AssertEquivalent(fromNoValue, fromNoValue.Then(Lift));
		AssertEquivalent(fromError, fromError.Then(Lift));
	}

	/// <summary>Ensures the associativity law: the grouping of chained binds doesn't affect the outcome.</summary>
	[Fact]
	public void MonadLaw_Associativity_Holds()
	{
		static Suspicious<int> F(int value) => value > 0 ? Suspicious.Ok(value + 1) : Suspicious.Invalid<int>("Value must be positive");
		static Suspicious<string> G(int value) => Suspicious.Ok($"value: {value}");

		foreach (var source in new[] { Suspicious.Ok(42), Suspicious.Ok(-1), Suspicious.NoValue<int>(), Suspicious.Unavailable<int>("Storage is down") })
		{
			AssertEquivalent
			(
				source.Then(F).Then(G),
				source.Then(value => F(value).Then(G))
			);
		}
	}

	#endregion

	#region Display

	/// <summary>Ensures that <c>ToString</c> contains the outcome, the value and the error.</summary>
	[Fact]
	public void ToString_ContainsOutcomeValueAndError()
	{
		Assert.Equal("Suspicious { Outcome = ok (0), Value = 42 }", Suspicious.Ok(42).ToString());
		Assert.Equal("Suspicious { Outcome = no_value (10) }", Suspicious.NoValue<int>().ToString());
		Assert.StartsWith("Suspicious { Outcome = invalid (1000), Error = {", Suspicious.Invalid<int>("Value is out of range").ToString());
	}

	#endregion

	#region Utilities

	/// <summary>Asserts that two <see cref="Suspicious{TValue}" /> are observably equivalent.</summary>
	/// <param name="expected">The expected result.</param>
	/// <param name="actual">The actual result.</param>
	/// <typeparam name="TValue">The value type.</typeparam>
	private static void AssertEquivalent<TValue>(Suspicious<TValue> expected, Suspicious<TValue> actual) where TValue : notnull
	{
		Assert.Equal(expected.Outcome, actual.Outcome);
		Assert.Equal(expected.HasValue, actual.HasValue);
		if (expected.HasValue) Assert.Equal(expected.Value, actual.Value);
		Assert.Equal(expected.Error, actual.Error);
	}

	#endregion
}