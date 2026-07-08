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
    public void NormalizesKnownCodesToTwoLetterForm(string input, string expected)
    {
        Assert.Equal(expected, LanguageCodes.Normalize(input));
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
