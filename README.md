# JellyBetterSubtitels

A Jellyfin server plugin that automatically selects the forced English subtitle
track - or, for anime, the "Signs & Songs" track - as soon as playback starts.
It works identically in Jellyfin Web and Jellyfin Desktop, since the selection
happens entirely server-side.

## Why

Jellyfin's built-in subtitle auto-selection only picks a track when its
"forced" disposition flag *and* language tag are both set correctly in the
file. Many anime and foreign releases don't do this - the "Signs & Songs"
track is often untagged, mislabeled, or just named descriptively instead of
being flagged forced - so Jellyfin's picker finds nothing and you're stuck
manually selecting subtitles every episode.

This plugin doesn't rely on Jellyfin's own matching logic or rewrite your
files. Instead it watches every session's playback-start event, looks at the
actual subtitle track titles and flags, and tells the client which subtitle
track to display - the same way Jellyfin's own remote-control commands work.
If nothing looks like a forced/Signs & Songs track, it turns subtitles off -
overriding any track Jellyfin's own auto-selection would otherwise have picked
(e.g. a forced foreign-language track), so you never get an unexpected
full-dialogue subtitle.

## How it decides

For every subtitle stream on the item that just started playing:

1. **Highest confidence**: flagged forced, in a preferred language, *and* the
   title matches a forced keyword (e.g. "Signs & Songs").
2. **High confidence**: flagged forced and in a preferred language (or the
   language tag is blank) - Jellyfin's own convention.
3. **Fallback**: not flagged forced (or mistagged), but the title matches a
   forced keyword *and* the track is in a preferred language (or the language
   tag is blank) - this is what catches the common mistagged anime case.
4. Anything else (full dialogue tracks, keyword tracks in the wrong language)
   is never auto-selected.

The best-scoring candidate wins; ties go to the lowest stream index. If no
stream qualifies, the plugin turns subtitles off - overriding whatever track
Jellyfin's own logic would otherwise have selected.

## Installation

### Option A: Plugin repository (recommended)

1. In Jellyfin, go to **Dashboard → Plugins → Repositories → Add Repository**.
2. Add this repository's `manifest.json` raw URL, e.g.
   `https://raw.githubusercontent.com/TIGamingTV/JellyBetterSubtitels/main/manifest.json`.
3. Go to **Catalog**, find **Better Subtitles**, install, and restart Jellyfin.

### Option B: Manual install

1. Download the latest release zip from the [Releases](../../releases) page.
2. Extract `Jellyfin.Plugin.BetterSubtitles.dll` into your Jellyfin
   `plugins/Better Subtitles/` data folder.
3. Restart Jellyfin.

## Configuration

Go to **Dashboard → Plugins → Better Subtitles**:

- **Enabled** - turn automatic selection on/off.
- **Preferred languages** - comma-separated ISO 639-2 codes (default `eng`).
  A track flagged forced is trusted automatically when its language is one of
  these, or blank.
- **Forced track keywords** - one keyword/phrase per line (defaults are
  `forced`, `signs`, `songs`, `signs & songs`, `signs and songs`, `sign`,
  `song`), matched case-insensitively as substrings of the subtitle track's
  title to catch tracks whose forced flag is missing or wrong.

## Building from source

Requires the .NET 8 SDK.

```bash
dotnet build JellyBetterSubtitels.sln
dotnet test Jellyfin.Plugin.BetterSubtitles.Tests/Jellyfin.Plugin.BetterSubtitles.Tests.csproj
```

The built DLL is at
`Jellyfin.Plugin.BetterSubtitles/bin/Debug/net8.0/Jellyfin.Plugin.BetterSubtitles.dll`.

## Verifying it works

1. Play an episode/movie that has a forced or "Signs & Songs" subtitle track.
2. Check **Dashboard → Logs** for an `Information`-level entry like
   `Selected forced subtitle stream N for "..." on session ...` - this
   confirms the plugin found and applied a match. When no track qualifies
   you'll instead see `No forced/signs & songs subtitle for "..."; disabled
   subtitles on session ...`.
3. Confirm the subtitle appears automatically in both Jellyfin Web and
   Jellyfin Desktop without manually selecting it.

Currently targets the Jellyfin 10.10.x plugin ABI (`targetAbi: 10.10.0.0` in
`Jellyfin.Plugin.BetterSubtitles/build.yaml`). To target a different server
version, update the `Jellyfin.Controller`/`Jellyfin.Model` package versions in
the `.csproj` files and the `targetAbi` in `build.yaml` to match.
