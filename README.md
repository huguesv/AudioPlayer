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

1. Select either a .cue file or a .zip file that contains a .cue file. See the
   next section for how to dump your own audio CDs to bin/cue files format.

1. A new playlist that consists of the audio tracks from the .cue file will be
   opened and the first track will start playing.

1. You can only load one album at a time. When you load another, the current
   playlist is replaced with the tracks from the new album.

1. Click the **Change View** button (also **View** menu on MacOS) to switch
   between different views: currently playing, playlist, waveform view and
   spectrum views.

## Usage (Console Player)

1. Open a terminal window.
1. Run the executable and pass in a single argument that is a path to a .cue
   file or a .zip file that contains a .cue file.
   ```shell
   Woohoo.Audio.Player.Cli "Guacamelee! Super Turbo Championship Edition (USA) (Official Soundtrack) (PC Game Bundle).zip"
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

Install the [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0).

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
