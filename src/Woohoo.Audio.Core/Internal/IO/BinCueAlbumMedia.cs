// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Internal.IO;

using System.Collections.Generic;
using System.Collections.Immutable;
using Woohoo.Audio.Core.Internal.Cue;
using Woohoo.Audio.Core.Media;

internal class BinCueAlbumMedia : IAlbumMedia
{
    private readonly CueSheet cueSheet;
    private readonly IFolder folder;

    public BinCueAlbumMedia(CueSheet cueSheet, IFolder folder, IEnumerable<IAlbumTrack> tracks, string toc)
    {
        this.cueSheet = cueSheet;
        this.folder = folder;
        this.Tracks = [.. tracks];
        this.CTDBToc = toc;
    }

    public string CTDBToc { get; }

    public ImmutableArray<IAlbumTrack> Tracks { get; }
}
