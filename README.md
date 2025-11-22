# Audio Player

![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/huguesv/AudioPlayer/build-and-test.yml)

This is an audio player for Windows, Linux and MacOS.

It is designed to play audio CDs dumped to raw bin/cue files.

The bin/cue files can be loaded from a folder on disk, or from a zip archive.

This player provides a convenient way of listening to dumps in that format
without any additional step(s):

- No need to mount the cue file as a virtual drive.
- No need to extract the cue/bin files from zip files.
- No need to convert the data to wav/flac/ogg/mp3.

This is currently NOT supported:

- Playing from archives in 7z and rar formats.
- Playing music from other formats such as wav, mp3, flac, ogg, etc. Just use
  a regular music player for that.

Album and track metadata is loaded from the CDTEXT information when
present in cue file.

Additional metadata is optionally retrieved from [CueToolsDB](https://db.cue.tools/),
including album art.

Lyrics are optionally retrieved from [LRCLIB](https://lrclib.net).
Using a local LRCLIB sqlite3 database is also supported.

## Desktop Player

![Audio Player on Windows Screenshot](images/windows-dark-nowplaying.png?raw=true "Audio Player on Windows Screenshot")

## Console Player

![Windows Terminal](images/windows-cli.png?raw=true "Windows Terminal")

For more screenshots, see the [SCREENSHOTS.md](SCREENSHOTS.md) file.

## Compatibility

The application has been tested on the following operating systems, but may
work on earlier versions.

- Windows 11 24H2 (x64, ARM64)
- MacOS 15.3 (Apple Silicon)
- Ubuntu 24.10 (x64, ARM64)

The Linux version is currently not as polished as the Windows and MacOS versions.

## Releases

Download the [latest release here](https://github.com/huguesv/AudioPlayer/releases/latest).

Binaries are not available for MacOS yet. You'll need to [build it from sources](#building).

Windows may prevent you from launching the application, since it is not signed.
You can still run it by clicking on "More info" and then "Run anyway".

## Usage (Desktop Player)

1. Click the **Open cue or zip file** button at the bottom left of the application.
   Also available from the **File** menu on MacOS.

1. Select either a .cue file or a .zip file that contains a .cue file.

1. A new playlist that consists of the audio tracks from the .cue file will be
   opened and the first track will start playing.

1. You can only load one album at a time. When you load another, the current
   playlist is replaced with the tracks from the new album.

1. Click the **Change View** button (also **View** menu on MacOS) to switch
   between different views: currently playing, playlist, waveform view and
   spectrum views.

1. Click the **Settings** button (also **Settings** menu on MacOS) to change
   settings such as **Fetch Online Metadata** and **Show Album Art**.

1. Click the **Settings** button (also **Settings** menu on MacOS) to change
   **Fetch Lyrics** setting.
   Note that this requires metadata to be available for your tracks, either
   from CDTEXT in the cue file, or from CueToolsDB.

### Lyrics Configuration

Lyrics are fetched from [LRCLIB](https://lrclib.net) using their API.

You can optionally use a local version of the LRCLIB database:

1. Download a [dump of the latest database](https://lrclib.net/db-dumps).
   Warning: this is a VERY large (~20GB).
1. Extract the .sqlite3 file from the downloaded .gz file.
1. Set the path to the .sqlite3 file in `LRCLIB_DB_PATH` environment variable.
1. Restart the application.

## Usage (Console Player)

1. Open a terminal window.

1. For help on command line options, run:
   ```shell
   Woohoo.Audio.Player.Cli -h
   ```

1. Run the executable and pass a path to a .cue file or a .zip file that contains
   a .cue file.
   ```shell
   Woohoo.Audio.Player.Cli "Life Is Strange - Before the Storm - Original Soundtrack (USA, Europe) (PS4 Game Bundle).zip"
   ```

1. Optionally pass in `-m` or `--metadata` to fetch metadata from CueToolsDB.
   ```shell
   Woohoo.Audio.Player.Cli -m "Life Is Strange - Before the Storm - Original Soundtrack (USA, Europe) (PS4 Game Bundle).zip"
   ```

1. Optionally pass in `-l` or `--lyrics` to fetch lyrics from LRCLIB.net.
   Note that this requires metadata to be available for your tracks, either
   from CDTEXT in the cue file, or from CueToolsDB.
   ```shell
   Woohoo.Audio.Player.Cli -m -l "Life Is Strange - Before the Storm - Original Soundtrack (USA, Europe) (PS4 Game Bundle).zip"
   ```

1. Optionally pass in `-ldb <path>` or `--lyrics-db <path>` to use a local LRCLIB
   database. A path to a .sqlite3 file must be provided. The latest database dump
   can be downloaded from [here](https://lrclib.net/db-dumps).
   When specified, lyrics will be fetched from the local database first, and fall
   back to the online service if no match is found locally.
   ```shell
   Woohoo.Audio.Player.Cli -m -l -ldb "C:\path\to\lrclib.sqlite3" "Life Is Strange - Before the Storm - Original Soundtrack (USA, Europe) (PS4 Game Bundle).zip"
   ```

1. Press the following keys to control the player:
   - `Q` to quit.
   - `P` to pause playback.
   - `R` to resume playback.
   - `Up` to increase volume.
   - `Down` to decrease volume.
   - `Left` to go to the previous track.
   - `Right` to go to the next track.
   - `-` to seek backward.
   - `+` to seek forward.

## Building

Install the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0).

To build the application, use the following command from the `\src` folder:

```
dotnet build
```

To run the desktop player, use the following command from the `\src` folder:
```
dotnet run --project Woohoo.Audio.Player
```

To run the unit tests, use the following command from the `\src` folder:

```
dotnet test
```

## License and Credits

This software is licensed under the MIT License. See the [LICENSE](LICENSE) file.

Copyright (c) 2025 Hugues Valois. All rights reserved.

This software uses the following libraries:

- [Avalonia](https://github.com/AvaloniaUI/Avalonia)
- [CommunityToolkit](https://github.com/CommunityToolkit/dotnet)
- [FftSharp](https://github.com/swharden/FftSharp)
- [ScottPlot](https://github.com/ScottPlot/ScottPlot)
- [SDL3-CS from ppy](https://github.com/ppy/SDL3-CS)
- [SDL3-CS from flibitijibibo](https://github.com/flibitijibibo/SDL3-CS)
- [SDL3](https://github.com/libsdl-org/SDL)

This software uses assets from:

- [Vidstack](https://www.vidstack.io/)
- [icons-icons.com](https://icon-icons.com/)
  - [Disc Icon Free](https://icon-icons.com/icon/disc/114465)

This software queries metadata from:

- [CueToolsDB](http://db.cue.tools/)

This software queries lyrics from:

- [LRCLIB](https://lrclib.net)
