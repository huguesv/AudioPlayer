// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

#define PLAY_USING_STREAM

namespace Woohoo.Audio.Player.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Woohoo.Audio.Core.Cue;
using Woohoo.Audio.Core.Cue.Serialization;
using Woohoo.Audio.Core.IO;
using Woohoo.Audio.Core.Metadata;
using Woohoo.Audio.Playback;
using Woohoo.Audio.Player.Models;
using Woohoo.Audio.Player.Services;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IFilePickerService filePickerService;
    private readonly IPowerManagementService powerManagementService;
    private readonly IMetadataProvider? metadataProvider;
    private readonly SdlAudioPlayer player;
    private readonly UserSettingsManager settingsManager;
    private readonly string localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    private int volume;
    private IMusicContainer? container;
    private CueSheet? cueSheet;
    private bool isLoading;

    public MainWindowViewModel(IFilePickerService filePickerService, IPowerManagementService powerManagementService, IMetadataProvider? metadataProvider)
    {
        this.filePickerService = filePickerService;
        this.powerManagementService = powerManagementService;
        this.metadataProvider = metadataProvider;

        this.player = new SdlAudioPlayer(this.Played);
        this.volume = this.player.Volume;

        this.View = ViewType.NowPlaying;
        this.IsTipVisible = true;

        this.CueSheetName = string.Empty;
        this.CueSheetFileName = string.Empty;
        this.CueSheetContainerPath = string.Empty;
        this.CurrentTrackPosition = 0;
        this.CurrentTrackEndPosition = 0;
        this.CurrentTrackTitle = string.Empty;
        this.ComplexAlbumTitle = string.Empty;
        this.AlbumPerformer = string.Empty;
        this.AlbumTitle = string.Empty;

        this.isLoading = true;
        try
        {
            this.settingsManager = new UserSettingsManager(Path.Combine(this.localApplicationData, "Woohoo.Audio.Player", "LocalSettings.json"));
            var settings = this.settingsManager.LoadSettings();

            this.FetchOnlineMetadata = settings.FetchOnlineMetadata;
            this.ShowAlbumArt = settings.ShowAlbumArt;
        }
        finally
        {
            this.isLoading = false;
        }
    }

    [ObservableProperty]
    public partial bool FetchOnlineMetadata { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextArtCommand))]
    [NotifyCanExecuteChangedFor(nameof(PreviousArtCommand))]
    public partial bool ShowAlbumArt { get; set; }

    [ObservableProperty]
    public partial bool IsTipVisible { get; set; }

    [ObservableProperty]
    public partial bool IsAlbumArtVisible { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextArtCommand))]
    [NotifyCanExecuteChangedFor(nameof(PreviousArtCommand))]
    public partial AlbumArtViewModel? CurrentArt { get; private set; }

    [ObservableProperty]
    public partial bool IsCueSheetOpen { get; set; }

    [ObservableProperty]
    public partial string CueSheetName { get; set; }

    [ObservableProperty]
    public partial string CueSheetFileName { get; set; }

    [ObservableProperty]
    public partial string CueSheetContainerPath { get; set; }

    [ObservableProperty]
    public partial int CurrentTrack { get; set; }

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
    public partial bool IsPlaying { get; set; }

    [ObservableProperty]
    public partial bool IsMuted { get; set; }

    [ObservableProperty]
    public partial ViewType View { get; set; }

    [ObservableProperty]
    public partial TrackViewModel? SelectedTrack { get; set; }

    public ObservableCollection<TrackViewModel> Tracks { get; } = [];

    public ObservableCollection<AlbumArtViewModel> Art { get; } = [];

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

    public bool CanPlayPause() => this.IsCueSheetOpen && this.Tracks.Count > 0;

    public bool CanPlayPreviousTrack() => this.IsCueSheetOpen && this.CurrentTrack > 0 && this.Tracks.Count > 0;

    public bool CanPlayNextTrack() => this.IsCueSheetOpen && this.CurrentTrack < this.Tracks.Count - 1;

    public bool CanPlaySelectedTrack() => this.SelectedTrack is not null;

    public bool CanSkipForwardOrBack() => this.IsCueSheetOpen;

    public bool CanChangeArt() => this.Art.Count > 1 && this.ShowAlbumArt;

    [RelayCommand]
    public void ChangeView(ViewType view)
    {
        this.View = view;
    }

    [RelayCommand(CanExecute = nameof(CanChangeArt))]
    public void NextArt()
    {
        if (this.Art.Count < 2 || this.CurrentArt is null || !this.ShowAlbumArt)
        {
            return;
        }

        int currentIndex = this.Art.IndexOf(this.CurrentArt);
        int nextIndex = (currentIndex + 1) % this.Art.Count;
        this.CurrentArt = this.Art[nextIndex];
    }

    [RelayCommand(CanExecute = nameof(CanChangeArt))]
    public void PreviousArt()
    {
        if (this.Art.Count < 2 || this.CurrentArt is null || !this.ShowAlbumArt)
        {
            return;
        }

        int currentIndex = this.Art.IndexOf(this.CurrentArt);
        int previousIndex = (currentIndex - 1 + this.Art.Count) % this.Art.Count;
        this.CurrentArt = this.Art[previousIndex];
    }

    [RelayCommand]
    public void ToggleFetchOnlineMetadata()
    {
        this.FetchOnlineMetadata = !this.FetchOnlineMetadata;
        this.QueryMetadata();
    }

    [RelayCommand]
    public void ToggleShowAlbumArt()
    {
        this.ShowAlbumArt = !this.ShowAlbumArt;
        this.IsAlbumArtVisible = this.Art.Count > 0 && this.ShowAlbumArt;
    }

    [RelayCommand]
    public async Task BrowseAsync()
    {
        var filePaths = await this.filePickerService.GetFilePathsAsync(
            Localized.BrowseDialogTitle,
            allowMultiple: false,
            [new FilePickerFileType("Albums") { Patterns = ["*.cue", "*.zip"] }]);
        if (filePaths.Length > 0)
        {
            this.Open(filePaths);
        }
    }

    [RelayCommand]
    public void Mute()
    {
        this.IsMuted = !this.IsMuted;
        this.player.Volume = this.IsMuted ? 0 : this.Volume;
    }

    [RelayCommand(CanExecute = nameof(CanPlayPause))]
    public void PlayPause()
    {
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
        if (this.CurrentTrack - 1 < 0)
        {
            return;
        }

        this.PlayTrack(this.CurrentTrack - 1);
    }

    [RelayCommand(CanExecute = nameof(CanPlayNextTrack))]
    public void PlayNextTrack()
    {
        if (this.CurrentTrack + 1 >= this.Tracks.Count)
        {
            return;
        }

        this.PlayTrack(this.CurrentTrack + 1);
    }

    [RelayCommand(CanExecute = nameof(CanPlaySelectedTrack))]
    public void PlaySelectedTrack()
    {
        if (this.SelectedTrack is not null)
        {
            this.PlayTrack(this.Tracks.IndexOf(this.SelectedTrack));
        }
    }

    [RelayCommand(CanExecute = nameof(CanSkipForwardOrBack))]
    public void SkipBack()
    {
        this.player.SkipBack(TimeSpan.FromSeconds(15));
    }

    [RelayCommand(CanExecute = nameof(CanSkipForwardOrBack))]
    public void SkipForward()
    {
        this.player.SkipForward(TimeSpan.FromSeconds(15));
    }

    public void Open(params IEnumerable<string> filePaths)
    {
        foreach (var filePath in filePaths)
        {
            if (Path.GetExtension(filePath) == ".cue")
            {
                this.OpenCue(filePath);
                break;
            }
            else if (Path.GetExtension(filePath) == ".zip")
            {
                this.OpenArchive(filePath);
                break;
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

    private void OpenCue(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        string? folder = Path.GetDirectoryName(filePath);
        if (folder is null)
        {
            return;
        }

        this.ProcessContainer(new MusicFolderContainer(folder));
    }

    private void OpenArchive(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        this.ProcessContainer(new MusicZipContainer(filePath));
    }

    private void ProcessContainer(IMusicContainer container)
    {
        this.container = container;

        var cueFile = this.container.EnumerateFilesByExtension("cue").FirstOrDefault();
        if (cueFile is null)
        {
            return;
        }

        this.CueSheetName = Path.GetFileNameWithoutExtension(cueFile);
        this.CueSheetFileName = cueFile;
        this.CueSheetContainerPath = this.container.ContainerPath;

        var cueData = this.container.ReadFileText(this.CueSheetFileName);

        CueSheetReader parser = new();
        this.cueSheet = parser.Parse(cueData);

        this.Tracks.Clear();

        foreach (var file in this.cueSheet.Files)
        {
            foreach (var track in file.Tracks)
            {
                if (track.TrackMode != KnownTrackModes.Audio)
                {
                    continue;
                }

                if (!this.container.FileExists(file.FileName))
                {
                    continue;
                }

                TrackViewModel trackViewModel = new()
                {
                    FileName = file.FileName,
                    TrackNumber = track.TrackNumber,
                    Title = track.Title ?? $"Track {track.TrackNumber}",
                    Performer = track.Performer ?? this.cueSheet.Performer ?? string.Empty,
                    Songwriter = track.Songwriter ?? this.cueSheet.Songwriter ?? string.Empty,
                    FileSize = this.container.GetFileSize(file.FileName),
                };

                this.Tracks.Add(trackViewModel);
            }
        }

        this.Art.Clear();
        this.IsAlbumArtVisible = this.Art.Count > 0 && this.ShowAlbumArt;
        this.CurrentArt = null;
        this.AlbumTitle = this.cueSheet.Title ?? this.CueSheetName;
        this.AlbumPerformer = this.cueSheet.Performer ?? string.Empty;

        if (string.IsNullOrEmpty(this.AlbumPerformer))
        {
            this.ComplexAlbumTitle = this.AlbumTitle;
        }
        else
        {
            this.ComplexAlbumTitle = $"{this.AlbumPerformer} - {this.AlbumTitle}";
        }

        this.IsTipVisible = false;
        this.IsCueSheetOpen = true;

        if (this.Tracks.Count > 0)
        {
            this.PlayTrack(0);
        }

        this.UpdateCommandUI();

        if (this.FetchOnlineMetadata)
        {
            this.QueryMetadata();
        }
    }

    private void QueryMetadata()
    {
        if (this.metadataProvider is null)
        {
            return;
        }

        // Capture mutable state in case it changes while the task is running
        var container = this.container;
        if (container is null)
        {
            return;
        }

        var cueSheet = this.cueSheet;
        if (cueSheet is null)
        {
            return;
        }

        _ = Task.Run(async () =>
        {
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
        });
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

        foreach (var art in metadata.Images)
        {
            this.Art.Add(new AlbumArtViewModel
            {
                Url = art.Url,
                IsPrimary = art.IsPrimary,
            });
        }

        this.IsAlbumArtVisible = this.Art.Count > 0 && this.ShowAlbumArt;
        this.CurrentArt = this.Art.FirstOrDefault(a => a.IsPrimary) ?? this.Art.FirstOrDefault();

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

        this.CurrentTrackTitle = this.Tracks[this.CurrentTrack].Title;

        static string BestString(params string[] values)
        {
            return values.FirstOrDefault(val => !string.IsNullOrEmpty(val)) ?? string.Empty;
        }
    }

    private void PlayTrack(int trackIndex)
    {
        Debug.Assert(this.container is not null, "Container not set");

        this.CurrentTrack = trackIndex;
        this.CurrentTrackPosition = 0;
        this.CurrentTrackEndPosition = this.Tracks[trackIndex].FileSize;
        this.CurrentTrackTitle = this.Tracks[trackIndex].Title;

        this.Tracks[trackIndex].IsCurrentTrack = true;
        for (int i = 0; i < this.Tracks.Count; i++)
        {
            if (i != trackIndex)
            {
                this.Tracks[i].IsCurrentTrack = false;
            }
        }

#if PLAY_USING_STREAM
        var trackStream = this.container.OpenFileStream(this.Tracks[trackIndex].FileName);
        this.player.Play(trackStream, (int)this.Tracks[trackIndex].FileSize);
#else
        var trackData = this.container.ReadFileBytes(this.Tracks[trackIndex].FileName);
        this.player.Play(trackData);
#endif

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

        if (this.IsPlaying)
        {
            this.powerManagementService.PreventSleep();
        }
        else
        {
            this.powerManagementService.RestoreSleep();
        }
    }

    private void SaveSettings()
    {
        var settings = new UserSettings
        {
            FetchOnlineMetadata = this.FetchOnlineMetadata,
            ShowAlbumArt = this.ShowAlbumArt,
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

    partial void OnShowAlbumArtChanged(bool value)
    {
        if (this.isLoading)
        {
            return;
        }

        this.SaveSettings();
    }
}
