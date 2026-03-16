// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Internal.IO;

using Woohoo.Audio.Core.Lyrics;
using Woohoo.Audio.Core.Media;
using Woohoo.IO.Compression.Chd;

internal class ChdAlbumTrack : IAlbumTrack
{
    private readonly CdRomFile cdRomFile;
    private readonly int trackIndex;
    private readonly CdRomTrackType trackType;

    public ChdAlbumTrack(CdRomFile cdRomFile, string mediaName, int trackIndex, CdRomTrackType trackType, int size)
    {
        this.cdRomFile = cdRomFile;
        this.MediaName = mediaName;
        this.trackIndex = trackIndex;
        this.trackType = trackType;
        this.TrackSize = size;
    }

    public int TrackNumber => this.trackIndex + 1;

    public bool IsAudio => this.trackType == CdRomTrackType.Audio;

    public int TrackSize { get; }

    public bool TrackFileNotFound => false;

    public string MediaName { get; }

    public string AlbumTitle { get; set; } = string.Empty;

    public string AlbumPerformer { get; set; } = string.Empty;

    public string TrackTitle { get; set; } = string.Empty;

    public string TrackPerformer { get; set; } = string.Empty;

    public string TrackSongwriter { get; set; } = string.Empty;

    public LyricsTrack? Lyrics { get; set; }

    public Stream OpenStream()
    {
        return this.cdRomFile.OpenStream(this.trackIndex);
    }
}
