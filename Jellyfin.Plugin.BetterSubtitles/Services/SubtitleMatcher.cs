using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.BetterSubtitles.Services;

/// <summary>
/// Pure matching logic for picking the "forced" / "signs &amp; songs" subtitle track
/// out of a media source's subtitle streams. Has no dependency on Jellyfin's runtime
/// so it can be exercised directly by unit tests.
/// </summary>
public static class SubtitleMatcher
{
    /// <summary>
    /// Finds the index of the subtitle stream that best represents a forced /
    /// "signs &amp; songs" track, or <see langword="null"/> if none of the streams
    /// look like one.
    /// </summary>
    /// <param name="mediaStreams">All media streams for the playing media source.</param>
    /// <param name="preferredLanguages">Preferred subtitle language codes (e.g. "eng").</param>
    /// <param name="forcedKeywords">Keywords/phrases that identify a forced "signs &amp; songs" track by title.</param>
    /// <returns>The chosen subtitle stream index, or <see langword="null"/> if no candidate qualifies.</returns>
    public static int? FindBestIndex(
        IEnumerable<MediaStream> mediaStreams,
        IReadOnlyCollection<string> preferredLanguages,
        IReadOnlyCollection<string> forcedKeywords)
    {
        ArgumentNullException.ThrowIfNull(mediaStreams);
        ArgumentNullException.ThrowIfNull(preferredLanguages);
        ArgumentNullException.ThrowIfNull(forcedKeywords);

        var candidates = new List<(MediaStream Stream, int Score)>();

        foreach (var stream in mediaStreams)
        {
            if (stream.Type != MediaStreamType.Subtitle)
            {
                continue;
            }

            var languageMatches = LanguageCodes.IsUndefined(stream.Language)
                || preferredLanguages.Any(lang => LanguageCodes.Normalize(lang) == LanguageCodes.Normalize(stream.Language));
            var keywordMatches = MatchesKeyword(stream, forcedKeywords);

            int score;
            if (stream.IsForced && languageMatches && keywordMatches)
            {
                // Explicitly forced, right language, and titled like a forced track: highest confidence.
                score = 4;
            }
            else if (stream.IsForced && languageMatches)
            {
                // Jellyfin's own convention for a forced track: trust it even without a matching title.
                score = 3;
            }
            else if (keywordMatches && languageMatches)
            {
                // Not flagged forced, but the title gives it away and the language is right
                // (or untagged) - this is the common mistagged anime "Signs & Songs" case.
                score = 2;
            }
            else
            {
                // Looks like a full dialogue track - never auto-select it.
                continue;
            }

            candidates.Add((stream, score));
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        return candidates
            .OrderByDescending(c => c.Score)
            .ThenBy(c => c.Stream.Index)
            .First()
            .Stream.Index;
    }

    private static bool MatchesKeyword(MediaStream stream, IReadOnlyCollection<string> keywords)
    {
        var text = string.IsNullOrEmpty(stream.Title) ? stream.DisplayTitle : stream.Title;
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        foreach (var keyword in keywords)
        {
            if (!string.IsNullOrWhiteSpace(keyword) && text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
