# Progress

Status tracking for **JellyBetterSubtitels** (Better Subtitles Jellyfin plugin).

_Last updated: 2026-07-12_

## Current state

The plugin is **functional and released** (latest published version `0.0.4.0`,
targeting the Jellyfin 10.10.x plugin ABI). Core auto-selection, configuration,
unit tests, and a CI/release pipeline are all in place.

## Done

- [x] Core plugin scaffolding — `Plugin`, `PluginServiceRegistrator`, `build.yaml`.
- [x] `ForcedSubtitleService` — hooks `PlaybackStart`, sends
      `SetSubtitleStreamIndex`, forces subtitles off when nothing qualifies,
      with a client-negotiation delay and detached error handling.
- [x] `SubtitleMatcher` — pure, scored track-selection logic (forced flag +
      language + title-keyword confidence tiers).
- [x] `LanguageCodes` — ISO 639-1/-2 normalization including bibliographic vs.
      terminology variants and `und`/blank handling.
- [x] Configurable settings: enable toggle, preferred languages, forced-track
      keywords, with a dashboard config page.
- [x] Unit tests for `SubtitleMatcher` and `LanguageCodes`.
- [x] GitHub Actions CI: build + test on `main`/`develop`; tag-triggered
      publish, GitHub release, and automatic `manifest.json` update.
- [x] Plugin-repository `manifest.json` with published release entries
      (0.0.1.0 → 0.0.4.0).
- [x] README with install, configuration, build, and verification docs.
- [x] LICENSE.
- [x] Fix: enforce English-only keyword match; force subtitles off when nothing
      qualifies (v0.0.3.0 era).
- [x] Fix: language-code matching gaps in forced-subtitle selection (v0.0.4.0).
- [x] Project documentation: `CLAUDE.md` and this `progress.md`.

## Not yet / possible future work

- [ ] Per-user or per-library preferred-language overrides (currently global).
- [ ] Broaden the default forced-keyword list for non-English/localized
      "Signs & Songs" titles.
- [ ] Optional "prefer default flag" or "leave Jellyfin's pick alone" mode for
      users who don't want the hard subtitles-off override.
- [ ] Additional test coverage for edge cases (multiple equal-scoring tracks,
      unusual language tags, empty/whitespace titles).
- [ ] Validate/expand the `targetAbi` as newer Jellyfin server versions land.

## Notes

- Releases are cut by pushing a `v*` tag — CI handles packaging, the release,
  and the manifest entry. Don't hand-edit release entries in `manifest.json`.
- Keep the plugin `Guid` consistent across `Plugin.cs`, `build.yaml`, and
  `manifest.json`.
