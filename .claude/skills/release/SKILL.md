---
name: release
description: Release Rumrunner0.BackToReality.Suspicious to nuget.org — bumps the version in the csproj AND Nuget/push.zsh, commits, packs, and pushes. Usage — /release 0.14.0 for a stable release, /release dev for the next dev pre-release.
disable-model-invocation: true
---

# Release

Requested release: `$ARGUMENTS`

## Determine the full version

- `$ARGUMENTS` is a bare version like `0.14.0` → stable release: `<VersionPrefix>` = that version, `<VersionSuffix>` = empty. Full version = the prefix (e.g. `0.14.0`).
- `$ARGUMENTS` is `dev` → dev pre-release: keep the current `<VersionPrefix>` from the library csproj; `<VersionSuffix>` = `dev.<YYYYMMDD>.<N>` where `<YYYYMMDD>` is today (`date +%Y%m%d`) and `<N>` is 1 + the number of `Release <prefix>-dev.<YYYYMMDD>.*` commits already in `git log --oneline` for today. Full version = `<prefix>-dev.<YYYYMMDD>.<N>` (e.g. `0.13.0-dev.20260607.1`).
- Anything else (or empty) → ask the user what to release; do not guess.

## Preconditions — stop and report if any fails

1. On `main` and the working tree is clean (`git status`). Unrelated uncommitted changes → stop and ask.
2. `dotnet build` succeeds and `dotnet test` passes.
3. The strong-name key exists at `../../documents/rumrunner0_backtoreality_suspicious.snk` relative to the library csproj (i.e. two levels above the repo root, in `documents/`).

## Steps

1. In `Rumrunner0.BackToReality.Suspicious/Rumrunner0.BackToReality.Suspicious.csproj`, set `<VersionPrefix>` and `<VersionSuffix>` (leave the suffix element empty for a stable release).
2. In `Nuget/push.zsh`, set `readonly VERSION="<full version>"` — this hard-coded value must always match the csproj, or push fails/publishes a stale package.
3. Commit exactly these two files with the message exactly `Release <full version>` (no other wording).
4. Run `Nuget/pack.zsh` and verify `Rumrunner0.BackToReality.Suspicious/bin/Release/Rumrunner0.BackToReality.Suspicious.<full version>.nupkg` now exists.
5. Publishing is IRREVERSIBLE (a nuget.org version can be unlisted but never replaced). Show the user the full version and ask for explicit confirmation before pushing. Then run `Nuget/push.zsh`. It requires `NUGET_ORG_API_KEY` in the environment — if it isn't set, ask the user to export it or to run `! Nuget/push.zsh` themselves (the `!` prefix runs it in-session with their shell environment).
6. After a successful push, run `git push origin main`.

If the push fails after the commit was made, leave the commit in place, report the error, and let the user decide — do not revert or retry automatically.
