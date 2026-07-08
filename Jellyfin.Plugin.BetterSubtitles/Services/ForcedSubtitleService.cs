using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Session;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.BetterSubtitles.Services;

/// <summary>
/// Watches for playback starting on any session and, when a forced / "signs &amp; songs"
/// subtitle track is found, commands the client to select it - regardless of whether the
/// track's forced disposition flag or language tag is actually correct.
/// </summary>
public class ForcedSubtitleService : IHostedService
{
    private static readonly TimeSpan NegotiationDelay = TimeSpan.FromMilliseconds(500);

    private readonly ISessionManager _sessionManager;
    private readonly ILogger<ForcedSubtitleService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ForcedSubtitleService"/> class.
    /// </summary>
    /// <param name="sessionManager">Instance of the <see cref="ISessionManager"/> interface.</param>
    /// <param name="logger">Instance of the <see cref="ILogger{ForcedSubtitleService}"/> interface.</param>
    public ForcedSubtitleService(ISessionManager sessionManager, ILogger<ForcedSubtitleService> logger)
    {
        _sessionManager = sessionManager;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _sessionManager.PlaybackStart += OnPlaybackStart;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _sessionManager.PlaybackStart -= OnPlaybackStart;
        return Task.CompletedTask;
    }

    private void OnPlaybackStart(object? sender, PlaybackProgressEventArgs e)
    {
        // The event itself is synchronous; run the actual work in the background so we
        // never block Jellyfin's session manager on our (deliberate) negotiation delay.
        _ = HandlePlaybackStartAsync(e);
    }

    private async Task HandlePlaybackStartAsync(PlaybackProgressEventArgs e)
    {
        try
        {
            var config = Plugin.Instance?.Configuration;
            if (config is null || !config.Enabled)
            {
                return;
            }

            var session = e.Session;
            var mediaStreams = e.MediaInfo?.MediaStreams;
            if (session is null || mediaStreams is null)
            {
                return;
            }

            var preferredLanguages = SplitConfigValue(config.PreferredLanguages);
            var forcedKeywords = SplitConfigValue(config.ForcedKeywords);

            var bestIndex = SubtitleMatcher.FindBestIndex(mediaStreams, preferredLanguages, forcedKeywords);
            if (bestIndex is null)
            {
                _logger.LogDebug("No forced/signs & songs subtitle candidate for {ItemName}", e.MediaInfo?.Name);
                return;
            }

            if (session.PlayState?.SubtitleStreamIndex == bestIndex)
            {
                return;
            }

            if (!session.SupportedCommands.Contains(GeneralCommandType.SetSubtitleStreamIndex))
            {
                _logger.LogDebug(
                    "Session {SessionId} ({Client}) does not support SetSubtitleStreamIndex, skipping",
                    session.Id,
                    session.Client);
                return;
            }

            // Some clients haven't finished their own initial stream negotiation the instant
            // PlaybackStart fires; give it a moment before overriding the selection.
            await Task.Delay(NegotiationDelay, CancellationToken.None).ConfigureAwait(false);

            var command = new GeneralCommand
            {
                Name = GeneralCommandType.SetSubtitleStreamIndex
            };
            command.Arguments["Index"] = bestIndex.Value.ToString(CultureInfo.InvariantCulture);

            await _sessionManager.SendGeneralCommand(session.Id, session.Id, command, CancellationToken.None)
                .ConfigureAwait(false);

            _logger.LogInformation(
                "Selected forced subtitle stream {Index} for \"{ItemName}\" on session {SessionId} ({Client})",
                bestIndex,
                e.MediaInfo?.Name,
                session.Id,
                session.Client);
        }
        catch (Exception ex)
        {
            // This runs detached from the PlaybackStart event, so an unhandled exception here
            // would otherwise vanish silently instead of surfacing in the server log.
            _logger.LogError(ex, "Failed to auto-select a forced subtitle track");
        }
    }

    private static string[] SplitConfigValue(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
