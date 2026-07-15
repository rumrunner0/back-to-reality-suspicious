namespace Rumrunner0.BackToReality.Suspicious.Demo.Essentials;

using System;
using Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Errors in depth — anatomy, cause chains and custom kinds.</summary>
internal static class ErrorsAndCustomKinds
{
	/// <summary>Custom kind for a declined payment; the code must sit in [100, 900) or [1100, 1900).</summary>
	private static readonly OutcomeKind _paymentDeclined = OutcomeKind.Custom("payment_declined", code: 1200, OutcomeSide.Failure);

	/// <summary>Runs the example.</summary>
	internal static void Run()
	{
		// An error carries a kind, a pure-text description, the call site (captured automatically),
		// an optional exception and a single cause chain — like InnerException, but for results.
		var outage = Error.Unavailable
		(
			"Payment gateway unreachable",
			exception: new TimeoutException("No response in 30 s"),
			cause: Error.Failure("Connection pool exhausted")
		);

		Console.WriteLine(outage);

		// WithCause enriches immutably — the original stays untouched.
		Console.WriteLine(Error.Invalid("Row 7 is malformed").WithCause(Error.Failure("Encoding mismatch")));

		// Custom kinds extend the presets; prefer a single side — `Any` is the escalation
		// for kinds that genuinely occur on both rails (see the PartialImport example).
		Suspicious<string> payment = Error.Custom(_paymentDeclined, "Card was declined");

		Console.WriteLine(payment);
		Console.WriteLine($"Is payment_declined: {payment.Is(_paymentDeclined)}");
	}
}