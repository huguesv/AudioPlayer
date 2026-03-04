// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Internal.IO;

using System.Collections.Generic;
using System.Collections.Immutable;
using Woohoo.Audio.Core.Media;
using Woohoo.IO.Compression.Chd;

internal class ChdAlbumMedia : IAlbumMedia
{
    private readonly CdRomFile cdRomFile;

    public ChdAlbumMedia(CdRomFile cdRomFile, IEnumerable<IAlbumTrack> tracks, string toc)
    {
        this.cdRomFile = cdRomFile;
        this.Tracks = [.. tracks];
        this.CTDBToc = toc;
    }

    public string CTDBToc { get; }

    public ImmutableArray<IAlbumTrack> Tracks { get; }
}
