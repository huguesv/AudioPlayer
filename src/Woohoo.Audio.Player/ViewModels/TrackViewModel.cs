// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Woohoo.Audio.Core;
using Woohoo.Audio.Core.Lyrics;
using Woohoo.Audio.Core.Media;

public partial class TrackViewModel : ViewModelBase
{
    public TrackViewModel(IAlbumTrack track)
    {
        this.Title = string.Empty;
        this.Performer = string.Empty;
        this.Songwriter = string.Empty;
        this.PlainLyrics = string.Empty;
        this.CurrentLyric = string.Empty;
        this.Track = track;
    }

    [ObservableProperty]
    public partial bool FileNotFound { get; set; }

    [ObservableProperty]
    public partial int TrackSize { get; set; }

    [ObservableProperty]
    public partial int TrackNumber { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; }

    [ObservableProperty]
    public partial string Performer { get; set; }

    [ObservableProperty]
    public partial string Songwriter { get; set; }

    [ObservableProperty]
    public partial bool IsCurrentTrack { get; set; }

    [ObservableProperty]
    public partial string PlainLyrics { get; set; }

    [ObservableProperty]
    public partial string CurrentLyric { get; set; }

    [ObservableProperty]
    public partial bool HasLyrics { get; set; }

    public List<LyricLineViewModel> Lyrics { get; } = [];

    public LyricsTrack? LyricsData { get; private set; }

    public IAlbumTrack Track { get; }

    public void UpdateLyrics(LyricsTrack? lyrics)
    {
        this.LyricsData = lyrics;
        this.Lyrics.Clear();
        this.CurrentLyric = string.Empty;
        this.HasLyrics = false;

        if (lyrics is null)
        {
            return;
        }

        this.PlainLyrics = lyrics.PlainText;

        if (lyrics.SyncedLines.Length > 0)
        {
            foreach (var line in lyrics.SyncedLines)
            {
                this.Lyrics.Add(new LyricLineViewModel
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
                this.Lyrics.Add(new LyricLineViewModel
                {
                    Timestamp = TimeSpan.Zero,
                    Text = line,
                });
            }
        }

        this.HasLyrics = this.Lyrics.Count > 0;
    }

    public void UpdateCurrentLyricFromPosition(int position)
    {
        if (this.LyricsData is null || this.LyricsData.SyncedLines.Length == 0)
        {
            return;
        }

        var time = TimeConversion.FromPosition(position);
        var index = this.LyricsData.GetLineIndexAt(time);
        for (int i = 0; i < this.Lyrics.Count; i++)
        {
            this.Lyrics[i].IsCurrent = i == index;
        }

        this.CurrentLyric = index >= 0 && index < this.Lyrics.Count ? this.Lyrics[index].Text : string.Empty;
    }
}
