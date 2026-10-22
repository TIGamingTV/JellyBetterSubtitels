# Troubleshooting

Start by checking the server log: **Dashboard → Logs**, then open the current
log file. The plugin's messages come from `ForcedSubtitleService`. See
[Configuration and behavior](configuration.md#verifying-the-result) for the
lines it writes.

## Better Subtitles is missing from the catalog

- Check that the repository URL was entered exactly as shown in the
  [README](../README.md#install-from-the-plugin-repository). It must be the
  `manifest.json` URL, not the GitHub page or a `.zip` download link.
- The current releases require **Jellyfin 12.x**. Jellyfin only lists
  plugin versions that are compatible with your server, so an older server
  shows nothing. See [Older Jellyfin servers](../README.md#older-jellyfin-servers).
- Make sure the Jellyfin server can reach `raw.githubusercontent.com` and
  `github.com` (firewall, proxy, or DNS restrictions).

## The plugin does not appear after installing

- Restart Jellyfin. Plugins are only loaded at server startup.
- For a manual install, check that `Jellyfin.Plugin.BetterSubtitles.dll` is in
  its own folder inside Jellyfin's `plugins` folder, and that the user running
  Jellyfin can read it.

## Subtitles are not selected automatically

Work through these checks in order:

1. **The plugin is enabled.** Open the plugin settings and confirm
   **Enabled** is checked.
2. **The file has a matching track.** In Jellyfin, open the subtitle menu of
   the item during playback and compare each track with the
   [selection rules](configuration.md#how-a-track-is-chosen). A track must be
   in a preferred language (or untagged) and either be flagged forced or have
   a keyword in its title.
3. **The language tag matches.** Add the track's language code to
   **Preferred languages** if it is missing.
4. **The title matches.** If the track's title uses a different word (for
   example a localized title), add that word to **Forced track keywords**.
5. **The client supports remote subtitle switching.** If the log shows no
   line from the plugin at all, the client may not support the command. The
   plugin skips such sessions and only notes it at `Debug` log level
   (`does not support SetSubtitleStreamIndex, skipping`). Try the same item
   in Jellyfin Web to compare.
6. **Errors.** Look for `Failed to auto-select a forced subtitle track`. The
   lines after it show the cause.

## Subtitles are turned off when I expected a track

This is expected when no track qualifies: the plugin deliberately turns
subtitles off rather than keep a track Jellyfin chose by itself. Check the
track against the [selection rules](configuration.md#how-a-track-is-chosen).
If you do not want this behavior at all, disable the plugin.

## The wrong track is selected

- A full dialogue track whose title contains a keyword is treated as forced.
  Remove broad keywords such as `sign` or `song`, or use more specific
  phrases.
- When several tracks have the same rank, the one listed first wins. Tracks
  flagged forced in the file always beat tracks matched by title only.

## 2-letter and 3-letter language codes do not match

The plugin uses the server's language data to treat codes like `en` and `eng`
as equal. If the server runs without that data (.NET "invariant
globalization" mode, for example in some minimal containers), this
conversion only works for the built-in pairs such as `ger`/`deu`. Use exactly
the same code form that your files use in **Preferred languages**, for example
`eng` rather than `en`.

## Settings do not save

Version 1.1.3.0 fixed a bug where **Save** on the settings page did nothing.
Update the plugin to the latest version. If saving still fails, the page shows
"Better Subtitles: failed to load or save settings"; check the browser console
and the server log for details.
