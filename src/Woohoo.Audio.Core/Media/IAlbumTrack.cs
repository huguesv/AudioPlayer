// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Media;

using Woohoo.Audio.Core.Lyrics;

public interface IAlbumTrack
{
    int TrackNumber { get; }

    bool IsAudio { get; }

    int TrackSize { get; }

    bool TrackFileNotFound { get; }

    string MediaName { get; }

    string AlbumTitle { get; set; }

    string AlbumPerformer { get; set; }

    string TrackTitle { get; set; }

    string TrackPerformer { get; set; }

    string TrackSongwriter { get; set; }

    LyricsTrack? Lyrics { get; set; }

    Stream OpenStream();
}
