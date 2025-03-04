// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

#define PLAY_USING_STREAM

namespace Woohoo.Audio.Player.Cli;

using System.Timers;
using Woohoo.Audio.Playback;

internal class ConsolePlayer
{
    private readonly SdlAudioPlayer player;

    private int volume;

    private Album? album;

    private int currentTrack;

    private long currentTrackEndPosition;

    private int currentTrackPosition;

    private bool isPlaying;

    private (int Left, int Top) cursorBeginPos;

    private Timer commandEraseTimer;

    private string previousTrackNumberAndTitle;

    private string previousTrackTime;

    public ConsolePlayer()
    {
        this.player = new SdlAudioPlayer(this.Played);
        this.volume = this.player.Volume;

        this.currentTrackPosition = 0;
        this.currentTrackEndPosition = 0;

        this.commandEraseTimer = new Timer(2000);
        this.commandEraseTimer.Elapsed += this.OnCommandEraseTimerElapsed;
        this.commandEraseTimer.Enabled = false;
        this.commandEraseTimer.AutoReset = false;

        this.previousTrackNumberAndTitle = string.Empty;
        this.previousTrackTime = string.Empty;
    }

    public static void ClearScreen()
    {
        Console.Clear();
    }

    public static void PrintCopyright()
    {
        Console.WriteLine("Woohoo.Audio.Player.Cli. Copyright (c) 2025 Hugues Valois.");
        Console.WriteLine("An audio player for CD dumps in bin/cue format.");
        Console.WriteLine();
    }

    public static void PrintUsage()
    {
        Console.WriteLine("Usage: Woohoo.Audio.Player.Cli <cue-or-zip-file>");
        Console.WriteLine();
    }

    public static void PrintCommands()
    {
        Console.WriteLine("Q  : Quit          P  : Pause        R : Resume");
        Console.WriteLine("UP : Volume Up     LT : Prev Track   - : Seek Back");
        Console.WriteLine("DN : Volume Down   RT : Next Track   + : Seek Forward");
        Console.WriteLine();
    }

    public void LoadAlbum(Album album)
    {
        this.album = album;
    }

    public bool HandleKey(ConsoleKey key)
    {
        switch (key)
        {
            case ConsoleKey.P:
                this.PauseTrack();
                break;
            case ConsoleKey.R:
                this.ResumeTrack();
                break;
            case ConsoleKey.UpArrow:
                this.VolumeUp();
                break;
            case ConsoleKey.DownArrow:
                this.VolumeDown();
                break;
            case ConsoleKey.LeftArrow:
                this.PreviousTrack();
                break;
            case ConsoleKey.RightArrow:
                this.NextTrack();
                break;
            case ConsoleKey.OemMinus:
                this.SkipBackward();
                break;
            case ConsoleKey.OemPlus:
                this.SkipForward();
                break;
            case ConsoleKey.Q:
                return false;
        }

        return true;
    }

    public void PlayAll()
    {
        this.cursorBeginPos = Console.GetCursorPosition();

        if (this.album is null)
        {
            return;
        }

        if (this.album.Tracks.Count > 0)
        {
            StringBuilder sb = new StringBuilder();
            if (this.album.Performer.Length > 0)
            {
                sb.Append(this.album.Performer);
                sb.Append(" - ");
            }

            if (this.album.Title.Length > 0)
            {
                sb.Append(this.album.Title);
            }
            else
            {
                sb.Append(this.album.CueSheetName);
            }

            this.PrintAlbumTitle(sb.ToString());
            this.PlayTrack(0);
        }
    }

    private static string PositionToString(long position)
    {
        long length = position / 176400;

        var minutes = length / 60;
        var seconds = length % 60;
        return $"{minutes:D2}:{seconds:D2}";
    }

    private void PrintAlbumTitle(string text) => this.PrintAtLine(text, 0);

    private void PrintTrackNumberAndTitle(string text) => this.PrintAtLine(text, 1);

    private void PrintTrackTime(string text) => this.PrintAtLine(text, 2);

    private void PrintCommand(string text)
    {
        this.PrintAtLine(text, 3);

        // Commands are only visible for a short time
        if (!string.IsNullOrEmpty(text))
        {
            this.commandEraseTimer.Stop();
            this.commandEraseTimer.Start();
        }
    }

    private void PrintAtLine(string text, int lineOffset)
    {
        Console.SetCursorPosition(this.cursorBeginPos.Left, this.cursorBeginPos.Top + lineOffset);

        int width = Console.WindowWidth;
        if (text.Length > width)
        {
            text = text[..width];
        }

        Console.WriteLine(text.PadRight(width));
    }

    private void OnCommandEraseTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        this.PrintCommand(string.Empty);
    }

    private void SkipForward()
    {
        if (this.album is null)
        {
            return;
        }

        this.PrintCommand($"Seek Forward 15 secs");

        this.player.SkipForward(TimeSpan.FromSeconds(15));
    }

    private void SkipBackward()
    {
        if (this.album is null)
        {
            return;
        }

        this.PrintCommand($"Seek Back 15 secs");

        this.player.SkipBack(TimeSpan.FromSeconds(15));
    }

    private void ResumeTrack()
    {
        if (this.album is null)
        {
            return;
        }

        this.PrintCommand($"Resume");

        this.player.Resume();
    }

    private void PauseTrack()
    {
        if (this.album is null)
        {
            return;
        }

        this.PrintCommand($"Pause");

        this.player.Pause();
    }

    private void VolumeDown()
    {
        this.volume = Math.Max(0, this.volume - 5);
        this.player.Volume = this.volume;

        this.PrintCommand($"Volume {this.volume}");
    }

    private void VolumeUp()
    {
        this.volume = Math.Min(100, this.volume + 5);
        this.player.Volume = this.volume;

        this.PrintCommand($"Volume {this.volume}");
    }

    private void NextTrack()
    {
        if (this.album is null)
        {
            return;
        }

        if (this.currentTrack + 1 >= this.album.Tracks.Count)
        {
            return;
        }

        this.PrintCommand("Next Track");

        this.PlayTrack(this.currentTrack + 1);
    }

    private void PreviousTrack()
    {
        if (this.album is null)
        {
            return;
        }

        if (this.currentTrack < 1)
        {
            return;
        }

        this.PrintCommand("Previous Track");

        this.PlayTrack(this.currentTrack - 1);
    }

    private void PlayTrack(int trackIndex)
    {
        if (this.album is null)
        {
            return;
        }

        this.currentTrack = trackIndex;
        this.currentTrackPosition = 0;
        this.currentTrackEndPosition = this.album.Tracks[trackIndex].FileSize;

        this.PrintCurrentlyPlayingInfo();

#if PLAY_USING_STREAM
        var trackStream = this.album.Container.OpenFileStream(this.album.Tracks[trackIndex].FileName);
        this.player.Play(trackStream, (int)this.album.Tracks[trackIndex].FileSize);
#else
        var trackData = this.album.Container.ReadFileBytes(this.album.Tracks[trackIndex].FileName);
        this.player.Play(trackData);
#endif
    }

    private void Played(byte[] buffer, int count, int position, bool eof)
    {
        if (this.album is null)
        {
            return;
        }

        bool oldIsPlaying = this.isPlaying;

        this.currentTrackPosition = position;
        this.isPlaying = !eof;

        this.PrintCurrentlyPlayingInfo();

        if (eof)
        {
            this.NextTrack();
        }
    }

    private void PrintCurrentlyPlayingInfo()
    {
        if (this.album is null)
        {
            return;
        }

        string trackLength = PositionToString(this.currentTrackEndPosition);
        string trackPos = PositionToString(this.currentTrackPosition);

        StringBuilder sb = new StringBuilder();
        sb.Append($"Track {this.currentTrack + 1:D2} of {this.album.Tracks.Count:D2}");
        if (this.album.Tracks[this.currentTrack].Performer.Length > 0)
        {
            sb.Append(" - ");
            sb.Append(this.album.Tracks[this.currentTrack].Performer);
        }

        if (this.album.Tracks[this.currentTrack].Title.Length > 0)
        {
            sb.Append(" - ");
            sb.Append(this.album.Tracks[this.currentTrack].Title);
        }

        string trackNumberAndTitle = sb.ToString();
        string trackTime = $"[{trackPos}/{trackLength}]";

        // Avoid re-printing the same thing to avoid flicker
        if (trackTime != this.previousTrackTime)
        {
            this.PrintTrackTime(trackTime);
            this.previousTrackTime = trackTime;
        }

        if (trackNumberAndTitle != this.previousTrackNumberAndTitle)
        {
            this.PrintTrackNumberAndTitle(trackNumberAndTitle);
            this.previousTrackNumberAndTitle = trackNumberAndTitle;
        }
    }
}
