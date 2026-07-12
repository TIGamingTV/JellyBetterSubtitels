using Jellyfin.Plugin.BetterSubtitles.Services;
using Xunit;

namespace Jellyfin.Plugin.BetterSubtitles.Tests;

public class LanguageCodesTests
{
    [Theory]
    [InlineData("en", "en")]
    [InlineData("EN", "en")]
    [InlineData("eng", "en")]
    [InlineData("ENG", "en")]
    [InlineData(" eng ", "en")]
    [InlineData("ger", "de")]
    [InlineData("deu", "de")]
    [InlineData("fre", "fr")]
    [InlineData("fra", "fr")]
    [InlineData("jpn", "ja")]
    // ISO 639-2/B bibliographic codes that .NET's culture data does not resolve,
    // paired here with their /T terminology twins that it does.
    [InlineData("cze", "cs")]
    [InlineData("ces", "cs")]
    [InlineData("gre", "el")]
    [InlineData("ell", "el")]
    [InlineData("ice", "is")]
    [InlineData("isl", "is")]
    [InlineData("slo", "sk")]
    [InlineData("slk", "sk")]
    [InlineData("wel", "cy")]
    [InlineData("cym", "cy")]
    public void NormalizesKnownCodesToTwoLetterForm(string input, string expected)
    {
        Assert.Equal(expected, LanguageCodes.Normalize(input));
    }

    [Theory]
    [InlineData("cze", "ces")]
    [InlineData("gre", "ell")]
    [InlineData("ice", "isl")]
    [InlineData("slo", "slk")]
    [InlineData("wel", "cym")]
    public void BibliographicAndTerminologyCodesNormalizeEqually(string biblio, string terminology)
    {
        Assert.Equal(LanguageCodes.Normalize(terminology), LanguageCodes.Normalize(biblio));
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("   ", true)]
    [InlineData("und", true)]
    [InlineData("UND", true)]
    [InlineData(" und ", true)]
    [InlineData("eng", false)]
    [InlineData("en", false)]
    public void IsUndefinedRecognizesBlankAndUndCodes(string? code, bool expected)
    {
        Assert.Equal(expected, LanguageCodes.IsUndefined(code));
    }

    [Fact]
    public void ReturnsEmptyForBlankInput()
    {
        Assert.Equal(string.Empty, LanguageCodes.Normalize(null));
        Assert.Equal(string.Empty, LanguageCodes.Normalize(string.Empty));
        Assert.Equal(string.Empty, LanguageCodes.Normalize("   "));
    }

    [Fact]
    public void PassesThroughUnrecognizedCodeLowercased()
    {
        Assert.Equal("xyz", LanguageCodes.Normalize("XYZ"));
    }
}
