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

## JSON

System.Text.Json converters ship with the library (wired via attributes) for **internal** transport, persistence, and logging:

```json
{ "outcome": { "name": "invalid", "code": 1000, "side": "failure" }, "error": { "kind": { "name": "invalid", "code": 1000, "side": "failure" }, "description": "Name is required", "site": { "member": "CreateUser", "filePath": "/src/UserService.cs", "line": 42 } } }
```

Don't serialize results into public API schemas — `Match` into DTOs/ProblemDetails at the boundary. Exceptions serialize as `{type, message}` and deserialize to `null` (documented lossy round-trip).