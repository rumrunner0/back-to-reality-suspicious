namespace Rumrunner0.BackToReality.Suspicious.Demo.Advanced;

using System;
using System.Collections.Generic;
using Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>A checkout pipeline — one expression weaving unit and generic binds, a map, taps and error enrichment.</summary>
/// <remarks>Inventory (unit precondition) → reservation (unit→generic) → pricing (generic→generic) → discount (map) → charge (tap — a void-like step that can veto) → telemetry (tap-error) → enrichment → boundary.</remarks>
internal static class OrderCheckout
{
	/// <summary>Stock by SKU.</summary>
	private static readonly Dictionary<string, int> _stock = new () { ["tea"] = 8, ["rum"] = 5 };

	/// <summary>Unit prices by SKU.</summary>
	private static readonly Dictionary<string, decimal> _prices = new () { ["tea"] = 12m, ["rum"] = 40m };

	/// <summary>Runs the example.</summary>
	internal static void Run()
	{
		Console.WriteLine(Checkout("tea", quantity: 2, balance: 100m));
		Console.WriteLine(Checkout("tea", quantity: 999, balance: 100m));
		Console.WriteLine(Checkout("unobtainium", quantity: 1, balance: 100m));
		Console.WriteLine(Checkout("rum", quantity: 3, balance: 50m));
	}

	/// <summary>Checks out an order — the whole flow is ONE chain across both result types.</summary>
	/// <param name="sku">The SKU.</param>
	/// <param name="quantity">The quantity.</param>
	/// <param name="balance">The account balance.</param>
	/// <returns>A receipt line, or the failure folded into text.</returns>
	private static string Checkout(string sku, int quantity, decimal balance)
	{
		// unit — a void-like precondition
		return CheckInventory(sku, quantity)

			// unit → generic: any success runs the binder
			.Then(() => Reserve(sku, quantity))

			// generic → generic: fail-fast on the value
			.Then(static reservation => Price(reservation))

			// map — transforms the value (a bulk discount); the rails stay untouched
			.Map(static invoice => invoice.Total > 100m ? invoice with { Total = invoice.Total * 0.95m } : invoice)

			// tap (the veto flavor): Charge is void-like — its success carries nothing, but
			// the receipt still needs the invoice. Tap runs the effect and, on its success,
			// lets the ORIGINAL result flow through; a failed charge replaces it instead.
			// (Hand-rolled, this is .Then(i => Charge(i, balance).Then(() => Suspicious.Ok(i))).)
			.Tap(invoice => Charge(invoice, balance))

			// tap-error: observes the failure rail without touching it — telemetry mid-chain,
			// BEFORE enrichment, so the raw kind is what gets logged.
			.TapError(static e => Console.WriteLine($"(TapError logs: {e.Kind})"))

			// enriches the failure side once, at the layer boundary
			.MapError(static e => Error.Failure("Checkout failed", cause: e))

			// two-way Match is contractually safe: no step in this chain produces a valueless success
			.Match
			(
				onValue: static invoice => $"Receipt: {invoice.Quantity} x {invoice.Sku} = {invoice.Total:F2}",
				onError: static e => $"{e.Description}: {e.Cause?.Description}"
			);
	}

	/// <summary>Checks that the requested quantity is available.</summary>
	/// <param name="sku">The SKU.</param>
	/// <param name="quantity">The quantity.</param>
	/// <returns>An <c>ok</c> result, or a failure.</returns>
	private static Suspicious CheckInventory(string sku, int quantity)
	{
		if (!_stock.TryGetValue(sku, out var available)) return Suspicious.Invalid($"Unknown SKU '{sku}'");
		if (available < quantity) return Suspicious.Conflict($"Only {available} unit(s) of '{sku}' left");

		return Suspicious.Ok();
	}

	/// <summary>Reserves stock.</summary>
	/// <param name="sku">The SKU.</param>
	/// <param name="quantity">The quantity.</param>
	/// <returns>The reservation.</returns>
	private static Suspicious<Reservation> Reserve(string sku, int quantity)
	{
		Console.WriteLine($"(Reserve runs for {quantity} x {sku})");
		_stock[sku] -= quantity;

		return new Reservation(sku, quantity);
	}

	/// <summary>Prices a reservation.</summary>
	/// <param name="reservation">The reservation.</param>
	/// <returns>The invoice.</returns>
	private static Suspicious<Invoice> Price(Reservation reservation)
	{
		return new Invoice(reservation.Sku, reservation.Quantity, _prices[reservation.Sku] * reservation.Quantity);
	}

	/// <summary>Charges the account — a void-like step.</summary>
	/// <param name="invoice">The invoice.</param>
	/// <param name="balance">The account balance.</param>
	/// <returns>An <c>ok</c> result, or a failure.</returns>
	private static Suspicious Charge(Invoice invoice, decimal balance)
	{
		if (balance < invoice.Total) return Suspicious.Failure($"Balance {balance:F2} can't cover {invoice.Total:F2}");

		return Suspicious.Ok();
	}
}

/// <summary>A stock reservation.</summary>
/// <param name="Sku">The SKU.</param>
/// <param name="Quantity">The quantity.</param>
internal sealed record class Reservation(string Sku, int Quantity);

/// <summary>An invoice.</summary>
/// <param name="Sku">The SKU.</param>
/// <param name="Quantity">The quantity.</param>
/// <param name="Total">The total price.</param>
internal sealed record class Invoice(string Sku, int Quantity, decimal Total);