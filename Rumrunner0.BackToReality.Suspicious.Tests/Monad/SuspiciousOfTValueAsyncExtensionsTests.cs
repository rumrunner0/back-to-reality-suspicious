namespace Rumrunner0.BackToReality.Suspicious.Tests.Monad;

using System;
using System.Threading;
using System.Threading.Tasks;
using Rumrunner0.BackToReality.Suspicious.Monad;
using Xunit;

/// <summary>Tests for the async extensions of <c>Suspicious&lt;TValue&gt;</c> and task-wrapped generic results.</summary>
public sealed class SuspiciousOfTValueAsyncExtensionsTests
{
	#region Then

	/// <summary>Ensures that the async <c>Then</c> runs the binder only on a value and short-circuits otherwise.</summary>
	[Fact]
	public async Task Then_Async_RunsBinderOnlyOnValue_AndShortCircuitsOtherwise()
	{
		var invocations = 0;

		Task<Suspicious<string>> Describe(int value)
		{
			invocations++;
			return Task.FromResult(Suspicious.Ok($"value: {value}"));
		}

		var chained = await Suspicious.Ok(42).Then(Describe);

		Assert.Equal(1, invocations);
		Assert.Equal("value: 42", chained.Value);

		var noValue = await Suspicious.NoValue<int>().Then(Describe);

		Assert.Equal(1, invocations);
		Assert.False(noValue.HasValue);
		Assert.True(noValue.Is(OutcomeKind.NoValue));

		var error = Error.Unavailable("Storage is down");
		var failure = await Suspicious.Fail<int>(error).Then(Describe);

		Assert.Equal(1, invocations);
		Assert.Same(error, failure.Error);

		var unit = await Suspicious.Ok(42).Then(static _ => Task.FromResult(Suspicious.Ok()));

		Assert.True(unit.IsSuccess);
	}

	/// <summary>Ensures that task-wrapped results chain without intermediate awaits — sync and async continuations mix freely.</summary>
	[Fact]
	public async Task Then_OnTask_ChainsWithoutIntermediateAwaits()
	{
		var result = await Task.FromResult(Suspicious.Ok(40))
			.Then(static value => Suspicious.Ok(value + 1))
			.Then(static value => Task.FromResult(Suspicious.Ok(value + 1)))
			.Map(static value => $"total: {value}");

		Assert.Equal("total: 42", result.Value);

		var failure = await Task.FromResult(Suspicious.Invalid<int>("Value is out of range"))
			.Then(static value => Suspicious.Ok(value + 1))
			.Then(static value => Task.FromResult(Suspicious.Ok(value + 1)));

		Assert.True(failure.IsFailure);
		Assert.Equal(OutcomeKind.Invalid, failure.Outcome);
	}

	#endregion

	#region Map and MapError

	/// <summary>Ensures that the async <c>Map</c> transforms the value and preserves a custom success kind.</summary>
	[Fact]
	public async Task Map_Async_TransformsValue_AndPreservesCustomKind()
	{
		var partial = OutcomeKind.Custom("partial", 150, OutcomeSide.Any);
		var mapped = await Suspicious.Success(partial, 7).Map(static value => Task.FromResult(value * 2));

		Assert.Equal(14, mapped.Value);
		Assert.Equal(partial, mapped.Outcome);

		var miss = await Suspicious.NoValue<int>().Map(static value => Task.FromResult(value * 2));

		Assert.True(miss.Is(OutcomeKind.NoValue));

		var taskMapped = await Task.FromResult(Suspicious.Ok(21)).Map(static value => value * 2);

		Assert.Equal(42, taskMapped.Value);
	}

	/// <summary>Ensures that the async <c>MapError</c> maps only a failure.</summary>
	[Fact]
	public async Task MapError_Async_MapsOnlyFailure()
	{
		var success = Suspicious.Ok(42);

		Assert.Same(success, await success.MapError(static e => Task.FromResult(Error.Failure("Wrapped", cause: e))));

		var failure = await Task.FromResult(Suspicious.Invalid<int>("Value is out of range"))
			.MapError(static e => Error.Failure("Wrapped", cause: e));

		Assert.Equal(OutcomeKind.Failure, failure.Outcome);
		Assert.Equal(OutcomeKind.Invalid, failure.Error!.Cause!.Kind);
	}

	#endregion

	#region Tap and TapError

	/// <summary>Ensures that the async <c>Tap</c> observes only a value, vetoes on an effect failure, and preserves the instance otherwise.</summary>
	[Fact]
	public async Task Tap_Async_ObservesValue_Vetoes_AndPreservesInstance()
	{
		var observed = 0;
		var ok = Suspicious.Ok(42);

		Assert.Same(ok, await ok.Tap(_ => { observed++; return Task.CompletedTask; }));
		Assert.Equal(1, observed);

		var vetoed = await ok.Tap(static _ => Task.FromResult(Suspicious.Unavailable("Storage is down")));

		Assert.Equal(OutcomeKind.Unavailable, vetoed.Outcome);
		Assert.Same(ok, await ok.Tap(static _ => Task.FromResult(Suspicious.Ok())));

		var partial = OutcomeKind.Custom("partial", 150, OutcomeSide.Any);
		var kept = await Suspicious.Success(partial, 7).Tap(static _ => Task.FromResult(Suspicious.Ok()));

		Assert.Equal(partial, kept.Outcome);

		var miss = Suspicious.NoValue<int>();

		Assert.Same(miss, await miss.Tap(_ => { observed++; return Task.CompletedTask; }));
		Assert.Equal(1, observed);

		var taskTapped = await Task.FromResult(ok).Tap(_ => observed++);

		Assert.Same(ok, taskTapped);
		Assert.Equal(2, observed);
	}

	/// <summary>Ensures that the async <c>TapError</c> observes only a failure.</summary>
	[Fact]
	public async Task TapError_Async_ObservesOnlyFailure()
	{
		var observed = default(Error);
		var failure = Suspicious.Invalid<int>("Value is out of range");

		Assert.Same(failure, await failure.TapError(e => { observed = e; return Task.CompletedTask; }));
		Assert.Same(failure.Error, observed);

		var ok = Suspicious.Ok(42);

		Assert.Same(ok, await ok.TapError(e => { observed = null; return Task.CompletedTask; }));
		Assert.NotNull(observed);

		Assert.Same(failure, await Task.FromResult(failure).TapError(e => observed = e));
	}

	#endregion

	#region Match and Switch

	/// <summary>Ensures that the async two-way <c>Match</c> handles a value and an error, and throws on an undeclared no-value success.</summary>
	[Fact]
	public async Task Match_Async_TwoWay_HandlesValueAndError_AndThrowsOnNoValue()
	{
		var fromValue = await Suspicious.Ok(42).Match(
			onValue: static value => Task.FromResult($"value: {value}"),
			onError: static e => Task.FromResult($"error: {e.Description}"));

		Assert.Equal("value: 42", fromValue);

		var fromError = await Task.FromResult(Suspicious.Invalid<int>("Value is out of range")).Match(
			onValue: static value => $"value: {value}",
			onError: static e => $"error: {e.Description}");

		Assert.Equal("error: Value is out of range", fromError);

		await Assert.ThrowsAsync<InvalidOperationException>(static () => Suspicious.NoValue<int>().Match(
			onValue: static value => Task.FromResult($"value: {value}"),
			onError: static e => Task.FromResult($"error: {e.Description}")));
	}

	/// <summary>Ensures that the async three-way <c>Match</c> is total, and the async <c>Switch</c> invokes the proper handler.</summary>
	[Fact]
	public async Task Match_Async_ThreeWay_IsTotal_AndSwitchInvokesProperHandler()
	{
		var fromNoValue = await Task.FromResult(Suspicious.NoValue<int>()).Match(
			onValue: static value => $"value: {value}",
			onNoValue: static () => "no value",
			onError: static e => $"error: {e.Description}");

		Assert.Equal("no value", fromNoValue);

		var switched = string.Empty;
		await Task.FromResult(Suspicious.Ok(42)).Switch(
			onValue: value => switched = $"value: {value}",
			onError: e => switched = $"error: {e.Description}");

		Assert.Equal("value: 42", switched);

		await Suspicious.NoValue<int>().Switch(
			onValue: value => { switched = $"value: {value}"; return Task.CompletedTask; },
			onNoValue: () => { switched = "no value"; return Task.CompletedTask; },
			onError: e => { switched = $"error: {e.Description}"; return Task.CompletedTask; });

		Assert.Equal("no value", switched);
	}

	#endregion

	#region Conversion

	/// <summary>Ensures that the task-wrapped <c>AsUnit</c> drops the value axis of the awaited result.</summary>
	[Fact]
	public async Task AsUnit_OnTask_DropsTheValueAxis()
	{
		var unit = await Task.FromResult(Suspicious.Ok(42)).AsUnit();

		Assert.True(unit.IsSuccess);

		var error = Error.Conflict("Entity already exists");
		var failed = await Task.FromResult(Suspicious.Fail<int>(error)).AsUnit();

		Assert.Same(error, failed.Error);
	}

	#endregion

	#region Cancellation and Guards

	/// <summary>Ensures that a pre-canceled token prevents the continuation from running, while short-circuit paths complete.</summary>
	[Fact]
	public async Task CancellationToken_PreCanceled_GatesTheContinuation()
	{
		var canceled = new CancellationToken(canceled: true);
		var invocations = 0;

		Task<Suspicious<string>> Describe(int value)
		{
			invocations++;
			return Task.FromResult(Suspicious.Ok($"value: {value}"));
		}

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Suspicious.Ok(42).Then(Describe, canceled));
		Assert.Equal(0, invocations);

		// A short-circuiting result never reaches the continuation — the canceled token is irrelevant there.
		var failure = await Suspicious.Invalid<int>("Value is out of range").Then(Describe, canceled);

		Assert.True(failure.IsFailure);
		Assert.Equal(0, invocations);
	}

	/// <summary>Ensures that a token-receiving continuation observes the passed token.</summary>
	[Fact]
	public async Task CancellationToken_IsPassedIntoTheContinuation()
	{
		using var cts = new CancellationTokenSource();
		var seen = default(CancellationToken);

		var result = await Suspicious.Ok(42).Then(
			(value, ct) => { seen = ct; return Task.FromResult(Suspicious.Ok(value + 1)); },
			cts.Token);

		Assert.Equal(43, result.Value);
		Assert.Equal(cts.Token, seen);

		var tapped = await Task.FromResult(Suspicious.Ok(42)).Tap((_, ct) => { seen = ct; return Task.CompletedTask; }, cts.Token);

		Assert.Equal(42, tapped.Value);
		Assert.Equal(cts.Token, seen);
	}

	/// <summary>Ensures that a null delegate throws synchronously and a null-producing delegate faults the task.</summary>
	[Fact]
	public async Task Guards_NullDelegateThrowsSync_NullProductFaultsTheTask()
	{
		var ok = Suspicious.Ok(42);

		Assert.ThrowsAny<ArgumentException>(() => { _ = ok.Then((Func<int, Task<Suspicious<string>>>)null!); });
		await Assert.ThrowsAsync<ArgumentNullException>(() => ok.Then(static _ => (Task<Suspicious<string>>)null!));
		await Assert.ThrowsAsync<ArgumentNullException>(() => ok.Tap(static _ => (Task)null!));
	}

	/// <summary>Ensures that every async member validates its arguments at the call — synchronously, before any task exists.</summary>
	[Fact]
	public void Guards_AreEager_AcrossFamilies()
	{
		var ok = Suspicious.Ok(42);
		var okTask = Task.FromResult(Suspicious.Ok(42));

		// Plain-source members: the guards run before the async Core is entered.
		Assert.ThrowsAny<ArgumentException>(() => { _ = ok.Map((Func<int, Task<string>>)null!); });
		Assert.ThrowsAny<ArgumentException>(() => { _ = ok.Map((Func<int, Task<string>>)null!, CancellationToken.None); });
		Assert.ThrowsAny<ArgumentException>(() => { _ = ok.MapError((Func<Error, Task<Error>>)null!); });
		Assert.ThrowsAny<ArgumentException>(() => { _ = ok.Tap((Func<int, Task>)null!); });
		Assert.ThrowsAny<ArgumentException>(() => { _ = ok.TapError((Func<Error, Task>)null!); });
		Assert.ThrowsAny<ArgumentException>(() => { _ = ok.Match((Func<int, Task<string>>)null!, static _ => Task.FromResult("error")); });

		// Task-source members validate before awaiting the source.
		Assert.ThrowsAny<ArgumentException>(() => { _ = okTask.Then((Func<int, Suspicious<string>>)null!); });
		Assert.ThrowsAny<ArgumentException>(() => { _ = okTask.Map((Func<int, string>)null!); });
		Assert.ThrowsAny<ArgumentException>(() => { _ = okTask.Select((Func<int, string>)null!); });

		// A null source is misuse too — same synchronous throw.
		Assert.ThrowsAny<ArgumentException>(() => { _ = ((Task<Suspicious<int>>)null!).AsUnit(); });
	}

	/// <summary>Ensures that null products fault the task, cancellation cancels it, and short-circuits complete it — none of them throw at the call.</summary>
	[Fact]
	public async Task Guards_FaultsAndCancellationLandInTheTask()
	{
		var ok = Suspicious.Ok(42);
		var canceled = new CancellationToken(canceled: true);

		// A null PRODUCT is not call-site misuse: the call returns normally with an already-faulted task.
		var faulted = ok.Map(static _ => (Task<string>)null!);
		Assert.True(faulted.IsFaulted);
		await Assert.ThrowsAsync<ArgumentNullException>(() => faulted);

		// Cancellation is control flow: the call returns normally with a canceled task, never a failed result.
		var gated = ok.Map(static v => Task.FromResult(v * 2), canceled);
		Assert.True(gated.IsCanceled);
		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => gated);

		// A short-circuiting source never reaches the delegate or the token: the task completes successfully.
		var shortCircuit = Suspicious.Fail<int>(Error.Failure(description: "Something failed")).Map(static _ => (Task<string>)null!, canceled);
		Assert.True(shortCircuit.IsCompletedSuccessfully);
		Assert.True((await shortCircuit).IsFailure);
	}

	#endregion
}