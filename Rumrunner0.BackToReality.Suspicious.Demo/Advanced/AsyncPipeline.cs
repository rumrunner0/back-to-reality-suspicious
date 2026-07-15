namespace Rumrunner0.BackToReality.Suspicious.Demo.Advanced;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>An async pipeline — one awaited chain over Task-wrapped results, with cancellation plumbing.</summary>
/// <remarks>Sync and async steps are same-named overloads, so the chain flows without intermediate awaits; cancellation surfaces as <see cref="OperationCanceledException" /> — it is never converted into a result.</remarks>
internal static class AsyncPipeline
{
	/// <summary>Profiles by user handle.</summary>
	private static readonly Dictionary<string, string> _profiles = new () { ["roman"] = "Roman, tea merchant" };

	/// <summary>Remaining request quota.</summary>
	private static int _quota = 2;

	/// <summary>Runs the example.</summary>
	internal static async Task Run()
	{
		Console.WriteLine(await Describe("roman", CancellationToken.None));
		Console.WriteLine(await Describe("ghost", CancellationToken.None));

		// The quota is exhausted now — the failed async precondition short-circuits
		// the whole chain, and LoadProfile below stays silent.
		Console.WriteLine(await Describe("roman", CancellationToken.None));

		// Cancellation is control flow, not a result — the chain surfaces OperationCanceledException.
		try
		{
			await Describe("roman", new CancellationToken(canceled: true));
		}
		catch (OperationCanceledException)
		{
			Console.WriteLine("Canceled: the chain surfaced OperationCanceledException — no result was produced");
		}

		// The async query syntax composes Task-wrapped results like the sync one does.
		var query = await
		(
			from profile in Task.FromResult(Suspicious.Ok("Roman"))
			from orders in Task.FromResult(Suspicious.Ok(3))
			select $"{profile} has {orders} order(s)"
		);

		Console.WriteLine(query);
	}

	/// <summary>Describes a user — ONE awaited chain across both result types.</summary>
	/// <param name="handle">The user handle.</param>
	/// <param name="cancellationToken">The cancellation token; threaded through the whole chain.</param>
	/// <returns>A task with the boundary line.</returns>
	private static Task<string> Describe(string handle, CancellationToken cancellationToken)
	{
		// unit precondition (Task<Suspicious>)
		return CheckQuota()

			// unit → generic: an async binder on a task source — no intermediate await
			.Then(() => LoadProfile(handle), cancellationToken)

			// a sync map on a task source
			.Map(static profile => profile.ToUpperInvariant(), cancellationToken)

			// a token-receiving veto tap — the token flows into the effect
			.Tap((profile, ct) => Audit(profile, ct), cancellationToken)

			// telemetry on the failure rail, before the boundary
			.TapError(static e => Console.WriteLine($"(TapError logs: {e.Kind})"), cancellationToken)

			// the boundary — sync handlers on a task source
			.Match
			(
				onValue: static profile => $"Profile: {profile}",
				onError: static e => $"Rejected: {e.Description}",
				cancellationToken
			);
	}

	/// <summary>Checks the request quota — a void-like async precondition.</summary>
	/// <returns>A task with an <c>ok</c> result, or a failure.</returns>
	private static async Task<Suspicious> CheckQuota()
	{
		await Task.Delay(1);
		if (_quota <= 0) return Suspicious.Conflict("Request quota exhausted");

		_quota--;
		return Suspicious.Ok();
	}

	/// <summary>Loads a profile.</summary>
	/// <param name="handle">The user handle.</param>
	/// <returns>A task with the profile, or a failure-rail miss.</returns>
	private static async Task<Suspicious<string>> LoadProfile(string handle)
	{
		Console.WriteLine($"(LoadProfile runs for '{handle}')");
		await Task.Delay(1);

		return _profiles.TryGetValue(handle, out var profile)
			? Suspicious.Ok(profile)
			: Suspicious.Fail<string>(Error.NoValue($"No profile for '{handle}'"));
	}

	/// <summary>Audits the access — a token-receiving void-like step that can veto.</summary>
	/// <param name="profile">The profile.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A task with an <c>ok</c> result.</returns>
	private static async Task<Suspicious> Audit(string profile, CancellationToken cancellationToken)
	{
		await Task.Delay(1, cancellationToken);
		return Suspicious.Ok();
	}
}