using System.Collections.Generic;
using Jellyfin.Plugin.BetterSubtitles.Services;
using MediaBrowser.Model.Entities;
using Xunit;

namespace Jellyfin.Plugin.BetterSubtitles.Tests;

public class SubtitleMatcherTests
{
    private static readonly string[] PreferredLanguages = ["eng"];
    private static readonly string[] ForcedKeywords = ["forced", "signs", "songs", "signs & songs"];

    private static MediaStream Subtitle(int index, bool isForced = false, string? language = null, string? title = null)
    {
        return new MediaStream
        {
            Index = index,
            Type = MediaStreamType.Subtitle,
            IsForced = isForced,
            Language = language!,
            Title = title!
        };
    }

    [Fact]
    public void PicksCorrectlyTaggedForcedEnglishTrack()
    {
        var streams = new List<MediaStream>
        {
            Subtitle(2, isForced: false, language: "eng", title: "Full Subtitles"),
            Subtitle(3, isForced: true, language: "eng", title: "Forced")
        };

        var result = SubtitleMatcher.FindBestIndex(streams, PreferredLanguages, ForcedKeywords);

        Assert.Equal(3, result);
    }

    [Fact]
    public void PicksMistaggedSignsAndSongsTrackByTitleAlone()
    {
        var streams = new List<MediaStream>
        {
            Subtitle(2, isForced: false, language: "eng", title: "Full Subtitles"),
            Subtitle(4, isForced: false, language: null, title: "Signs & Songs")
        };

        var result = SubtitleMatcher.FindBestIndex(streams, PreferredLanguages, ForcedKeywords);

        Assert.Equal(4, result);
    }

    [Fact]
    public void IgnoresKeywordTitledTrackInNonPreferredLanguage()
    {
        // A "Signs & Songs" style title in a non-preferred language must NOT be selected -
        // the keyword rule only applies to preferred-language (or untagged) tracks.
        var streams = new List<MediaStream>
        {
            Subtitle(2, isForced: false, language: "eng", title: "Full Subtitles"),
            Subtitle(3, isForced: false, language: "ita", title: "Signs & Songs")
        };

        var result = SubtitleMatcher.FindBestIndex(streams, PreferredLanguages, ForcedKeywords);

        Assert.Null(result);
    }

    [Fact]
    public void NeverPicksAFullDialogueTrack()
    {
        var streams = new List<MediaStream>
        {
            Subtitle(2, isForced: false, language: "eng", title: "Full Subtitles"),
            Subtitle(3, isForced: false, language: "jpn", title: "Japanese")
        };

        var result = SubtitleMatcher.FindBestIndex(streams, PreferredLanguages, ForcedKeywords);

        Assert.Null(result);
    }

    [Fact]
    public void ReturnsNullWhenNoSubtitleStreamsPresent()
    {
        var streams = new List<MediaStream>
        {
            new() { Index = 0, Type = MediaStreamType.Video },
            new() { Index = 1, Type = MediaStreamType.Audio }
        };

        var result = SubtitleMatcher.FindBestIndex(streams, PreferredLanguages, ForcedKeywords);

        Assert.Null(result);
    }

    [Fact]
    public void IgnoresForcedTrackWithNonPreferredLanguageUnlessTitleMatches()
    {
        var streams = new List<MediaStream>
        {
            Subtitle(5, isForced: true, language: "spa", title: "Forzado")
        };

        // Wrong language, but title still matches a keyword (translated "forced") -> not configured, so no match.
        var result = SubtitleMatcher.FindBestIndex(streams, PreferredLanguages, ForcedKeywords);

        Assert.Null(result);
    }

    [Fact]
    public void PrefersHigherConfidenceMatchOverLowerConfidenceOne()
    {
        var streams = new List<MediaStream>
        {
            Subtitle(6, isForced: false, language: null, title: "Signs"),
            Subtitle(7, isForced: true, language: "eng", title: "Signs & Songs")
        };

        var result = SubtitleMatcher.FindBestIndex(streams, PreferredLanguages, ForcedKeywords);

        Assert.Equal(7, result);
    }

    [Fact]
    public void BreaksTiesByLowestIndex()
    {
        var streams = new List<MediaStream>
        {
            Subtitle(9, isForced: true, language: "eng", title: "Forced"),
            Subtitle(8, isForced: true, language: "eng", title: "Forced")
        };

        var result = SubtitleMatcher.FindBestIndex(streams, PreferredLanguages, ForcedKeywords);

        Assert.Equal(8, result);
    }

    [Fact]
    public void PicksForcedTrackTaggedWithTwoLetterLanguageCode()
    {
        // Regression: a plain forced track with no keyword-matching title, tagged with the
        // 2-letter ISO 639-1 code "en" instead of the 3-letter "eng" the default config uses,
        // used to be rejected outright by an exact string comparison.
        var streams = new List<MediaStream>
        {
            Subtitle(1, isForced: false, language: "eng", title: "Full Subtitles"),
            Subtitle(2, isForced: true, language: "en", title: null)
        };

        var result = SubtitleMatcher.FindBestIndex(streams, PreferredLanguages, ForcedKeywords);

        Assert.Equal(2, result);
    }

    [Fact]
    public void PicksForcedTrackWhenPreferredLanguageIsTwoLetterAndTrackIsThreeLetter()
    {
        var streams = new List<MediaStream>
        {
            Subtitle(3, isForced: true, language: "eng", title: null)
        };

        var result = SubtitleMatcher.FindBestIndex(streams, new[] { "en" }, ForcedKeywords);

        Assert.Equal(3, result);
    }

    [Fact]
    public void PicksForcedTrackUsingBibliographicLanguageAlias()
    {
        // "ger" (ISO 639-2/B) and "deu" (ISO 639-2/T) both mean German.
        var streams = new List<MediaStream>
        {
            Subtitle(4, isForced: true, language: "ger", title: null)
        };

        var result = SubtitleMatcher.FindBestIndex(streams, new[] { "deu" }, ForcedKeywords);

        Assert.Equal(4, result);
    }
}
