// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.Chd;

using Woohoo.IO.Compression.Chd.Internal;

public sealed class CdRomToc
{
    public CdRomToc()
    {
        this.Tracks = new CdRomTrackInfo[CdRomConstants.MaxTracks + 1];

        for (int i = 0; i < this.Tracks.Length; i++)
        {
            this.Tracks[i] = new CdRomTrackInfo();
        }
    }

    /// <summary>
    /// Number of tracks.
    /// </summary>
    public uint NumTracks { get; internal set; }

    /// <summary>
    /// Number of sessions.
    /// </summary>
    public uint NumSessions { get; internal set; }

    public CdRomTocFlag Flags { get; internal set; }

    public CdRomTrackInfo[] Tracks { get; }
}
