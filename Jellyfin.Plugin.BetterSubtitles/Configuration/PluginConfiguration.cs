using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.BetterSubtitles.Configuration;

/// <summary>
/// Configuration for the Better Subtitles plugin.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Default keywords used to recognize forced "signs &amp; songs" style subtitle tracks
    /// when a release doesn't set the forced disposition flag correctly.
    /// </summary>
    public static readonly string[] DefaultForcedKeywords =
    [
        "forced",
        "signs",
        "songs",
        "signs & songs",
        "signs and songs",
        "sign",
        "song"
    ];

    /// <summary>
    /// Gets or sets a value indicating whether the plugin is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the comma-separated list of preferred subtitle language codes
    /// (ISO 639-2, e.g. "eng") used both to trust an explicitly forced track and to
    /// match against title keywords.
    /// </summary>
    public string PreferredLanguages { get; set; } = "eng";

    /// <summary>
    /// Gets or sets the newline-separated list of keywords/phrases (case-insensitive,
    /// matched as substrings) used to recognize forced "signs &amp; songs" subtitle
    /// tracks whose forced disposition flag is missing or incorrect.
    /// </summary>
    public string ForcedKeywords { get; set; } = string.Join('\n', DefaultForcedKeywords);
}
