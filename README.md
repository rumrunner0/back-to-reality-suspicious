# back-to-reality-suspicious
Outcome-first result monad for .NET with rich error semantics and traceable context.

This repository contains the `Rumrunner0.BackToReality.Suspicious` class library, `Rumrunner0.BackToReality.Suspicious.Demo` demo project, and `Rumrunner0.BackToReality.Suspicious.Tests` test project. All the content in the repository is an original work created as a personal project to replace exception-driven control flow with explicit result values.

[![License](https://img.shields.io/github/license/rumrunner0/back-to-reality-suspicious?label=license)](https://github.com/rumrunner0/back-to-reality-suspicious/blob/main/LICENSE)
[![Nuget](https://img.shields.io/nuget/v/Rumrunner0.BackToReality.Suspicious?logo=nuget&label=nuget)](https://www.nuget.org/packages/Rumrunner0.BackToReality.Suspicious)

## Description
The `Rumrunner0.BackToReality.Suspicious` is a class library implementing an outcome-first result monad, a replacement for exception-driven control flow, `Try*` methods, and union-type workarounds. The unit `Suspicious` represents the outcome of a void-like operation; `Suspicious<TValue>` additionally carries a value. Every result holds an `OutcomeKind`, a domain identity that works like an HTTP status code for your domain, while success stays a per-instance fact: a result is a success if and only if no `Error` is attached, so an outcome such as `no_value` can be a plain success or a failure, depending on what the producer means. A failure carries exactly one immutable `Error` with a kind, a description, an automatically captured call site, an optional exception, and a single cause chain. The whole consumption surface (binding, mapping, matching, side effects, LINQ query syntax) has a `Task`-based async mirror under the same names, with cancellation plumbed through every step.

The `Rumrunner0.BackToReality.Suspicious.Demo` is a console application with a guided example gallery, runnable with `dotnet run --project Rumrunner0.BackToReality.Suspicious.Demo`: the `Essentials` examples walk the fundamentals in reading order (creating and consuming results, outcome kinds, the miss on either rail, chaining, query syntax, combining, errors and custom kinds), and the `Advanced` examples show real-world flows (a layered registration boundary, a partial import, error triage, JSON transport, an order checkout, and an async pipeline with cancellation).

The `Rumrunner0.BackToReality.Suspicious.Tests` is an xunit test project covering the behavior of the library.

## Installation
To install the package, use the following command:
```shell
$ dotnet add package Rumrunner0.BackToReality.Suspicious
```

## Usage
The result types and every extension family live under the single `Rumrunner0.BackToReality.Suspicious.Monad` namespace, so one `using` directive lights up the whole surface; the JSON converters live under `Rumrunner0.BackToReality.Suspicious.Serialization` and are attached to the types, so serialization needs no setup.

### Creating results
Creation splits by rail. The static factories on `Suspicious` mint successes, plus the single explicit bridge to the failure rail, `Fail`; the factories on `Error` mint failures, which flow into results through implicit conversions wherever the result type is known:

- `Suspicious.Ok()` and `Suspicious.Ok(value)` for the plain success.
- `Suspicious.NoValue<TValue>()` for a successful miss.
- `Suspicious.Success(kind)`, `Suspicious.Success(kind, value)`, and the valueless `Suspicious.Success<TValue>(kind)` for other success kinds.
- `Suspicious.Fail(error)` and `Suspicious.Fail<TValue>(error)` to lift an `Error` where no target type helps.
- Implicit conversions from a value to `Suspicious<TValue>` and from an `Error` to either result type.

```csharp
using Rumrunner0.BackToReality.Suspicious.Monad;

Suspicious<User> Parse(string name)
{
	if (string.IsNullOrWhiteSpace(name)) return Error.Invalid("Name is required"); // The implicit failure conversion.
	return new User(name);                                                         // The implicit value conversion.
}
```

The same miss can ride either rail, and the producer decides what absence means:

```csharp
// Absence is normal here: the success rail without a value.
Suspicious<User> Find(Guid id) =>
	this._users.TryGetValue(id, out var user) ? Suspicious.Ok(user) : Suspicious.NoValue<User>();

// Absence is a failure here: the same outcome identity, the failure rail.
Suspicious<User> Require(Guid id) =>
	this._users.TryGetValue(id, out var user) ? Suspicious.Ok(user) : Suspicious.Fail<User>(Error.NoValue($"User {id} is required"));
```

### Outcome kinds
`OutcomeKind` is the domain identity of a result: a `Name`, a `Code` that orders kinds by severity (the comparison operators compare codes), and an `OutcomeSide` that declares the rails the kind can be constructed on (`Success`, `Failure`, or `Any`; a construction-time guard only, since the runtime truth is the presence of an `Error`). The presets are `Ok` (0), `NoValue` (10, the only any-side preset), `Invalid` (1000), `Conflict` (1010), `Failure` (1020), `Unavailable` (1030), and `Unexpected` (1999). `OutcomeKind.Custom(name, code, side)` mints domain-specific kinds; codes are restricted to [100, 900) and [1100, 1900), so no custom kind can outrank `unexpected` or underrank `ok`.

```csharp
using Rumrunner0.BackToReality.Suspicious.Monad;

private static readonly OutcomeKind _partial = OutcomeKind.Custom("partial", code: 150, OutcomeSide.Any);

// A lenient producer keeps a partial import useful: the success rail with a value attached.
return Suspicious.Success(_partial, new ImportSummary(imported, rejected));

// A strict producer treats the same domain fact as a failure.
return Error.Custom(_partial, $"{rejected} of {records.Count} records were rejected");

// Consumers check the kind uniformly and the rail only when it matters; severity policies are one-liners.
if (result.Is(_partial)) this.RespondPartialContent();
if (result.IsFailure && result.Error.Kind >= OutcomeKind.Unavailable) this.PageOnCall();
```

### Consuming results
Both result types expose `Outcome`, `Error`, and the rail flags `IsSuccess` and `IsFailure`; the generic type adds `HasValue` and `Value`. `Value` throws on a valueless result: expected outcomes are values and never throw, while API misuse throws immediately and is never caught. The never-throwing paths:

- `Is(kind)` compares the outcome.
- `TryGetValue(out value)` is the imperative path for loops and early returns.
- `GetValueOr(fallback)` and `GetValueOr(fallbackFactory)` fit flows with a genuine fallback where the error can be discarded; the factory runs only when no value is present.
- `Match` folds every rail into one value; `Switch` runs a handler per rail. On the generic type both come in a two-way shape (`onValue`, `onError`) that throws on a success without a value by contract, and a three-way shape with `onNoValue` that is total; use the three-way one in any flow where a miss can occur.

```csharp
using Rumrunner0.BackToReality.Suspicious.Monad;

var greeting = this.Find(userId).Match
(
	onValue: user => $"Hi, {user.Name}",
	onNoValue: () => "Hi, guest",
	onError: error => $"Sign-in failed: {error.Description}"
);

if (this.Find(userId).TryGetValue(out var cached)) this._cache.Add(cached);
var display = this.Find(userId).Map(user => user.Name).GetValueOr("guest");
```

### Chaining results
`Then` is the short-circuiting bind and `Map` is the value transform; `MapError` rewrites or enriches the failure side while successes pass through untouched. On the generic type the binder and the mapper run only when a value is present, and both a failure and a valueless success are propagated unchanged (fail-fast). On the unit type the binder runs on any success: rails gate execution, kinds never dispatch, so a non-`ok` success kind is consumed by the chain (the binder's outcome wins), while `Map` preserves it. `Then` bridges both types in every direction: unit to unit, unit to generic, and generic to unit.

```csharp
using Rumrunner0.BackToReality.Suspicious.Monad;

var report = this.Find(userId)
	.Then(user => this.LoadOrders(user))
	.Map(orders => Report.From(orders))
	.MapError(error => Error.Failure("Report generation failed", cause: error))
	.Match
	(
		onValue: r => r.Render(),
		onNoValue: () => "No data",
		onError: e => $"Failed: {e.Description}"
	);
```

LINQ query syntax is an alias of the same pipeline (`Select` maps, `SelectMany` binds and projects):

```csharp
var summary =
	from user in this.Find(userId)
	from orders in this.LoadOrders(user)
	select $"{user.Name}: {orders.Count} order(s)";
```

### Side effects
`Tap` observes a success (on the generic type, the value) and `TapError` observes the error of a failure; the instance flows through by reference, success kind included. The result-returning `Tap` overload is the veto flavor: the effect's failure replaces the result, while the effect's success (and its kind) is discarded. Overload resolution routes result-returning lambdas to the veto flavor by design, because a result that would be ignored is a result that should count. Effects that produce `null` throw, and effect exceptions propagate; `Tap` never catches.

```csharp
using Rumrunner0.BackToReality.Suspicious.Monad;

var charged = this.CreateInvoice(order)
	.Tap(invoice => this._logger.Log($"Invoice {invoice.Id} created")) // A pure observation.
	.Tap(invoice => this.Charge(invoice))                              // The veto flavor: a failed charge replaces the result.
	.TapError(error => this._metrics.Increment("invoice.failed"));
```

### Converting results
`AsUnit()` drops the value axis and keeps the outcome and the error; it also exists for `Task`-wrapped results. `AsFailure<TResult>()` re-types a failure by carrying its `Error` into a differently typed result and throws on any success (a success has no value to lift), so it belongs behind an `IsFailure` guard. The conversion family lives on the types themselves rather than in extension classes, so that `AsFailure` takes exactly one explicit type argument.

```csharp
using Rumrunner0.BackToReality.Suspicious.Monad;

Suspicious<User> Register(RegistrationRequest request)
{
	var validation = this.Validate(request);
	if (validation.IsFailure) return validation.AsFailure<User>();

	return this.Save(request);
}
```

### Errors
A failure carries exactly one immutable `Error`: an `OutcomeKind`, a pure-text `Description`, a `CallSite` captured automatically at the creation point (member, file path, line), an optional `Exception`, a single `Cause` chain (like `InnerException`), and `Details`, the child errors of an aggregate. The factories mirror the failure kinds: `Error.NoValue`, `Error.Invalid`, `Error.Conflict`, `Error.Failure`, `Error.Unavailable`, `Error.Unexpected` (with and without an exception), `Error.Custom(kind, ...)`, and the explicit `Error.Aggregate(details, ...)`, whose kind escalates to the most critical child. Every factory takes an optional `cause`, and `WithCause(cause)` copies an existing error with another one. The tree is queryable: `Find(kind)` returns the first matching error and `Contains(kind)` reports existence, searching the `Details` recursively, then self, then the `Cause` chain; details first means a query for the kind an aggregate escalated to resolves to the concrete child, not the synthetic aggregate. Both throw on a kind that can never ride the failure rail, mirroring the constructor's own guard.

```csharp
using Rumrunner0.BackToReality.Suspicious.Monad;

var error = Error.Unavailable
(
	"Payment gateway timed out",
	exception: timeout,
	cause: Error.Failure("All retries were rejected")
);

if (error.Contains(OutcomeKind.Unavailable)) this.ScheduleRetry();
var site = error.Find(OutcomeKind.Failure)?.Site; // at Charge in PaymentService.cs, line 42
```

### Combining results
`Suspicious.Combine(results)` answers exactly one question, did they all succeed: values are discarded, a single failure returns its error as is, and several failures gather into an aggregate escalated to the most critical child kind. The overloads cover unit results, homogeneous generic results, and both again as task collections (awaited with `Task.WhenAll`; the token cancels the wait, not the underlying tasks, and a faulted input faults the combined task). Differently typed results drop to the unit rail explicitly with `AsUnit()`.

```csharp
using Rumrunner0.BackToReality.Suspicious.Monad;

var validation = Suspicious.Combine
(
	this.ValidateName(request.Name),
	this.ValidateEmail(request.Email),
	this.ValidateAge(request.Age)
);

if (validation.IsFailure) return Suspicious.Fail<User>(validation.Error);
```

After a successful `Combine`, reading `Value` off an original result still throws if that producer returned a valueless success (`no_value` passes `Combine` as a success but carries no value); use `TryGetValue` unless the flow is `ok`-only.

### Async pipelines
The pipeline surface has a `Task`-based mirror under the same names: sync and async are overloads selected by the source type (`Task`-wrapped or plain) and the delegate shape, so chains flow without intermediate awaits. Every task-returning member takes a trailing optional `CancellationToken` that is checked between steps, and every async continuation also comes in a token-receiving shape. `OperationCanceledException` always propagates, because cancellation is control flow and never a result, and exceptions from continuations are never caught. Query syntax works over task sources with mixed sync and async `from` clauses. The value-access trio (`Is`, `TryGetValue`, `GetValueOr`) and `AsFailure` are deliberately sync-only: await first, then ask.

```csharp
using Rumrunner0.BackToReality.Suspicious.Monad;

var line = await this.CheckQuota()                              // Task<Suspicious>
	.Then(() => this.LoadProfile(id))                           // An async binder without an await in between.
	.Map(profile => profile.Name)                               // A sync mapper on a task source.
	.Tap((name, ct) => this.Audit(name, ct), cancellationToken) // The token flows into the effect.
	.Match(onValue: name => $"Hi, {name}", onError: error => $"No: {error.Description}");
```

### JSON serialization
The `Rumrunner0.BackToReality.Suspicious.Serialization` namespace ships `System.Text.Json` converters for both result types, `Error`, `OutcomeKind`, and `CallSite`, attached to the types with `[JsonConverter]` attributes, so plain `JsonSerializer` calls work with no registration. The JSON is meant for internal transport, persistence, and logging; public APIs should `Match` into DTOs at the boundary instead of serializing results. An attached exception serializes as its type and message for observability and deserializes to `null` (a documented lossy round-trip); deserialization is validating (the outcome must match the error kind, presets come back as their singleton instances, and custom kinds pass the range checks).

```csharp
using System.Text.Json;
using Rumrunner0.BackToReality.Suspicious.Monad;

var json = JsonSerializer.Serialize(this.Find(userId));
var restored = JsonSerializer.Deserialize<Suspicious<User>>(json);
```

```json
{
	"outcome": { "name": "invalid", "code": 1000, "side": "failure" },
	"error": 
	{
		"kind": { "name": "invalid", "code": 1000, "side": "failure" },
		"description": "Name is required",
		"site": { "member": "CreateUser", "filePath": "/src/UserService.cs", "line": 42 }
	}
}
```

## Contributing
If you have any suggestions, ideas, or feedback to enhance the project, please feel free to create an issue. Your collaboration is welcomed to make this project a bit better.