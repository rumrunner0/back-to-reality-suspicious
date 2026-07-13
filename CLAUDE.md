# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

`Rumrunner0.BackToReality.Suspicious` — a .NET 9 (C#, `net9`) NuGet library implementing an outcome-first result monad: the unit `Suspicious` and the generic `Suspicious<TValue>`. Every result carries an `OutcomeKind` (extensible "domain HTTP codes": `ok`, `no_value`, `invalid`, `conflict`, `failure`, `unavailable`, `unexpected`, plus `Custom`); success is a per-instance fact (`IsSuccess` ≡ no `Error` attached), so `no_value` can ride either rail. Failures carry exactly ONE immutable `Error` (single `Cause` chain + `Details` only for `Combine` aggregation). Consumption is `Match`/`Switch`/`Then`/`Map`/`MapError`/`Tap`/`TapError`/`AsUnit`/`AsFailure`/LINQ query syntax. Three projects: the library, `…Demo` (guided example gallery: `Essentials/` fundamentals + `Advanced/` real-world flows, orchestrated by `Program.cs`), and `…Tests` (xUnit).

## Commands

- Build: `dotnet build`
- Test: `dotnet test` (single test: `dotnet test --filter "FullyQualifiedName~SuspiciousOfTValueTests.Ok_CreatesSuccessWithValue"`)
- Demo gallery (doubles as a smoke test): `dotnet run --project Rumrunner0.BackToReality.Suspicious.Demo`
- Pack: `Nuget/pack.zsh` (runs `dotnet pack --configuration Release`)
- Publish to nuget.org: `Nuget/push.zsh` (requires the `NUGET_ORG_API_KEY` env var)

Building requires the strong-name key at `../../documents/rumrunner0_backtoreality_suspicious.snk` — outside the repo, never committed (`*.snk` is gitignored). On a machine without it, the library does not build.

## Releases

Manual, from `main`, no tags, no CI — use the `/release` skill. The version lives in TWO places that must stay in sync: `<VersionPrefix>`/`<VersionSuffix>` in `Rumrunner0.BackToReality.Suspicious/Rumrunner0.BackToReality.Suspicious.csproj` and the hard-coded `VERSION` in `Nuget/push.zsh`. Stable releases have an empty suffix; dev pre-releases use suffix `dev.YYYYMMDD.N` (N = per-day counter). The release commit message is exactly `Release <version>`. Other commit messages are short past-tense sentences (e.g. "Fixed Error and Suspicious constructors").

## Code style

Match the existing source exactly; it deliberately differs from common C# defaults:

- Tabs for indentation — in `.cs` and in `.csproj`/`.props` XML. Files end WITHOUT a final newline. `.editorconfig` codifies the basics; the source is the reference for the rest.
- `<ImplicitUsings>` is disabled: every file needs explicit `using` directives.
- Mandatory `this.` qualification for all instance member access.
- XML doc comments on every member, including private ones (`GenerateDocumentationFile` is on; missing docs on public members raise CS1591).
- Everything is `sealed`; data types are written `sealed record class` (never bare `record`).
- File-scoped namespaces; files organized with `#region` blocks (`Instance State`, `Common API`, `Creation`, `Display`, …).
- Allman braces, except single-statement `if` bodies stay inline without braces: `if (cause is null) return;`
- Private fields `_camelCase`; `var` for initialized locals; digit separators in large numeric literals (`25_000`).
- Long signatures/calls: one parameter per line with the parens on their own lines; named arguments for optional parameters.
- Argument validation via `ArgumentExceptionExtensions` from `Rumrunner0.BackToReality.SharedExtensions`, not raw `throw`.
- Central package management: package versions belong in `Directory.Packages.props`; `PackageReference` entries in csproj files carry no `Version` attribute.

## Gotchas

- Inside any `Rumrunner0.BackToReality.*` namespace, the simple name `Suspicious` resolves to the *namespace*, shadowing the non-generic monad type (`Suspicious<TValue>` is immune — arity-1 lookup skips namespaces). The fix is to place the `using` directives AFTER the file-scoped namespace declaration — usings inside the namespace body win over the parent-namespace walk; the test project, the `Serialization/` converters and the Demo example files do this. Top-level statements (Demo's `Program.cs`) are unaffected. The same shadow hits doc comments: `<see cref="Suspicious" />` binds to the namespace — write `<c>Suspicious</c>` instead (the tests do).
- `Suspicious<TValue>.Value` THROWS on a valueless result (contract guard — no more silent `default(int)`); the never-throwing paths are `TryGetValue`, `GetValueOr`, and the three-way `Match`/`Switch`. The two-way `Match`/`Switch` also throw if a success-without-value shows up. Philosophy: expected outcomes → values, never throw; API misuse → throw, never catch.
- `AsFailure<T>()` (on both types) re-types a FAILURE by carrying its `Error` into a differently-typed result; it THROWS on any success (valued or valueless) — a success has no value to lift, so call it behind an `IsFailure` guard. `GetValueOr(factory)` throws if the factory produces `null` for a null-capable `TValue` — mirrors the eager overload's guard; the factory still runs only on valueless results.
- Unit `Then` runs the binder on ANY success — rails gate execution, kinds never dispatch — so a non-`ok` unit success kind (e.g. `Success(partial)`) is CONSUMED by the chain (the binder's outcome wins); check `Is(...)` first if the kind carries policy. The generic→unit `Then(Func<TValue, Suspicious>)` mirrors the generic one: valueless successes propagate their kind and skip the binder. The monad laws hold on the {ok, error} core; the kind axis is extra-monadic decoration that `Then` normalizes whenever it runs.
- `Tap`/`TapError` observe and return `this` — the success kind is PRESERVED (unlike `Then`). The `Func`-returning `Tap` overload is the VETO flavor: the effect's failure replaces the result; the effect's success (and its kind) is discarded. Overload resolution routes result-returning lambdas to the veto flavor BY DESIGN — an ignored result is the unused-result smell. Effects that produce `null` throw; effect exceptions propagate (Tap never catches).
- Don't serialize results into public API schemas — `Match` into DTOs at the boundary. The JSON converters (`Serialization/`) exist for internal transport, persistence, and logging.
- `OutcomeKind.Custom` codes are restricted to `[100, 900) ∪ [1100, 1900)` — no custom kind can outrank `unexpected (1999)` or underrank `ok (0)`; preset codes live outside the custom ranges.
- `Error.Find`/`Contains` THROW on a kind whose `Side` doesn't allow the failure rail (e.g. `ok`, custom Success-only kinds) — such a kind can never appear in an error tree, so the query would be always-false; the guard mirrors the `Error` constructor's. `no_value` (`Any` side) stays legal.
- `Error.Find` searches `Details` BEFORE self (then the `Cause` chain) — an aggregate escalates its kind from a child, so `Find` on that kind returns the concrete child, not the synthetic aggregate. Only aggregates have `Details`, so every other error still checks self first.
- `Combine` DISCARDS values ("did they all succeed"); the generic overload is homogeneous — mix `TValue`s via `AsUnit()` + the unit overload. Reading `.Value` off the original results after a successful `Combine` throws if a producer returned a `no_value` success — use `TryGetValue` unless the flow is Ok-only. A value-keeping tuple `Combine<T1, T2>` is deliberately deferred pending its `no_value` semantics.
- XML doc crefs inside `Suspicious<TValue>`: the name `Error` in a cref signature collides with the `Error` property — qualify it as `Monad.Error` (the compiler binds bare `Error` to the type, Rider binds it to the property and reports a false "ambiguous reference"). Rider additionally can't overload-resolve crefs to generic methods (`Match{TResult}(…)` shows "ambiguous" even fully signed) — those crefs are valid C# and compiler-clean, and are kept DELIBERATELY: the remaining Rider warnings on `Match` crefs are accepted false positives; don't "fix" them by dropping the crefs.
- CS8002 warnings ("Ardalis.SmartEnum … does not have a strong name") are expected — the library is strong-named, that dependency isn't. Don't try to fix them.
