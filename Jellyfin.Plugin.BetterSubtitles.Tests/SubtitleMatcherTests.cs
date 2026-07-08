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
}
