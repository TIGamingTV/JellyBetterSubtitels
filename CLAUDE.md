# CLAUDE.md

Guidance for Claude Code (and other AI assistants) working in this repository.

## What this is

**JellyBetterSubtitels** ("Better Subtitles") is a Jellyfin **server plugin**
that automatically selects the forced English subtitle track — or, for anime,
the "Signs & Songs" track — as soon as playback starts. Selection happens
entirely server-side, so it behaves identically in Jellyfin Web and Jellyfin
Desktop.

The plugin does **not** rewrite media files or rely on Jellyfin's own subtitle
auto-selection. It listens to each session's `PlaybackStart` event, inspects the
actual subtitle stream titles and flags, and sends the client a
`SetSubtitleStreamIndex` general command. If no track qualifies, it forces
subtitles **off** (index `-1`), overriding whatever Jellyfin would otherwise
have picked.

## Tech stack

- **.NET 8** class library (`net8.0`).
- **Jellyfin 10.10.x** plugin ABI (`targetAbi: 10.10.0.0`).
- `Jellyfin.Controller` / `Jellyfin.Model` **10.10.7** (referenced with
  `ExcludeAssets: runtime` so the plugin builds against, but does not ship, the
  server assemblies).
- Tests: **xUnit** (`Microsoft.NET.Test.Sdk`, `xunit`).

## Project layout

```
JellyBetterSubtitels.sln
Jellyfin.Plugin.BetterSubtitles/
  Plugin.cs                         # Entry point (BasePlugin), config page registration
  PluginServiceRegistrator.cs       # Registers ForcedSubtitleService as a hosted service
  build.yaml                        # Jellyfin plugin metadata (guid, targetAbi, framework)
  Configuration/
    PluginConfiguration.cs          # Enabled, PreferredLanguages, ForcedKeywords + defaults
    configPage.html                 # Dashboard settings UI
  Services/
    ForcedSubtitleService.cs        # IHostedService: hooks PlaybackStart, sends the command
    SubtitleMatcher.cs              # Pure, testable track-scoring logic
    LanguageCodes.cs                # ISO 639-1/-2 normalization (bibliographic vs terminology)
Jellyfin.Plugin.BetterSubtitles.Tests/
  SubtitleMatcherTests.cs
  LanguageCodesTests.cs
manifest.json                       # Plugin repository manifest (release entries)
.github/workflows/build.yml         # CI: build + test; tag push -> publish, release, manifest update
```

## How selection works (`SubtitleMatcher.FindBestIndex`)

For every subtitle stream, a score is assigned; the highest score wins, ties
break to the **lowest stream index**. `null` means "no candidate — turn
subtitles off".

| Score | Condition |
|-------|-----------|
| 4 | `IsForced` **and** language matches **and** title matches a keyword |
| 3 | `IsForced` **and** language matches (Jellyfin's own convention) |
| 2 | Title matches a keyword **and** language matches (the mistagged-anime case) |
| — | Anything else is skipped (never auto-selected) |

- **Language match**: the stream language is undefined (blank or `und`) *or*
  normalizes to one of the preferred languages. Normalization via
  `LanguageCodes.Normalize` folds 2-letter/3-letter and bibliographic vs.
  terminology codes (e.g. `ger`/`deu` → `de`) together.
- **Keyword match**: case-insensitive substring of the stream `Title` (falling
  back to `DisplayTitle`) against any configured keyword.

## Configuration (`PluginConfiguration`)

- `Enabled` (default `true`).
- `PreferredLanguages` — comma-separated ISO 639-2 codes, default `eng`.
- `ForcedKeywords` — newline-separated, default: `forced`, `signs`, `songs`,
  `signs & songs`, `signs and songs`, `sign`, `song`.

## Build & test

```bash
dotnet build JellyBetterSubtitels.sln
dotnet test Jellyfin.Plugin.BetterSubtitles.Tests/Jellyfin.Plugin.BetterSubtitles.Tests.csproj
```

Built DLL: `Jellyfin.Plugin.BetterSubtitles/bin/Debug/net8.0/Jellyfin.Plugin.BetterSubtitles.dll`.

## Conventions & guardrails

- **Keep `SubtitleMatcher` and `LanguageCodes` pure** — no Jellyfin runtime
  dependencies. That is what makes them unit-testable; add tests there when you
  change matching or normalization behavior.
- **Any behavior change to matching, scoring, or language handling must be
  covered by a test** in the Tests project.
- The plugin **deliberately overrides** Jellyfin's own subtitle pick, including
  forcing subtitles off. Don't "fix" this to defer to Jellyfin — it's the whole
  point (see `ForcedSubtitleService` comments).
- The `NegotiationDelay` (500 ms) before sending the command exists because some
  clients haven't finished their initial stream negotiation the instant
  `PlaybackStart` fires. Don't remove it without understanding why.
- Playback-start work runs **detached** from the event and wraps everything in a
  try/catch so exceptions surface in the server log instead of vanishing.
- The plugin `Guid` (`a2e6f8b0-3f7c-4b7a-9b0e-6f1c2d3a4b5c`) must stay in sync
  across `Plugin.cs`, `build.yaml`, and `manifest.json`. Never change it.

## Versioning & releases

- Releases are cut by pushing a `v*` **tag**. CI then publishes the DLL, zips it
  as `better-subtitles_<version>.zip`, creates a GitHub release, and **commits a
  new entry to `manifest.json` on the default branch** automatically.
- Do **not** hand-edit `manifest.json` version entries for a release — let the
  workflow do it. `targetAbi` for new entries is hardcoded to `10.10.0.0` in the
  workflow.

## Targeting a different Jellyfin version

Update `Jellyfin.Controller` / `Jellyfin.Model` versions in the `.csproj` files
and `targetAbi` in `build.yaml` (and the release workflow) to match.

## Git workflow

- `main` — stable/default branch.
- `develop` — integration branch; PRs target `develop`.
- CI (`build.yml`) runs build + test on pushes and PRs to `main` and `develop`.
