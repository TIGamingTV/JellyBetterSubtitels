# Configuration and behavior

This guide covers every setting of the **Better Subtitles** plugin and exactly
how it decides which subtitle track to show. For installation, see the
[README](../README.md).

## Where to find the settings

1. Sign in to Jellyfin as an administrator.
2. Open **Dashboard → Plugins** and select **Better Subtitles** to open its
   settings page.
3. Change the settings and click **Save**.

Saved settings apply to the **next** playback that starts. A Jellyfin restart
is not required after saving.

The settings are global: they apply to every user, library, and client on the
server.

## Settings

| Setting | Default | Accepted values |
|---------|---------|-----------------|
| **Enabled** | On | On / Off |
| **Preferred languages** | `eng` | Language codes separated by commas (or line breaks) |
| **Forced track keywords** | `forced`, `signs`, `songs`, `signs & songs`, `signs and songs`, `sign`, `song` | One keyword or phrase per line (commas also separate entries) |

### Enabled

Turns the plugin on or off. When it is off, the plugin does nothing and
Jellyfin's own subtitle selection applies unchanged.

### Preferred languages

The subtitle languages you want forced / "Signs & Songs" tracks in. A track
can only be auto-selected when its language tag matches one of these codes,
**or** when the track has no language tag (blank or `und`).

- Both 2-letter (ISO 639-1, e.g. `en`) and 3-letter (ISO 639-2, e.g. `eng`)
  codes are accepted, and are compared in a normalized form. `en` matches a
  track tagged `eng` and vice versa.
- Both 3-letter variants of languages that have two are treated as equal, e.g.
  `ger`/`deu`, `fre`/`fra`, `chi`/`zho`, `cze`/`ces`, `dut`/`nld`.
- Matching is case-insensitive and ignores surrounding spaces.
- Example for English and German: `eng, ger`

If you leave this field empty, only tracks without a language tag can match.

### Forced track keywords

Words or phrases that identify a forced / "Signs & Songs" track by its
**title**. They catch tracks that are missing the "forced" flag in the file.

- Matching is case-insensitive and checks whether the keyword appears
  **anywhere** in the track title (substring match). `sign` therefore matches
  `Signs`, `Signs & Songs`, and `English [Signs]`.
- The track's title is used; if it has none, Jellyfin's display title is used.
- A comma also separates entries, so a keyword cannot contain a comma.
- To restore the defaults, enter them again one per line:

  ```text
  forced
  signs
  songs
  signs & songs
  signs and songs
  sign
  song
  ```

If you leave this field empty, only tracks flagged "forced" in the file can be
selected.

## How a track is chosen

When playback of an item starts, the plugin rates every subtitle track that
Jellyfin lists for the item:

| Rank | The track is… |
|------|---------------|
| 1 (best) | flagged forced, **and** in a preferred language, **and** its title contains a keyword |
| 2 | flagged forced **and** in a preferred language |
| 3 | not flagged forced, but its title contains a keyword **and** it is in a preferred language |
| — | anything else, e.g. a full dialogue track or a keyword track in another language: never selected |

"In a preferred language" includes tracks with no language tag.

- The best-ranked track wins. If several tracks share the best rank, the one
  listed first (lowest stream index) wins.
- If **no** track qualifies, the plugin turns subtitles **off**, even if
  Jellyfin had already chosen a subtitle track by itself.
- If the client already shows the chosen track (or already has subtitles off
  when that is the result), the plugin sends nothing.

### Example

An anime episode with Japanese audio and these subtitle tracks, using the
default settings:

| Index | Title | Language | Forced flag | Result |
|-------|-------|----------|-------------|--------|
| 2 | `Full Subtitles` | `eng` | no | not eligible (no keyword, not forced) |
| 3 | `Signs & Songs` | `eng` | no | rank 3 |
| 4 | `Signs & Songs` | `ger` | no | not eligible (wrong language) |

Track 3 is selected automatically. If track 3 were missing, subtitles would
be turned off.

## What happens during playback

1. A client starts playing an item.
2. About half a second later, the plugin sends the client a remote-control
   command to switch to the chosen subtitle track (or to turn subtitles off).
   The short wait lets the client finish setting up playback first.
3. The client switches tracks. Nothing is written to your media files.
4. The server log records an `Information` entry. See
   [Verifying the result](#verifying-the-result).

The plugin acts **once per playback start**. If you change subtitles manually
afterwards, the plugin does not change them back. It runs again when the next
item starts, e.g. the next episode.

## Verifying the result

Open **Dashboard → Logs** and open the current server log. Each time the
plugin changes the selection, it writes one of these lines:

```text
Selected forced subtitle stream <index> for "<item name>" on session <session id> (<client>)
No forced/signs & songs subtitle for "<item name>"; disabled subtitles on session <session id> (<client>)
```

No line is written when the client was already showing the right track, or
when the plugin skipped the session. See
[Troubleshooting](troubleshooting.md) if nothing happens.

## Limitations

- **Clients must support remote subtitle switching.** The plugin only acts on
  sessions whose client reports support for the "set subtitle stream" command.
  Other clients are skipped silently (logged at `Debug` level only).
- **No per-user or per-library settings.** One configuration applies to the
  whole server.
- **Subtitles-off override cannot be disabled.** When no track qualifies, the
  plugin always turns subtitles off. To keep Jellyfin's own choice, disable
  the plugin.
- **Substring matching can be too broad.** A full dialogue track whose title
  contains a keyword (e.g. `Dialogue + Songs` contains `song`) is treated as a
  forced track. Remove or narrow keywords if this happens with your files.
- **Only the subtitle language and title are examined.** The audio language
  is not taken into account.
