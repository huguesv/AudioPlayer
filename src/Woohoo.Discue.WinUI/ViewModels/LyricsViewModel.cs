// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.ViewModels;

using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Woohoo.Audio.Core.Lyrics;
using Woohoo.Audio.Core.Playback;
using Woohoo.Audio.Services;

public sealed partial class LyricsViewModel : ObservableObject
{
    private readonly IMediaPlayerService mediaPlayerService;
    private readonly ILocalSettingsService localSettingsService;
    private readonly ILogger logger;
    private readonly IDispatcherQueue dispatcherQueue;
    private LyricsTrack? lyric;
    private bool autoScroll;

    public LyricsViewModel(
        IDispatcherQueueService dispatcherQueueService,
        IMediaPlayerService mediaPlayerService,
        ILocalSettingsService localSettingsService,
        ILogger<LyricsViewModel> logger)
    {
        ArgumentNullException.ThrowIfNull(dispatcherQueueService);
        ArgumentNullException.ThrowIfNull(mediaPlayerService);
        ArgumentNullException.ThrowIfNull(localSettingsService);
        ArgumentNullException.ThrowIfNull(logger);

        this.mediaPlayerService = mediaPlayerService;
        this.localSettingsService = localSettingsService;
        this.logger = logger;
        this.dispatcherQueue = dispatcherQueueService.GetDispatcherQueue();

        this.Lines = [];

        this.mediaPlayerService.ActiveTrackChanged += this.MediaPlayerService_ActiveTrackChanged;
        this.mediaPlayerService.LyricsUpdated += this.MediaPlayerService_LyricsUpdated;
        this.mediaPlayerService.PlaybackPositionChanged += this.MediaPlayerService_PlaybackPositionChanged;

        this.autoScroll = this.localSettingsService.ReadSetting<bool?>(KnownSettingKeys.LyricsAutoScroll) ?? true;

        this.localSettingsService.SettingChanged += this.LocalSettingsService_SettingChanged;

        var activeTrack = this.mediaPlayerService.GetActiveTrack();
        var lyrics = activeTrack is not null ? this.mediaPlayerService.GetTrackLyrics(activeTrack.Id) : null;
        this.Load(lyrics);
    }

    private void LocalSettingsService_SettingChanged(object? sender, SettingChangedEventArgs e)
    {
        if (e.SettingKey == KnownSettingKeys.LyricsAutoScroll)
        {
            this.autoScroll = this.localSettingsService.ReadSetting<bool?>(KnownSettingKeys.LyricsAutoScroll) ?? true;
        }
    }

    [ObservableProperty]
    public partial ObservableCollection<LyricsLineViewModel> Lines { get; set; } = [];

    [ObservableProperty]
    public partial bool HasLyrics { get; set; }

    private void MediaPlayerService_ActiveTrackChanged(object? sender, EventArgs e)
    {
        _ = this.dispatcherQueue.TryEnqueue(() =>
        {
            var activeTrack = this.mediaPlayerService.GetActiveTrack();
            var lyrics = activeTrack is not null ? this.mediaPlayerService.GetTrackLyrics(activeTrack.Id) : null;
            this.Load(lyrics);
            this.UpdateCurrentLyric();
        });
    }

    private void MediaPlayerService_LyricsUpdated(object? sender, AudioPlayerTrackEventArgs e)
    {
        _ = this.dispatcherQueue.TryEnqueue(() =>
        {
            if (e.Track.Id == this.mediaPlayerService.GetActiveTrack()?.Id)
            {
                var lyrics = e.Track is not null ? this.mediaPlayerService.GetTrackLyrics(e.Track.Id) : null;
                this.Load(lyrics);
                this.UpdateCurrentLyric();
            }
        });
    }

    private void MediaPlayerService_PlaybackPositionChanged(object? sender, EventArgs e)
    {
        _ = this.dispatcherQueue.TryEnqueue(() =>
        {
            this.UpdateCurrentLyric();
        });
    }

    private void Load(LyricsTrack? lyrics)
    {
        this.lyric = lyrics;

        // WinRT sometimes throws COM Exception with no details if clearing
        // the lines collection that is bound to the UI.
        // Trying a workaround by replacing the Lines property value with a whole new collection.
        var lines = new ObservableCollection<LyricsLineViewModel>();

        if (lyrics is not null)
        {
            if (lyrics.SyncedLines.Length > 0)
            {
                foreach (var line in lyrics.SyncedLines)
                {
                    lines.Add(new LyricsLineViewModel
                    {
                        Timestamp = line.Timestamp,
                        Text = line.Text,
                    });
                }
            }
            else if (lyrics.PlainText.Length > 0)
            {
                foreach (var line in lyrics.PlainText.Split(new[] { '\r', '\n' }, StringSplitOptions.TrimEntries))
                {
                    lines.Add(new LyricsLineViewModel
                    {
                        Timestamp = TimeSpan.Zero,
                        Text = line,
                    });
                }
            }
        }

        this.Lines = lines;

        this.HasLyrics = this.Lines.Count > 0;
    }

    private void UpdateCurrentLyric()
    {
        if (this.lyric is null)
        {
            return;
        }

        var previous = this.Lines.FirstOrDefault(l => l.IsCurrent);

        var currentLyricLineIndex = this.lyric.GetLineIndexAt(this.mediaPlayerService.PlaybackPosition);
        for (int i = 0; i < this.Lines.Count; i++)
        {
            var lineViewModel = this.Lines[i];
            lineViewModel.IsCurrent = i == currentLyricLineIndex;
        }

        if (currentLyricLineIndex >= 0)
        {
            var updated = this.Lines[currentLyricLineIndex];
            if (updated != previous)
            {
                var message = new CurrentLyricChangeMessage
                {
                    Line = updated,
                    Index = currentLyricLineIndex,
                    AutoScroll = this.autoScroll,
                };

                WeakReferenceMessenger.Default.Send(message);
            }
        }
    }
}
