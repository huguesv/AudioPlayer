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
using Woohoo.Audio.Core.CueToolsDatabase;
using Woohoo.Audio.Core.CueToolsDatabase.Models;
using Woohoo.Audio.Core.IO;
using Woohoo.Audio.Playback;
using Woohoo.Audio.Player.Services;

public partial class MainWindowViewModel : ViewModelBase
{
    private static readonly bool AllowQueryDatabase = true;

    private readonly IFilePickerService filePickerService;
    private readonly IPowerManagementService powerManagementService;
    private readonly CTDBClient? databaseClient;
    private readonly SdlAudioPlayer player;

    [ObservableProperty]
    private bool isTipVisible;

    [ObservableProperty]
    private bool isAlbumArtVisible;

    [ObservableProperty]
    private bool isCueSheetOpen;

    [ObservableProperty]
    private string cueSheetName;

    [ObservableProperty]
    private string cueSheetFileName;

    [ObservableProperty]
    private string cueSheetContainerPath;

    [ObservableProperty]
    private int currentTrack;

    [ObservableProperty]
    private long currentTrackPosition;

    [ObservableProperty]
    private long currentTrackEndPosition;

    [ObservableProperty]
    private string currentTrackTitle;

    [ObservableProperty]
    private string complexAlbumTitle;

    [ObservableProperty]
    private string albumArtUrl;

    [ObservableProperty]
    private string albumTitle;

    [ObservableProperty]
    private string albumPerformer;

    [ObservableProperty]
    private bool isPlaying;

    [ObservableProperty]
    private bool isMuted;

    [ObservableProperty]
    private ViewType view;

    [ObservableProperty]
    private TrackViewModel? selectedTrack;

    private int volume;

    private IMusicContainer? container;

    public MainWindowViewModel(IFilePickerService filePickerService, IPowerManagementService powerManagementService, CTDBClient? databaseClient)
    {
        this.filePickerService = filePickerService;
        this.powerManagementService = powerManagementService;
        this.databaseClient = databaseClient;

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
        this.AlbumArtUrl = string.Empty;
        this.AlbumTitle = string.Empty;
        this.AlbumPerformer = string.Empty;
    }

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

    public bool CanPlayPause() => this.IsCueSheetOpen && this.Tracks.Count > 0;

    public bool CanPlayPreviousTrack() => this.IsCueSheetOpen && this.CurrentTrack > 0 && this.Tracks.Count > 0;

    public bool CanPlayNextTrack() => this.IsCueSheetOpen && this.CurrentTrack < this.Tracks.Count - 1;

    public bool CanPlaySelectedTrack() => this.SelectedTrack is not null;

    public bool CanSkipForwardOrBack() => this.IsCueSheetOpen;

    [RelayCommand]
    public void ChangeView(ViewType view)
    {
        this.View = view;
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
        CueSheet cueSheet = parser.Parse(cueData);

        this.Tracks.Clear();

        bool shouldQueryDatabase = true;

        foreach (var file in cueSheet.Files)
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

                if (!string.IsNullOrEmpty(track.Title))
                {
                    shouldQueryDatabase = false;
                }

                TrackViewModel trackViewModel = new()
                {
                    FileName = file.FileName,
                    TrackNumber = track.TrackNumber,
                    Title = track.Title ?? $"Track {track.TrackNumber}",
                    Performer = track.Performer ?? cueSheet.Performer ?? string.Empty,
                    Songwriter = track.Songwriter ?? cueSheet.Songwriter ?? string.Empty,
                    FileSize = this.container.GetFileSize(file.FileName),
                };

                this.Tracks.Add(trackViewModel);
            }
        }

        this.AlbumArtUrl = string.Empty;
        this.IsAlbumArtVisible = !string.IsNullOrEmpty(this.AlbumArtUrl);
        this.AlbumTitle = cueSheet.Title ?? this.CueSheetName;
        this.AlbumPerformer = cueSheet.Performer ?? string.Empty;

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

        if (AllowQueryDatabase && shouldQueryDatabase)
        {
            this.QueryDatabase(cueSheet);
        }
    }

    private void QueryDatabase(CueSheet cueSheet)
    {
        if (this.databaseClient is null)
        {
            return;
        }

        var container = this.container;
        if (container is null)
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                var response = await this.databaseClient.QueryAsync(
                    CTDBTocCalculator.GetTocFromCue(cueSheet, container),
                    ctdb: true,
                    fuzzy: true,
                    metadataSearch: CTDBMetadataSearch.Extensive,
                    cancellationToken: CancellationToken.None);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var metadata = response?.Metadatas?.FirstOrDefault(md => md.Tracks?.Length == this.Tracks.Count);
                    if (metadata is not null)
                    {
                        this.UpdateMetadataFromDatabase(metadata);
                    }
                });
            }
            catch
            {
            }
        });
    }

    private void UpdateMetadataFromDatabase(CTDBResponseMeta metadata)
    {
        for (int i = 0; i < this.Tracks.Count; i++)
        {
            var track = this.Tracks[i];
            var dbTrack = metadata.Tracks?[i];
            if (dbTrack is not null)
            {
                track.Title = BestString(dbTrack.Name, track.Title);
                track.Performer = BestString(dbTrack.Artist, metadata.Artist, track.Performer);
            }
        }

        this.AlbumArtUrl = metadata.CoverArts?.FirstOrDefault()?.Uri ?? string.Empty;
        this.IsAlbumArtVisible = !string.IsNullOrEmpty(this.AlbumArtUrl);
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
}
