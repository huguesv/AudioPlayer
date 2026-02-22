// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

#define PLAY_USING_STREAM

namespace Woohoo.Audio.Player.Tui.ViewModels;

using System;
using System.Collections.Generic;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Woohoo.Audio.Core;
using Woohoo.Audio.Core.Cue;
using Woohoo.Audio.Core.IO;
using Woohoo.Audio.Core.Lyrics;
using Woohoo.Audio.Core.Metadata;
using Woohoo.Audio.Playback;
using Woohoo.Audio.Player.Tui.Models;
using Woohoo.Audio.Player.Tui.Services;

public partial class MainViewModel : ObservableObject
{
    private const string PlayCaptionPlay = " ► ";
    private const string PlayCaptionPause = " ■ ";

    private readonly IFilePickerService filePickerService;
    private readonly IThemeService themeService;
    private readonly IMetadataProvider? metadataProvider;
    private readonly ILyricsProvider? lyricsProvider;
    private readonly SdlAudioPlayer player;
    private readonly UserSettingsManager settingsManager;
    private readonly string localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    private string lastBrowseFolder = string.Empty;
    private int volume;
    private bool isLoading;

    public MainViewModel(IFilePickerService filePickerService, IThemeService themeService, IMetadataProvider? metadataProvider, ILyricsProvider? lyricsProvider)
    {
        this.filePickerService = filePickerService;
        this.themeService = themeService;
        this.metadataProvider = metadataProvider;
        this.lyricsProvider = lyricsProvider;

        this.player = new SdlAudioPlayer(this.Played);
        this.volume = this.player.Volume;

        this.SelectedView = KnownViews.NowPlaying;
        this.IsTipVisible = true;

        this.CurrentTrackPosition = 0;
        this.CurrentTrackEndPosition = 0;
        this.CurrentTrackTitle = string.Empty;
        this.ComplexAlbumTitle = string.Empty;
        this.AlbumPerformer = string.Empty;
        this.AlbumTitle = string.Empty;

        this.isLoading = true;
        try
        {
            this.settingsManager = new UserSettingsManager(Path.Combine(this.localApplicationData, "Woohoo.Audio.Player.Tui", "LocalSettings.json"));
            var settings = this.settingsManager.LoadSettings();

            this.lastBrowseFolder = settings.LastBrowseFolder;
            this.SelectedTheme = !string.IsNullOrEmpty(settings.Theme) ? settings.Theme : KnownThemes.ModernDark;
            this.FetchOnlineMetadata = settings.FetchOnlineMetadata;
            this.FetchLyrics = settings.FetchLyrics;
        }
        finally
        {
            this.isLoading = false;
        }
    }

    [ObservableProperty]
    public partial string SelectedTheme { get; set; }

    [ObservableProperty]
    public partial string SelectedView { get; set; }

    [ObservableProperty]
    public partial bool FetchOnlineMetadata { get; set; }

    [ObservableProperty]
    public partial bool FetchLyrics { get; set; }

    [ObservableProperty]
    public partial bool IsTipVisible { get; set; }

    [ObservableProperty]
    public partial bool IsAlbumArtVisible { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PlayPauseCommand))]
    [NotifyCanExecuteChangedFor(nameof(SkipBackCommand))]
    [NotifyCanExecuteChangedFor(nameof(SkipForwardCommand))]
    public partial TrackViewModel? CurrentTrack { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PlayPauseCommand))]
    [NotifyCanExecuteChangedFor(nameof(SkipBackCommand))]
    [NotifyCanExecuteChangedFor(nameof(SkipForwardCommand))]
    public partial int CurrentTrackIndex { get; set; }

    [ObservableProperty]
    public partial long CurrentTrackPosition { get; set; }

    [ObservableProperty]
    public partial long CurrentTrackEndPosition { get; set; }

    [ObservableProperty]
    public partial string CurrentTrackTitle { get; set; }

    [ObservableProperty]
    public partial string ComplexAlbumTitle { get; set; }

    [ObservableProperty]
    public partial string AlbumTitle { get; set; }

    [ObservableProperty]
    public partial string AlbumPerformer { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PlayPauseCommand))]
    [NotifyCanExecuteChangedFor(nameof(SkipBackCommand))]
    [NotifyCanExecuteChangedFor(nameof(SkipForwardCommand))]
    public partial bool IsPlaying { get; set; }

    [ObservableProperty]
    public partial bool IsMuted { get; set; }

    [ObservableProperty]
    public partial string PlayPauseCaption { get; set; } = PlayCaptionPlay;

    [ObservableProperty]
    public partial TrackViewModel? PlaylistSelectedTrack { get; set; }

    public AboutInformationViewModel AboutInfo { get; } = new();

    public ObservableCollection<TrackViewModel> Tracks { get; } = [];

    public int Volume
    {
        get => this.volume;
        set
        {
            this.SetProperty(ref this.volume, value);
            this.player.Volume = value;
            this.IsMuted = false;
        }
    }

    public SdlAudioPlayer Player => this.player;

    public bool CanPlayPause() => this.Tracks.Count > 0;

    public bool CanPlayPreviousTrack() => this.CurrentTrackIndex > 0 && this.Tracks.Count > 0;

    public bool CanPlayNextTrack() => this.CurrentTrackIndex < this.Tracks.Count - 1;

    public bool CanPlaySelectedTrack() => this.IsPlaying;

    public bool CanSkipForwardOrBack() => this.IsPlaying;


    [RelayCommand]
    public void SelectTheme(string theme)
    {
        this.SelectedTheme = theme;
        this.themeService.SelectedTheme = theme;
    }

    [RelayCommand]
    public void SelectView(string view)
    {
        this.SelectedView = view;
    }

    [RelayCommand]
    public async Task BrowseAsync()
    {
        var filePath = await this.filePickerService.GetFilePathAsync(
            this.lastBrowseFolder,
            "Open cue sheet",
            allowMultiple: false,
            new List<FilePickerFileType>
            {
                new("Cue sheets") { Patterns = ["*.cue", "*.zip"] },
                new("*.* files") { Patterns = ["*.*"] }
            });

        if (!string.IsNullOrEmpty(filePath))
        {
            this.Open([filePath]);

            this.lastBrowseFolder = Path.GetDirectoryName(filePath) ?? string.Empty;
            this.SaveSettings();
        }
    }

    [RelayCommand]
    public void ToggleFetchOnlineMetadata()
    {
        this.FetchOnlineMetadata = !this.FetchOnlineMetadata;
        this.QueryMetadataAndLyrics(this.FetchOnlineMetadata, this.FetchLyrics);
    }

    [RelayCommand]
    public void ToggleFetchLyrics()
    {
        this.FetchLyrics = !this.FetchLyrics;
        this.QueryMetadataAndLyrics(this.FetchOnlineMetadata, this.FetchLyrics);
    }

    [RelayCommand]
    public void Mute()
    {
        this.IsMuted = !this.IsMuted;
        this.player.Volume = this.IsMuted ? 0 : this.Volume;
    }

    [RelayCommand]
    public void VolumeUp()
    {
        this.Volume = Math.Min(100, this.Volume + 5);
    }

    [RelayCommand]
    public void VolumeDown()
    {
        this.Volume = Math.Max(0, this.Volume - 5);
    }

    [RelayCommand(CanExecute = nameof(CanPlayPause))]
    public void PlayPause()
    {
        if (this.Tracks[this.CurrentTrackIndex].FileNotFound)
        {
            return;
        }

        if (this.player.IsPlaying)
        {
            this.player.Pause();
            this.IsPlaying = false;
        }
        else
        {
            this.player.Resume();
            this.IsPlaying = true;
        }

        this.UpdateCommandUI();
    }

    [RelayCommand(CanExecute = nameof(CanPlayPreviousTrack))]
    public void PlayPreviousTrack()
    {
        if (this.CurrentTrackIndex - 1 < 0)
        {
            return;
        }

        this.PlayTrack(this.CurrentTrackIndex - 1);
    }

    [RelayCommand(CanExecute = nameof(CanPlayNextTrack))]
    public void PlayNextTrack()
    {
        if (this.CurrentTrackIndex + 1 >= this.Tracks.Count)
        {
            return;
        }

        this.PlayTrack(this.CurrentTrackIndex + 1);
    }

    [RelayCommand(CanExecute = nameof(CanPlaySelectedTrack))]
    public void PlaySelectedTrack()
    {
        if (this.PlaylistSelectedTrack is null)
        {
            return;
        }

        this.PlayTrack(this.Tracks.IndexOf(this.PlaylistSelectedTrack));
    }

    [RelayCommand(CanExecute = nameof(CanSkipForwardOrBack))]
    public void SkipBack()
    {
        if (this.CurrentTrackIndex >= this.Tracks.Count)
        {
            return;
        }

        this.player.SkipBack(TimeSpan.FromSeconds(15));
    }

    [RelayCommand(CanExecute = nameof(CanSkipForwardOrBack))]
    public void SkipForward()
    {
        if (this.CurrentTrackIndex >= this.Tracks.Count)
        {
            return;
        }

        this.player.SkipForward(TimeSpan.FromSeconds(15));
    }

    public void Open(params IEnumerable<string> filePaths)
    {
        foreach (var filePath in filePaths)
        {
            try
            {
                this.OpenFile(filePath);
                break;
            }
            catch
            {
            }
        }
    }

    private void Played(byte[] buffer, int count, int position, bool eof)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            bool oldIsPlaying = this.IsPlaying;

            this.CurrentTrackPosition = position;
            this.IsPlaying = !eof;
            this.CurrentTrack?.UpdateCurrentLyricFromPosition(position);

            if (eof)
            {
                this.PlayNextTrack();
            }

            if (eof || oldIsPlaying != this.IsPlaying)
            {
                this.UpdateCommandUI();
            }
        });
    }

    private void OpenFile(string filePath)
    {
        var loader = new AlbumLoader();
        var tracks = loader.LoadFrom(filePath);
        if (tracks.Count == 0)
        {
            return;
        }

        this.Tracks.Clear();

        foreach (var track in tracks)
        {
            TrackViewModel trackViewModel = new()
            {
                Container = track.Container,
                CueSheet = track.CueSheet,
                FileName = track.TrackFileName,
                FileNotFound = track.TrackFileNotFound,
                TrackNumber = track.TrackNumber,
                Title = track.TrackTitle.Length > 0 ? track.TrackTitle : $"Track {track.TrackNumber:00}",
                Performer = track.TrackPerformer,
                Songwriter = track.TrackSongwriter,
                TrackOffset = track.TrackOffset,
                TrackSize = track.TrackSize,
            };

            this.Tracks.Add(trackViewModel);
        }

        this.AlbumTitle = tracks[0].AlbumTitle;
        this.AlbumPerformer = tracks[0].AlbumPerformer;

        if (string.IsNullOrEmpty(this.AlbumPerformer))
        {
            this.ComplexAlbumTitle = this.AlbumTitle;
        }
        else
        {
            this.ComplexAlbumTitle = $"{this.AlbumPerformer} - {this.AlbumTitle}";
        }

        this.IsTipVisible = false;

        if (this.Tracks.Count > 0)
        {
            this.PlayTrack(0);
        }

        this.UpdateCommandUI();

        this.QueryMetadataAndLyrics(this.FetchOnlineMetadata, this.FetchLyrics);
    }

    private void QueryMetadataAndLyrics(bool fetchOnlineMetadata, bool fetchLyrics)
    {
        if (this.Tracks.Count == 0)
        {
            return;
        }

        var container = this.Tracks[0].Container;
        var cueSheet = this.Tracks[0].CueSheet;

        _ = Task.Run(async () =>
        {
            if (fetchOnlineMetadata)
            {
                await this.QueryMetadataAsync(container, cueSheet);
            }

            // Lyrics fetching depends on metadata fetching to have track titles/performers updated
            if (fetchLyrics)
            {
                await this.QueryLyricsAsync();
            }
        });
    }

    private async Task QueryLyricsAsync()
    {
        if (this.lyricsProvider is null || this.Tracks.Count == 0)
        {
            return;
        }

        try
        {
            CancellationTokenSource tokenSource = new();
            tokenSource.CancelAfter(TimeSpan.FromSeconds(30));
            for (int i = 0; i < this.Tracks.Count; i++)
            {
                var track = this.Tracks[i];
                var lyrics = await this.lyricsProvider.QueryAsync(
                    this.AlbumTitle,
                    this.AlbumPerformer,
                    track.Title,
                    TimeConversion.FromPosition(track.TrackSize),
                    cancellationToken: tokenSource.Token);
                if (lyrics is not null)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        track.UpdateLyrics(lyrics);
                    });
                }
            }
        }
        catch
        {
        }
    }

    private async Task QueryMetadataAsync(IMusicContainer container, CueSheet cueSheet)
    {
        if (this.metadataProvider is null || this.Tracks.Count == 0)
        {
            return;
        }

        try
        {
            CancellationTokenSource tokenSource = new();
            tokenSource.CancelAfter(TimeSpan.FromSeconds(30));

            var metadata = await this.metadataProvider.QueryAsync(
                cueSheet,
                container,
                cancellationToken: tokenSource.Token);

            if (metadata is not null)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    this.LoadMetadata(metadata);
                });
            }
        }
        catch
        {
        }
    }

    private void LoadMetadata(AlbumMetadata metadata)
    {
        for (int i = 0; i < this.Tracks.Count; i++)
        {
            var track = this.Tracks[i];
            var dbTrack = metadata.Tracks[i];
            if (dbTrack is not null)
            {
                track.Title = BestString(dbTrack.Name, track.Title);
                track.Performer = BestString(dbTrack.Artist, metadata.Artist, track.Performer);
            }
        }

        this.AlbumTitle = BestString(metadata.Album, this.AlbumTitle);
        this.AlbumPerformer = BestString(metadata.Artist, this.AlbumPerformer);

        if (string.IsNullOrEmpty(this.AlbumPerformer))
        {
            this.ComplexAlbumTitle = this.AlbumTitle;
        }
        else
        {
            this.ComplexAlbumTitle = $"{this.AlbumPerformer} - {this.AlbumTitle}";
        }

        this.CurrentTrackTitle = string.Format("{0:00}. {1}", this.Tracks[this.CurrentTrackIndex].TrackNumber, this.Tracks[this.CurrentTrackIndex].Title);

        static string BestString(params string[] values)
        {
            return values.FirstOrDefault(val => !string.IsNullOrEmpty(val)) ?? string.Empty;
        }
    }

    private void PlayTrack(int trackIndex)
    {
        if (this.player.IsPlaying)
        {
            this.player.Pause();
        }

        this.IsPlaying = false;

        for (int i = 0; i < this.Tracks.Count; i++)
        {
            if (i != trackIndex)
            {
                this.Tracks[i].IsCurrentTrack = false;
            }
        }

        this.CurrentTrackIndex = trackIndex;
        this.CurrentTrack = this.Tracks[trackIndex];
        this.CurrentTrackPosition = 0;
        this.CurrentTrackEndPosition = this.Tracks[trackIndex].TrackSize;
        this.CurrentTrackTitle = string.Format("{0:00}. {1}", this.Tracks[trackIndex].TrackNumber, this.Tracks[trackIndex].Title);

        if (!this.Tracks[trackIndex].FileNotFound)
        {
            this.Tracks[trackIndex].IsCurrentTrack = true;

#if PLAY_USING_STREAM
            var fileStream = this.Tracks[trackIndex].Container.OpenFileStream(this.Tracks[trackIndex].FileName);
            var trackStream = new SubStream(
                fileStream,
                this.Tracks[trackIndex].TrackOffset,
                this.Tracks[trackIndex].TrackSize);
            this.player.Play(trackStream, this.Tracks[trackIndex].TrackSize);
#else
            var trackData = this.Tracks[trackIndex].Container.ReadFileBytes(
                this.Tracks[trackIndex].FileName,
                this.Tracks[trackIndex].TrackOffset,
                this.Tracks[trackIndex].TrackSize);
            this.player.Play(trackData);
#endif

            this.IsPlaying = true;
        }

        this.UpdateCommandUI();
    }

    private void UpdateCommandUI()
    {
        this.PlayNextTrackCommand.NotifyCanExecuteChanged();
        this.PlayPreviousTrackCommand.NotifyCanExecuteChanged();
        this.PlayPauseCommand.NotifyCanExecuteChanged();
        this.SkipBackCommand.NotifyCanExecuteChanged();
        this.SkipForwardCommand.NotifyCanExecuteChanged();
        this.PlaySelectedTrackCommand.NotifyCanExecuteChanged();
        this.PlayPauseCaption = this.IsPlaying ? PlayCaptionPause : PlayCaptionPlay;
    }

    private void SaveSettings()
    {
        var settings = new UserSettings
        {
            LastBrowseFolder = this.lastBrowseFolder,
            Theme = this.SelectedTheme,
            FetchOnlineMetadata = this.FetchOnlineMetadata,
            FetchLyrics = this.FetchLyrics,
        };

        this.settingsManager.SaveSettings(settings);
    }

    partial void OnFetchOnlineMetadataChanged(bool value)
    {
        if (this.isLoading)
        {
            return;
        }

        this.SaveSettings();
    }

    partial void OnFetchLyricsChanged(bool value)
    {
        if (this.isLoading)
        {
            return;
        }

        this.SaveSettings();
    }

    partial void OnSelectedThemeChanged(string value)
    {
        if (this.isLoading)
        {
            return;
        }

        this.SaveSettings();
    }
}
