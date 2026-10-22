<p align="center">
  <img src="JellyBetterSubtitels.png" alt="JellyBetterSubtitels" width="400">
</p>

# JellyBetterSubtitels

**Better Subtitles** is a Jellyfin server plugin that automatically turns on
the forced English subtitle track — or, for anime, the "Signs & Songs" track —
when playback starts. If no such track exists, it turns subtitles off.

It is for people who watch foreign-language or anime releases where the
forced track is not flagged correctly, so Jellyfin's own subtitle selection
misses it and you have to pick the track by hand every episode.

## Features

- **Finds forced tracks reliably**: uses the track's "forced" flag *and* its
  title (e.g. `Signs & Songs`), so mislabeled tracks are still found.
- **Never shows full dialogue subtitles by accident**: if no forced track
  qualifies, subtitles are switched off, overriding Jellyfin's own pick.
- **Configurable**: choose your subtitle languages and the title keywords
  that identify forced tracks.
- **Flexible language codes**: `en`, `eng`, and variants like `ger`/`deu` are
  treated as the same language.
- **Server-side**: the server tells the client which track to show, so it
  works the same in every client that supports remote subtitle switching,
  such as Jellyfin Web. Your media files are never modified.

## Requirements

- Jellyfin server **12.x** (for 10.10.x, see
  [Older Jellyfin servers](#older-jellyfin-servers)).
- Administrator access to the Jellyfin Dashboard.
- For installation from the plugin repository: network access from the
  server to `raw.githubusercontent.com` and `github.com`.

## Installation

### Install from the plugin repository

1. In Jellyfin, open **Dashboard → Plugins → Repositories** and add a new
   repository. (Menu labels can differ slightly between Jellyfin versions.)
2. Enter any name, e.g. `Better Subtitles`, and this repository URL:

   ```text
   https://raw.githubusercontent.com/TIGamingTV/JellyBetterSubtitels/main/manifest.json
   ```

3. Save, then open the plugin **Catalog**, select **Better Subtitles**, and
   install it.
4. Restart Jellyfin.
5. Check that **Better Subtitles** appears in **Dashboard → Plugins** as
   active.

### Manual installation

1. Download the latest `better-subtitles_<version>.zip` from the
   [Releases page](https://github.com/TIGamingTV/JellyBetterSubtitels/releases).
2. Create a folder such as `Better Subtitles` inside Jellyfin's `plugins`
   folder (e.g. `/var/lib/jellyfin/plugins/` on most Linux installs; the
   `plugins` folder is in Jellyfin's data directory).
3. Extract `Jellyfin.Plugin.BetterSubtitles.dll` into that folder.
4. Restart Jellyfin.

### Older Jellyfin servers

Releases `v0.0.1.0` to `v0.0.5.0` are built for Jellyfin **10.10.x**. They are
not in the repository manifest; install
[v0.0.5.0](https://github.com/TIGamingTV/JellyBetterSubtitels/releases/tag/v0.0.5.0)
manually as described above. These versions do not receive fixes made in
later releases.

## Quick start

The defaults work for English forced and "Signs & Songs" subtitles, so no
configuration is needed.

1. Play a movie or episode that has a forced or "Signs & Songs" subtitle
   track.
2. After about half a second, that subtitle track turns on by itself.
3. Play something without such a track: subtitles are turned off.
4. To confirm, open **Dashboard → Logs**. The current log contains a line
   like:

   ```text
   Selected forced subtitle stream <index> for "<item name>" on session <session id> (<client>)
   ```

   No line is written when the client already had the right track selected.

## Configuration

Open **Dashboard → Plugins → Better Subtitles**, change the settings, and
click **Save**. Changes apply to the next playback; no restart is needed.

| Setting | Default | Purpose |
|---------|---------|---------|
| **Enabled** | On | Turns automatic selection on or off. |
| **Preferred languages** | `eng` | Comma-separated language codes, e.g. `eng, ger`. Only tracks in these languages (or with no language tag) are selected. |
| **Forced track keywords** | `forced`, `signs`, `songs`, `signs & songs`, `signs and songs`, `sign`, `song` | One per line. A track whose title contains one of these is treated as forced. |

Settings apply to all users. See
[Configuration and behavior](docs/configuration.md) for the exact selection
rules, examples, and limitations.

## Updating and uninstalling

- **Update**: new versions are published to the same repository URL and are
  updated like any other Jellyfin plugin. Restart Jellyfin after updating.
  Settings are kept. For manual installs, replace the DLL and restart.
- **Uninstall**: in **Dashboard → Plugins**, open **Better Subtitles**,
  uninstall it, and restart Jellyfin.

## Documentation

- [Configuration and behavior](docs/configuration.md): all settings, how a
  track is chosen, log messages, limitations.
- [Troubleshooting](docs/troubleshooting.md): common problems and fixes.
- [Development](docs/development.md): building, testing, and releasing.

## License

[MIT](LICENSE)
