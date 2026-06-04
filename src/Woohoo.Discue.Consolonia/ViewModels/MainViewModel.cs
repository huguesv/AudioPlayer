// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Tui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Woohoo.Audio.Player.Tui.Services;
using Woohoo.Audio.Services;

public partial class MainViewModel : ObservableObject
{
    private readonly IDispatcherQueue dispatcherQueue;
    private readonly IFilePickerService filePickerService;
    private readonly IMediaPlayerService mediaPlayerService;
    private readonly ILocalSettingsService localSettingsService;
    private readonly ILogger logger;

    private string lastBrowseFolder = string.Empty;

    public MainViewModel(
        IDispatcherQueueService dispatcherQueueService,
        IFilePickerService filePickerService,
        IMediaPlayerService mediaPlayerService,
        ILocalSettingsService localSettingsService,
        IAvaloniaBitmapCacheService bitmapCacheService,
        HomeViewModel homeViewModel,
        PlaybackViewModel playbackViewModel,
        NowPlayingViewModel nowPlayingViewModel,
        PlaylistViewModel playlistViewModel,
        LyricsViewModel lyricsViewModel,
        SettingsViewModel settingsViewModel,
        VisualizationViewModel visualizationViewModel,
        ILogger<MainViewModel> logger)
    {
        this.filePickerService = filePickerService;
        this.mediaPlayerService = mediaPlayerService;
        this.localSettingsService = localSettingsService;
        this.Home = homeViewModel;
        this.Playback = playbackViewModel;
        this.NowPlaying = nowPlayingViewModel;
        this.Playlist = playlistViewModel;
        this.Lyrics = lyricsViewModel;
        this.Settings = settingsViewModel;
        this.Visualization = visualizationViewModel;
        this.logger = logger;
        this.dispatcherQueue = dispatcherQueueService.GetDispatcherQueue();

        this.View = ViewType.Home;

        this.mediaPlayerService.ActiveTrackChanged += this.MediaPlayerService_ActiveTrackChanged;

        try
        {
            this.lastBrowseFolder = this.localSettingsService.ReadSetting<string>(KnownSettingKeys.LastBrowseFolder) ?? string.Empty;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error loading settings.");
        }
        finally
        {
        }
    }

    public HomeViewModel Home { get; }

    public PlaybackViewModel Playback { get; }

    public NowPlayingViewModel NowPlaying { get; }

    public PlaylistViewModel Playlist { get; }

    public LyricsViewModel Lyrics { get; }

    public SettingsViewModel Settings { get; }
    public VisualizationViewModel Visualization { get; }

    [ObservableProperty]
    public partial bool IsAlbumLoaded { get; set; }

    [ObservableProperty]
    public partial ViewType View { get; set; }

    public async Task OpenFileAsync(string filePath)
    {
        await this.mediaPlayerService.LoadFromFileAsync(filePath);
        this.View = ViewType.NowPlaying;
    }

    [RelayCommand]
    public async Task BrowseAsync()
    {
        var filePath = await this.filePickerService.GetFilePathAsync(
            this.lastBrowseFolder,
            "Open Disc Image",
            allowMultiple: false,
            [
                new("Disc Image Files") { Patterns = ["*.cue", "*.zip", "*.chd"] },
                new("All Files") { Patterns = ["*.*"] }
            ]);

        if (!string.IsNullOrEmpty(filePath))
        {
            await this.OpenFileAsync(filePath);

            this.lastBrowseFolder = Path.GetDirectoryName(filePath) ?? string.Empty;
            this.localSettingsService.SaveSetting(KnownSettingKeys.LastBrowseFolder, this.lastBrowseFolder);
        }
    }

    [RelayCommand]
    public void ChangeView(ViewType view)
    {
        this.View = view;
    }

    private void MediaPlayerService_ActiveTrackChanged(object? sender, EventArgs e)
    {
        _ = this.dispatcherQueue.TryEnqueue(() =>
        {
            var activeTrack = this.mediaPlayerService.GetActiveTrack();

            this.IsAlbumLoaded |= activeTrack is not null;
        });
    }
}
