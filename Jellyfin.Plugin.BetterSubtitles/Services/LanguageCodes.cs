using System;
using System.Collections.Generic;
using System.Globalization;

namespace Jellyfin.Plugin.BetterSubtitles.Services;

/// <summary>
/// Normalizes ISO 639 language codes so that 2-letter (ISO 639-1) and 3-letter
/// (ISO 639-2) forms of the same language - including the classic bibliographic
/// vs. terminology 3-letter variants (e.g. "ger" vs "deu") - compare as equal.
/// Media files frequently mix these forms, and an exact string comparison
/// silently fails to match an otherwise-correct forced subtitle track.
/// </summary>
public static class LanguageCodes
{
    // .NET's culture data doesn't consistently expose both the bibliographic (B)
    // and terminology (T) ISO 639-2 codes for a language, so the most common
    // divergent codes are supplemented by hand.
    private static readonly IReadOnlyDictionary<string, string> BibliographicAliases =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ger"] = "de",
            ["deu"] = "de",
            ["fre"] = "fr",
            ["fra"] = "fr",
            ["dut"] = "nl",
            ["nld"] = "nl",
            ["may"] = "ms",
            ["msa"] = "ms",
            ["bur"] = "my",
            ["mya"] = "my",
            ["rum"] = "ro",
            ["ron"] = "ro",
            ["chi"] = "zh",
            ["zho"] = "zh",
            ["per"] = "fa",
            ["fas"] = "fa",
            ["arm"] = "hy",
            ["hye"] = "hy",
            ["geo"] = "ka",
            ["kat"] = "ka",
            ["baq"] = "eu",
            ["eus"] = "eu",
            ["alb"] = "sq",
            ["sqi"] = "sq",
            ["mac"] = "mk",
            ["mkd"] = "mk"
        };

    private static readonly IReadOnlyDictionary<string, string> IsoToTwoLetter = BuildIsoLookup();

    /// <summary>
    /// Normalizes a language code to a canonical lowercase form (2-letter ISO
    /// 639-1 where known) so it can be compared against another normalized code.
    /// Unrecognized input is returned trimmed and lowercased, unchanged.
    /// </summary>
    /// <param name="code">A 2 or 3 letter ISO 639 language code, or any other string.</param>
    /// <returns>The normalized form of <paramref name="code"/>.</returns>
    public static string Normalize(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return string.Empty;
        }

        var trimmed = code.Trim();

        if (BibliographicAliases.TryGetValue(trimmed, out var alias))
        {
            return alias;
        }

        if (IsoToTwoLetter.TryGetValue(trimmed, out var twoLetter))
        {
            return twoLetter;
        }

        return trimmed.ToLowerInvariant();
    }

    private static IReadOnlyDictionary<string, string> BuildIsoLookup()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var culture in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
        {
            var twoLetter = culture.TwoLetterISOLanguageName;
            if (string.IsNullOrEmpty(twoLetter))
            {
                continue;
            }

            map.TryAdd(twoLetter, twoLetter);

            var threeLetter = culture.ThreeLetterISOLanguageName;
            if (!string.IsNullOrEmpty(threeLetter))
            {
                map.TryAdd(threeLetter, twoLetter);
            }
        }

        return map;
    }
}
