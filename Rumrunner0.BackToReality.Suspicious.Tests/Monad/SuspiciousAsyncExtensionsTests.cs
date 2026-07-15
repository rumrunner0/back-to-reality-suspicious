namespace Rumrunner0.BackToReality.Suspicious.Tests.Monad;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rumrunner0.BackToReality.Suspicious.Monad;
using Xunit;

/// <summary>Tests for the async extensions of the unit <c>Suspicious</c> and task-wrapped unit results.</summary>
public sealed class SuspiciousAsyncExtensionsTests
{
	#region Then

	/// <summary>Ensures that the async unit <c>Then</c> runs the binder on ANY success and short-circuits a failure.</summary>
	[Fact]
	public async Task Then_Async_RunsBinderOnAnySuccess_AndShortCircuitsFailure()
	{
		var chained = await Suspicious.Ok().Then(static () => Task.FromResult(Suspicious.Ok(42)));

		Assert.Equal(42, chained.Value);

		// Convention A — a non-ok success kind is consumed by the chain.
		var partial = OutcomeKind.Custom("partial", 150, OutcomeSide.Any);
		var consumed = await Suspicious.Success(partial).Then(static () => Task.FromResult(Suspicious.Ok()));

		Assert.Equal(OutcomeKind.Ok, consumed.Outcome);

		var failure = Suspicious.Fail(Error.Conflict("Entity already exists"));

		Assert.Same(failure, await failure.Then(static () => Task.FromResult(Suspicious.Ok())));
		Assert.Same(failure.Error, (await failure.Then(static () => Task.FromResult(Suspicious.Ok(42)))).Error);

		var taskChained = await Task.FromResult(Suspicious.Ok())
			.Then(static () => Suspicious.Ok())
			.Then(static () => Task.FromResult(Suspicious.Ok(7)));

		Assert.Equal(7, taskChained.Value);
	}

	#endregion

	#region MapError, Tap and TapError

	/// <summary>Ensures that the async unit <c>MapError</c> maps only a failure.</summary>
	[Fact]
	public async Task MapError_Async_MapsOnlyFailure()
	{
		var success = Suspicious.Ok();

		Assert.Same(success, await success.MapError(static e => Task.FromResult(Error.Failure("Wrapped", cause: e))));

		var mapped = await Task.FromResult<Suspicious>(Error.Conflict("Entity already exists"))
			.MapError(static e => Error.Failure("Wrapped", cause: e));

		Assert.Equal(OutcomeKind.Failure, mapped.Outcome);
		Assert.Equal(OutcomeKind.Conflict, mapped.Error!.Cause!.Kind);
	}

	/// <summary>Ensures that the async unit <c>Tap</c> observes any success, vetoes on an effect failure, and skips a failure by reference.</summary>
	[Fact]
	public async Task Tap_Async_ObservesAnySuccess_Vetoes_AndSkipsFailure()
	{
		var observed = 0;
		var ok = Suspicious.Ok();

		Assert.Same(ok, await ok.Tap(() => { observed++; return Task.CompletedTask; }));
		Assert.Equal(1, observed);

		var vetoed = await ok.Tap(static () => Task.FromResult<Suspicious>(Error.Conflict("Entity already exists")));

		Assert.Equal(OutcomeKind.Conflict, vetoed.Outcome);
		Assert.Same(ok, await ok.Tap(static () => Task.FromResult(Suspicious.Ok())));

		var failure = Suspicious.Fail(Error.Invalid("Name is required"));

		Assert.Same(failure, await failure.Tap(() => { observed++; return Task.CompletedTask; }));
		Assert.Equal(1, observed);

		Assert.Same(ok, await Task.FromResult(ok).Tap(() => observed++));
		Assert.Equal(2, observed);
	}

	/// <summary>Ensures that the async unit <c>TapError</c> observes only a failure.</summary>
	[Fact]
	public async Task TapError_Async_ObservesOnlyFailure()
	{
		var observed = default(Error);
		var failure = Suspicious.Fail(Error.Invalid("Name is required"));

		Assert.Same(failure, await failure.TapError(e => { observed = e; return Task.CompletedTask; }));
		Assert.Same(failure.Error, observed);

		Assert.Same(Suspicious.Ok(), await Task.FromResult(Suspicious.Ok()).TapError(e => observed = null));
		Assert.NotNull(observed);
	}

	#endregion

	#region Match, Switch and Conversion

	/// <summary>Ensures that the async unit <c>Match</c> and <c>Switch</c> invoke the proper handler.</summary>
	[Fact]
	public async Task Match_Switch_Async_InvokeProperHandler()
	{
		var matched = await Suspicious.Ok().Match(
			onSuccess: static () => Task.FromResult("success"),
			onError: static e => Task.FromResult($"error: {e.Description}"));

		Assert.Equal("success", matched);

		var taskMatched = await Task.FromResult<Suspicious>(Error.Conflict("Entity already exists")).Match(
			onSuccess: static () => "success",
			onError: static e => $"error: {e.Description}");

		Assert.Equal("error: Entity already exists", taskMatched);

		var switched = string.Empty;
		await Task.FromResult(Suspicious.Ok()).Switch(
			onSuccess: () => switched = "success",
			onError: e => switched = $"error: {e.Description}");

		Assert.Equal("success", switched);
	}

	#endregion

	#region Combine

	/// <summary>Ensures that <c>Combine</c> over tasks aggregates like the sync overloads.</summary>
	[Fact]
	public async Task Combine_OverTasks_Aggregates()
	{
		var allOk = await Suspicious.Combine([Task.FromResult(Suspicious.Ok()), Task.FromResult(Suspicious.Ok())]);

		Assert.True(allOk.IsSuccess);

		var single = await Suspicious.Combine([Task.FromResult(Suspicious.Ok()), Task.FromResult<Suspicious>(Error.Conflict("Entity already exists"))]);

		Assert.Equal(OutcomeKind.Conflict, single.Outcome);

		var escalated = await Suspicious.Combine(
		[
			Task.FromResult<Suspicious>(Error.Invalid("Name is required")),
			Task.FromResult<Suspicious>(Error.Unexpected("Validator crashed"))
		]);

		Assert.Equal(OutcomeKind.Unexpected, escalated.Outcome);
		Assert.Equal(2, escalated.Error!.Details.Count);

		var generic = await Suspicious.Combine([Task.FromResult(Suspicious.Ok(1)), Task.FromResult(Suspicious.Ok(2))]);

		Assert.True(generic.IsSuccess);
	}

	/// <summary>Ensures that <c>Combine</c> over tasks throws synchronously on empty input, propagates faults, and honors cancellation.</summary>
	[Fact]
	public async Task Combine_OverTasks_ThrowsOnEmpty_PropagatesFaults_AndHonorsCancellation()
	{
		Assert.ThrowsAny<ArgumentException>(() => { _ = Suspicious.Combine((IEnumerable<Task<Suspicious>>)[]); });

		await Assert.ThrowsAsync<InvalidOperationException>(static () => Suspicious.Combine(
		[
			Task.FromResult(Suspicious.Ok()),
			Task.FromException<Suspicious>(new InvalidOperationException("Producer crashed"))
		]));

		var canceled = new CancellationToken(canceled: true);
		var pending = new TaskCompletionSource<Suspicious>();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Suspicious.Combine([pending.Task], canceled));
	}

	#endregion

	#region Cancellation and Guards

	/// <summary>Ensures that a pre-canceled token prevents the unit continuation from running, while short-circuit paths complete.</summary>
	[Fact]
	public async Task CancellationToken_PreCanceled_GatesTheContinuation()
	{
		var canceled = new CancellationToken(canceled: true);
		var invocations = 0;

		Task<Suspicious> Step()
		{
			invocations++;
			return Task.FromResult(Suspicious.Ok());
		}

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Suspicious.Ok().Then(Step, canceled));
		Assert.Equal(0, invocations);

		var failure = await Suspicious.Fail(Error.Conflict("Entity already exists")).Then(Step, canceled);

		Assert.True(failure.IsFailure);
		Assert.Equal(0, invocations);
	}

	/// <summary>Ensures that a null delegate throws synchronously and a null-producing delegate faults the task.</summary>
	[Fact]
	public async Task Guards_NullDelegateThrowsSync_NullProductFaultsTheTask()
	{
		var ok = Suspicious.Ok();

		Assert.ThrowsAny<ArgumentException>(() => { _ = ok.Then((Func<Task<Suspicious>>)null!); });
		await Assert.ThrowsAsync<ArgumentNullException>(() => ok.Then(static () => (Task<Suspicious>)null!));
	}

	/// <summary>Ensures that every async member validates its arguments at the call — synchronously, before any task exists.</summary>
	[Fact]
	public void Guards_AreEager_AcrossFamilies()
	{
		var ok = Suspicious.Ok();
		var okTask = Task.FromResult(Suspicious.Ok());

		// Plain-source members: the guards run before the async Core is entered.
		Assert.ThrowsAny<ArgumentException>(() => { _ = ok.MapError((Func<Error, Task<Error>>)null!); });
		Assert.ThrowsAny<ArgumentException>(() => { _ = ok.Tap((Func<Task>)null!); });
		Assert.ThrowsAny<ArgumentException>(() => { _ = ok.Match((Func<Task<string>>)null!, static _ => Task.FromResult("error")); });
		Assert.ThrowsAny<ArgumentException>(() => { _ = ok.Switch((Func<Task>)null!, static _ => Task.CompletedTask); });

		// Task-source members validate before awaiting the source.
		Assert.ThrowsAny<ArgumentException>(() => { _ = okTask.Then((Func<Suspicious>)null!); });
		Assert.ThrowsAny<ArgumentException>(() => { _ = okTask.Match(static () => "success", (Func<Error, string>)null!); });

		// A null source is misuse too — same synchronous throw.
		Assert.ThrowsAny<ArgumentException>(() => { _ = ((Task<Suspicious>)null!).Switch(static () => { }, static _ => { }); });
		Assert.ThrowsAny<ArgumentException>(() => { _ = Suspicious.Combine((IEnumerable<Task<Suspicious>>)null!); });
	}

	/// <summary>Ensures that null products fault the task, cancellation cancels it, and short-circuits complete it — none of them throw at the call.</summary>
	[Fact]
	public async Task Guards_FaultsAndCancellationLandInTheTask()
	{
		var ok = Suspicious.Ok();
		var canceled = new CancellationToken(canceled: true);

		// A null PRODUCT is not call-site misuse: the call returns normally with an already-faulted task.
		var faulted = ok.Then(static () => (Task<Suspicious>)null!);
		Assert.True(faulted.IsFaulted);
		await Assert.ThrowsAsync<ArgumentNullException>(() => faulted);

		// Cancellation is control flow: the call returns normally with a canceled task, never a failed result.
		var gated = ok.Then(static () => Task.FromResult(Suspicious.Ok()), canceled);
		Assert.True(gated.IsCanceled);
		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => gated);

		// A short-circuiting source never reaches the delegate or the token: the task completes successfully.
		var shortCircuit = Suspicious.Fail(Error.Failure(description: "Something failed")).Then(static () => (Task<Suspicious>)null!, canceled);
		Assert.True(shortCircuit.IsCompletedSuccessfully);
		Assert.True((await shortCircuit).IsFailure);
	}

	#endregion
}