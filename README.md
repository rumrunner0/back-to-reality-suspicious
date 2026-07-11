# Back to reality: Suspicious

An outcome-first result monad for .NET — `Suspicious` (unit) and `Suspicious<TValue>`. A replacement for exception-driven control flow, `Try*` methods, and union-type workarounds.

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

`Then` is a short-circuiting bind: the first failure (or valueless success) skips everything downstream. Unwrap with `Match` at the application boundary.

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

`Value` throws on a valueless result — a contract guard, not control flow (the safe paths are `TryGetValue`, `GetValueOr`, `Match`). Expected outcomes never throw; API misuse throws immediately.

Picking a consumption path: `Match`/`Switch` at boundaries where every rail must be handled; `GetValueOr` when a genuine fallback exists and the error can be discarded; `TryGetValue` for imperative glue (loops, early returns). All are first-class — they answer different questions.

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

The error tree is queryable: `error.Find(kind)` and `error.Contains(kind)` search self, the `Details` (recursively), and the `Cause` chain. Both throw on a kind whose side can't ride the failure rail (e.g. `ok`) — such a kind can never appear in an error, so searching for it is API misuse, mirroring the `Error` constructor's own guard.

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