// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Avalonia.ViewModels;

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Woohoo.Audio.Core.Lyrics;

public sealed partial class PlaylistItemViewModel : ObservableObject
{
    private readonly ILogger logger;

    public PlaylistItemViewModel(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        this.logger = logger;
    }

    public Guid TrackId { get; init; }

    [ObservableProperty]
    public partial bool FileNotFound { get; set; }

    [ObservableProperty]
    public partial bool IsActive { get; set; }

    [ObservableProperty]
    public partial int TrackNumber { get; set; }

    [ObservableProperty]
    public partial string TrackTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TrackPerformer { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AlbumPerformer { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AlbumTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Category { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Year { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int TrackSize { get; set; }

    [ObservableProperty]
    public partial TimeSpan Duration { get; set; } = TimeSpan.Zero;

    public LyricsTrack? Lyrics { get; set; }

    [RelayCommand]
    public void Play()
    {
        try
        {
            WeakReferenceMessenger.Default.Send(new PlayTrackMessage { TrackId = this.TrackId });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to play track");
        }
    }
}
