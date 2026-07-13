# Back to reality: Suspicious

An outcome-first result monad for .NET — `Suspicious` (unit) and `Suspicious<TValue>`. A replacement for exception-driven control flow, `Try*` methods, and union-type workarounds.

```shell
dotnet add package Rumrunner0.BackToReality.Suspicious
```

Targets .NET 9; strong-named; ships with source and symbols.

Every result carries an **outcome** — a domain identity like HTTP codes, but for your domain: `ok`, `no_value`, `invalid`, `conflict`, `failure`, `unavailable`, `unexpected`, or your own via `OutcomeKind.Custom(name, code, side)`. Success is a per-instance fact: a result is a success if and only if no `Error` is attached. That lets the *same* outcome ride either rail — a repository miss can be a normal success without a value, or a failure, depending on what the producer means:

```csharp
// Absence is normal here — success rail, no value.
Suspicious<User> Find(Guid id) =>
	this._store.TryGetValue(id, out var user) ? Suspicious.Ok(user) : Suspicious.NoValue<User>();

// Absence is a failure here — same outcome identity, failure rail.
Suspicious<User> LoadRequired(Guid id) =>
	this._store.TryGetValue(id, out var user) ? Suspicious.Ok(user) : Suspicious.Fail<User>(Error.NoValue($"User {id} is required"));

// Both answer uniformly:
result.Is(OutcomeKind.NoValue); // was it a miss?
result.IsSuccess;               // which rail?
```

The rail rule: `Error.*` always builds the failure rail — you're constructing an `Error`. A kind-named factory on `Suspicious` constructs on the kind's *home rail*: `Ok` → success, `Invalid`/`Conflict`/`Failure`/`Unavailable`/`Unexpected` → failure, `NoValue` → success (a plain miss). Putting a miss on the failure rail is the explicit opt-in: `Fail<T>(Error.NoValue(…))`.

## Pipelines

`Then` is a short-circuiting bind: the first failure (or valueless success) skips everything downstream. Unwrap with `Match` at the application boundary. The bind family spans both types — unit→unit, unit→generic (`Validate().Then(() => Load())`) and generic→unit (`Load(id).Then(user => Archive(user))`); a **unit** success always runs the binder (rails gate execution, kinds never do — a non-`ok` success kind is consumed by the chain).

```csharp
var report = this.Find(userId)
	.Then(user => this.LoadOrders(user))
	.Map(orders => Report.From(orders))
	.Match
	(
		onValue: r => r.Render(),
		onNoValue: () => "No data",
		onError: e => $"Failed: {e.Description}"
	);

// Or LINQ query syntax:
var summary =
	from user in this.Find(userId)
	from orders in this.LoadOrders(user)
	select $"{user.Name}: {orders.Count} order(s)";
```

`Value` throws on a valueless result — a contract guard, not control flow (the never-throwing paths are `TryGetValue`, `GetValueOr`, and the three-way `Match`/`Switch`). Expected outcomes never throw; API misuse throws immediately.

Picking a consumption path: `Match`/`Switch` at boundaries where every rail must be handled; `GetValueOr` when a genuine fallback exists and the error can be discarded; `TryGetValue` for imperative glue (loops, early returns). All are first-class — they answer different questions.

Two more axes: `MapError` rewrites or enriches the failure side (wrap with a `cause:` at a layer boundary) while successes pass through untouched; `AsUnit()` drops the value axis when only the outcome matters. The reverse re-typing exists for failures only — `AsFailure<T>()` carries the `Error` into a differently-typed result (`if (validation.IsFailure) return validation.AsFailure<User>();`); a success has no value to lift, so there it throws.

`Tap`/`TapError` observe without touching — the instance flows through by reference (logging, metrics, audit mid-chain). The result-returning `Tap` overload is the veto flavor: the effect's failure replaces the result, its success is discarded, and the original — success kind included — flows on (`.Tap(invoice => Charge(invoice, balance))` runs a void-like step without losing the invoice). Overload resolution sends result-returning effects to the veto flavor deliberately: a result you'd ignore is a result that should count.

## Errors

A failure carries exactly ONE immutable `Error`: an `OutcomeKind`, a pure-text `Description`, a structured `CallSite` (captured automatically), an optional `Exception`, and a single `Cause` chain (like `InnerException`). Aggregation is explicit — `Suspicious.Combine(...)` gathers independent checks (validation) into one aggregate error whose children live in `Details`, escalated to the most critical child kind:

```csharp
var validation = Suspicious.Combine
(
	this.ValidateName(request.Name),
	this.ValidateEmail(request.Email),
	this.ValidateAge(request.Age)
);

if (validation.IsFailure) return Suspicious.Fail<User>(validation.Error);
```

`Combine` answers exactly one question — *did they all succeed* — so values are discarded and only errors are gathered. The generic overload is sugar for homogeneous batches (one `TValue` per call); combining differently-typed results drops to the unit rail explicitly:

```csharp
var all = Suspicious.Combine(user.AsUnit(), quota.AsUnit(), sessionCleanup);

if (all.IsFailure) return Suspicious.Fail<Report>(all.Error);
return new Report(user.Value, quota.Value); // read-back — see the caveat below
```

The read-back on the last line is safe only when those producers can't return a valueless success: `no_value` passes `Combine` as a *success* but carries no value, so `.Value` would throw — use `TryGetValue` in flows where a miss can occur. A value-keeping, error-accumulating `Combine<T1, T2>` (tuple result) is deliberately not shipped yet: unlike the fail-fast `Then`/query chains it would gather *all* errors and the values together, but its semantics for a valueless success (there is no tuple component to build from it) need deciding first.

The error tree is queryable: `error.Find(kind)` and `error.Contains(kind)` search the `Details` (recursively), then self, then the `Cause` chain — details-first means a query for the kind an aggregate escalated to resolves to the concrete child, not the synthetic aggregate. Both throw on a kind whose side can't ride the failure rail (e.g. `ok`) — such a kind can never appear in an error, so searching for it is API misuse, mirroring the `Error` constructor's own guard.

Kinds are ordered by severity, and the comparison operators make policies one-liners — `if (run.Error.Kind >= OutcomeKind.Unavailable) PageOnCall();` — with aggregates already escalated to their most critical child.

## Custom kinds

Mint domain-specific outcomes with `OutcomeKind.Custom(name, code, side)` — codes are restricted to `[100, 900)` and `[1100, 1900)`, so no custom kind can outrank `unexpected` or underrank `ok`. Default to a single side (`OutcomeSide.Success` or `OutcomeSide.Failure`); reach for `OutcomeSide.Any` only when the kind genuinely occurs on both rails — the built-in template is `no_value`:

```csharp
static readonly OutcomeKind Partial = OutcomeKind.Custom("partial", code: 150, OutcomeSide.Any);

// Lenient producer — a partial import is still useful: success rail, value attached.
return Suspicious.Success(Partial, new ImportSummary(imported, rejected));

// Strict producer — same domain fact, failure rail.
return Error.Custom(Partial, $"{rejected.Count} of {records.Count} records rejected");

// Consumers check the kind uniformly, then the rail only if it matters:
if (result.Is(Partial)) { /* e.g. HTTP 206 for both rails */ }
```

`Match`/`Switch` dispatch on per-instance state (value present / error attached) — never on the kind or its side. The two-way overloads throw on a success without a value, so use the three-way overloads (`onValue`/`onNoValue`/`onError`) in any flow where valueless successes can occur (`NoValue<T>()`, valueless `Success<T>(kind)`). One pipeline nuance: `Map` preserves a custom success kind; `Then` replaces it with the binder's outcome.

## JSON

System.Text.Json converters ship with the library (wired via attributes) for **internal** transport, persistence, and logging:

```json
{ "outcome": { "name": "invalid", "code": 1000, "side": "failure" }, "error": { "kind": { "name": "invalid", "code": 1000, "side": "failure" }, "description": "Name is required", "site": { "member": "CreateUser", "filePath": "/src/UserService.cs", "line": 42 } } }
```

Don't serialize results into public API schemas — `Match` into DTOs/ProblemDetails at the boundary. Exceptions serialize as `{type, message}` and deserialize to `null` (documented lossy round-trip).

## Demo

A guided tour lives in the `…Demo` project: `Essentials/` walks the fundamentals in reading order (creating → consuming → kinds → the dual-rail miss → chaining → query syntax → combining → errors and custom kinds); `Advanced/` shows real-world flows (a layered registration boundary, an any-side `partial` import, error triage, JSON transport, and a checkout pipeline chaining both result types in one expression). Run it with `dotnet run --project Rumrunner0.BackToReality.Suspicious.Demo`.