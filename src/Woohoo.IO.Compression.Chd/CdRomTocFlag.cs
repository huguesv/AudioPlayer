// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.Chd;

[Flags]
public enum CdRomTocFlag
{
    /// <summary>
    /// disc is a GD-ROM, all tracks should be stored with GD-ROM metadata
    /// </summary>
    GdRom = 1,

    /// <summary>
    /// legacy GD-ROM, with little-endian CDDA data
    /// </summary>
    GdRomLE = 2,

    /// <summary>
    /// multisession CD-ROM
    /// </summary>
    MultiSession = 4,
}
