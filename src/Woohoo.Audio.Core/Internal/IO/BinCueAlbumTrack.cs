// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Internal.IO;

using Woohoo.Audio.Core.Internal.Cue;
using Woohoo.Audio.Core.Lyrics;
using Woohoo.Audio.Core.Media;

internal class BinCueAlbumTrack : IAlbumTrack
{
    private readonly IFolder container;
    private readonly string fileName;
    private readonly string trackMode;
    private readonly int offset;

    public BinCueAlbumTrack(IFolder container, int trackNumber, string trackMode, string fileName, bool trackFileNotFound, string mediaName, int offset, int size)
    {
        this.container = container;
        this.TrackNumber = trackNumber;
        this.trackMode = trackMode;
        this.fileName = fileName;
        this.TrackFileNotFound = trackFileNotFound;
        this.MediaName = mediaName;
        this.offset = offset;
        this.TrackSize = size;
    }

    public int TrackNumber { get; }

    public bool IsAudio => this.trackMode == KnownTrackModes.Audio;

    public int TrackSize { get; }

    public bool TrackFileNotFound { get; }

    public string MediaName { get; }

    public string AlbumTitle { get; set; } = string.Empty;

    public string AlbumPerformer { get; set; } = string.Empty;

    public string TrackTitle { get; set; } = string.Empty;

    public string TrackPerformer { get; set; } = string.Empty;

    public string TrackSongwriter { get; set; } = string.Empty;

    public LyricsTrack? Lyrics { get; set; }

    public Stream OpenStream()
    {
        var fileStream = this.container.OpenFileStream(this.fileName);
        return new SubStream(fileStream, this.offset, this.TrackSize);
    }
}
