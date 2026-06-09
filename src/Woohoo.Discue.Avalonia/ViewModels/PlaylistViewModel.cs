// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Avalonia.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Woohoo.Audio.Core.Playback;
using Woohoo.Audio.Services;

public sealed partial class PlaylistViewModel : ObservableObject
{
    private readonly IMediaPlayerService mediaPlayerService;
    private readonly ILogger logger;
    private readonly IDispatcherQueue dispatcherQueue;

    public PlaylistViewModel(
        IDispatcherQueueService dispatcherQueueService,
        IMediaPlayerService mediaPlayerService,
        ILogger<PlaylistViewModel> logger)
    {
        ArgumentNullException.ThrowIfNull(dispatcherQueueService);
        ArgumentNullException.ThrowIfNull(mediaPlayerService);
        ArgumentNullException.ThrowIfNull(logger);

        this.mediaPlayerService = mediaPlayerService;
        this.logger = logger;
        this.dispatcherQueue = dispatcherQueueService.GetDispatcherQueue();

        this.Items = [];

        this.mediaPlayerService.ActiveTrackChanged += this.MediaPlayerService_ActiveTrackChanged;
        this.mediaPlayerService.LyricsUpdated += this.MediaPlayerService_LyricsUpdated;
        this.mediaPlayerService.MetadataUpdated += this.MediaPlayerService_MetadataUpdated;
        this.mediaPlayerService.PlaylistUpdated += this.MediaPlayerService_PlaylistUpdated;

        this.Load(this.mediaPlayerService.PlaylistTracks, this.mediaPlayerService.GetActiveTrack()?.Id);
    }

    public ObservableCollection<PlaylistItemViewModel> Items { get; }

    [ObservableProperty]
    public partial bool HasItems { get; set; }

    private void MediaPlayerService_ActiveTrackChanged(object? sender, EventArgs e)
    {
        _ = this.dispatcherQueue.TryEnqueue(() =>
        {
            var activeTrack = this.mediaPlayerService.GetActiveTrack();
            if (activeTrack is not null)
            {
                this.SetActive(activeTrack.Id);
            }
        });
    }

    private void MediaPlayerService_LyricsUpdated(object? sender, AudioPlayerTrackEventArgs e)
    {
        _ = this.dispatcherQueue.TryEnqueue(() =>
        {
            var playlistItemViewModel = this.Items.FirstOrDefault(t => t.TrackId == e.Track.Id);
            if (playlistItemViewModel is not null)
            {
                var lyrics = this.mediaPlayerService.GetTrackLyrics(e.Track.Id);
                playlistItemViewModel.Lyrics = lyrics;
            }
        });
    }

    private void MediaPlayerService_MetadataUpdated(object? sender, AudioPlayerTrackEventArgs e)
    {
        _ = this.dispatcherQueue.TryEnqueue(() =>
        {
            var disc = this.mediaPlayerService.FindDisc(e.Track.DiscId);

            var playlistItemViewModel = this.Items.FirstOrDefault(t => t.TrackId == e.Track.Id);
            if (playlistItemViewModel is not null)
            {
                var trackMetadata = this.mediaPlayerService.GetTrackMetadata(e.Track.Id);
                var discMetadata = this.mediaPlayerService.GetDiscMetadata(e.Track.DiscId);

                playlistItemViewModel.TrackTitle = trackMetadata?.TrackTitle ?? string.Empty;
                playlistItemViewModel.TrackPerformer = trackMetadata?.TrackPerformer ?? string.Empty;
                playlistItemViewModel.AlbumTitle = trackMetadata?.AlbumTitle ?? string.Empty;
                playlistItemViewModel.AlbumPerformer = trackMetadata?.AlbumPerformer ?? string.Empty;
                playlistItemViewModel.Year = discMetadata?.Year ?? string.Empty;
            }
        });
    }

    private void MediaPlayerService_PlaylistUpdated(object? sender, EventArgs e)
    {
        _ = this.dispatcherQueue.TryEnqueue(() =>
        {
            this.Load(this.mediaPlayerService.PlaylistTracks, this.mediaPlayerService.GetActiveTrack()?.Id);
        });
    }

    private void Load(IReadOnlyList<AudioPlayerTrack> playlistTracks, Guid? activeTrackId)
    {
        this.Items.Clear();

        foreach (var track in playlistTracks)
        {
            var trackMetadata = this.mediaPlayerService.GetTrackMetadata(track.Id);
            var discMetadata = this.mediaPlayerService.GetDiscMetadata(track.DiscId);

            string trackTitle = !string.IsNullOrEmpty(trackMetadata?.TrackTitle)
                ? trackMetadata.TrackTitle
                : $"Track {track.TrackNumber:00}";

            var lyrics = this.mediaPlayerService.GetTrackLyrics(track.Id);

            var viewModel = new PlaylistItemViewModel(this.logger)
            {
                IsActive = activeTrackId.HasValue && track.Id == activeTrackId.Value,
                TrackId = track.Id,
                Duration = track.Duration,
                TrackNumber = track.TrackNumber,
                TrackSize = track.TrackSize,
                TrackTitle = trackTitle,
                TrackPerformer = trackMetadata?.TrackPerformer ?? string.Empty,
                AlbumTitle = trackMetadata?.AlbumTitle ?? string.Empty,
                AlbumPerformer = trackMetadata?.AlbumPerformer ?? string.Empty,
                Year = discMetadata?.Year ?? string.Empty,
                Lyrics = lyrics,
            };

            this.Items.Add(viewModel);
        }

        this.HasItems = this.Items.Count > 0;
    }

    private void SetActive(Guid trackId)
    {
        foreach (var item in this.Items)
        {
            item.IsActive = trackId == item.TrackId;
        }
    }
}
