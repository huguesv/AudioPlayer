// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core;

using Woohoo.Audio.Core.Cue;
using Woohoo.Audio.Core.IO;
using Woohoo.Audio.Core.Lyrics;

public sealed class AlbumTrack
{
    public required IMusicContainer Container { get; init; }

    public required CueSheet CueSheet { get; init; }

    public string CueSheetName { get; set; } = string.Empty;

    public string CueSheetFileName { get; set; } = string.Empty;

    public string CueSheetContainerPath { get; set; } = string.Empty;

    public string AlbumTitle { get; set; } = string.Empty;

    public string AlbumPerformer { get; set; } = string.Empty;

    public string TrackFileName { get; set; } = string.Empty;

    public bool TrackFileNotFound { get; set; }

    public int TrackOffset { get; set; }

    public int TrackSize { get; set; }

    public int TrackNumber { get; set; }

    public string TrackTitle { get; set; } = string.Empty;

    public string TrackPerformer { get; set; } = string.Empty;

    public string TrackSongwriter { get; set; } = string.Empty;

    public LyricsTrack? Lyrics { get; set; }
}
