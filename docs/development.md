# Development

Notes for building, testing, and releasing the plugin. End users do not need
this; see the [README](../README.md).

## Requirements

- .NET 10 SDK
- ICU libraries (normally present on desktop systems). Without them .NET runs
  in invariant globalization mode and the language-code normalization tests
  for 2-letter ↔ 3-letter codes fail.

## Build and test

Run from the repository root:

```bash
dotnet build JellyBetterSubtitels.sln
dotnet test Jellyfin.Plugin.BetterSubtitles.Tests/Jellyfin.Plugin.BetterSubtitles.Tests.csproj
```

The built plugin is
`Jellyfin.Plugin.BetterSubtitles/bin/Debug/net10.0/Jellyfin.Plugin.BetterSubtitles.dll`.
To try it on a test server, copy that DLL into a folder inside the server's
`plugins` folder and restart Jellyfin.

## Project layout

| Path | Purpose |
|------|---------|
| `Jellyfin.Plugin.BetterSubtitles/Plugin.cs` | Plugin entry point; registers the settings page |
| `Jellyfin.Plugin.BetterSubtitles/PluginServiceRegistrator.cs` | Registers `ForcedSubtitleService` as a hosted service |
| `Jellyfin.Plugin.BetterSubtitles/Services/ForcedSubtitleService.cs` | Handles `PlaybackStart` and sends `SetSubtitleStreamIndex` |
| `Jellyfin.Plugin.BetterSubtitles/Services/SubtitleMatcher.cs` | Pure track-ranking logic |
| `Jellyfin.Plugin.BetterSubtitles/Services/LanguageCodes.cs` | ISO 639 code normalization |
| `Jellyfin.Plugin.BetterSubtitles/Configuration/` | Settings model and dashboard page (`configPage.html`, embedded resource) |
| `Jellyfin.Plugin.BetterSubtitles/build.yaml` | Plugin metadata (GUID, target ABI, framework) |
| `Jellyfin.Plugin.BetterSubtitles.Tests/` | xUnit tests |
| `manifest.json` | Plugin repository manifest served to Jellyfin |
| `.github/workflows/build.yml` | CI build, test, and release |

Keep `SubtitleMatcher` and `LanguageCodes` free of Jellyfin runtime
dependencies, and cover any change to matching or language handling with a
test. The plugin GUID `a2e6f8b0-3f7c-4b7a-9b0e-6f1c2d3a4b5c` must stay identical
in `Plugin.cs`, `build.yaml`, `configPage.html`, and `manifest.json`.

## Branches and CI

- `main` is the default, stable branch; `develop` is the integration branch.
- The **Build** workflow restores, builds (Release), and tests on every push
  and pull request to `main` or `develop`.

## Releasing

1. Make sure the change is on `main` and CI passes.
2. Update `version` in `build.yaml` for reference (the workflow does not read
   it).
3. Push a tag in the form `v<major>.<minor>.<build>.<revision>`, e.g.:

   ```bash
   git tag v1.1.4.0
   git push origin v1.1.4.0
   ```

The workflow then:

1. publishes the plugin with the tag as its version,
2. zips `Jellyfin.Plugin.BetterSubtitles.dll` as
   `better-subtitles_<version>.zip`,
3. adds a new entry (with MD5 checksum and `targetAbi` `12.0.0.0`) to the top
   of `manifest.json` and commits it to the default branch,
4. creates a GitHub release with the zip attached.

Do not edit release entries in `manifest.json` by hand.

## Targeting another Jellyfin version

Update the `Jellyfin.Controller` and `Jellyfin.Model` package versions in the
`.csproj` files, `targetAbi` in `build.yaml`, and the hardcoded `target_abi`
in `.github/workflows/build.yml`.

## Release history by Jellyfin version

| Releases | Jellyfin | .NET |
|----------|----------|------|
| `v1.0.0.0` and later | 12.x (`targetAbi` `12.0.0.0`) | net10.0 |
| `v0.0.1.0` – `v0.0.5.0` | 10.10.x (`targetAbi` `10.10.0.0`) | net8.0 |

## Ideas not implemented

These are not available in any release:

- Per-user or per-library preferred languages.
- Default keywords for localized "Signs & Songs" titles.
- An option to keep Jellyfin's own choice instead of turning subtitles off.
