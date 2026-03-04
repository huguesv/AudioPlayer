// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.Chd;

public enum CdRomTrackType
{
    /// <summary>
    /// mode 1 2048 bytes/sector
    /// </summary>
    Mode1 = 0,

    /// <summary>
    /// mode 1 2352 bytes/sector
    /// </summary>
    Mode1Raw = 1,

    /// <summary>
    /// mode 2 2336 bytes/sector
    /// </summary>
    Mode2 = 2,

    /// <summary>
    /// mode 2 2048 bytes/sector
    /// </summary>
    Mode2Form1 = 3,

    /// <summary>
    /// mode 2 2324 bytes/sector
    /// </summary>
    Mode2Form2 = 4,

    /// <summary>
    /// mode 2 2336 bytes/sector
    /// </summary>
    Mode2FormMix = 5,

    /// <summary>
    /// mode 2 2352 bytes/sector
    /// </summary>
    Mode2Raw = 6,

    /// <summary>
    /// redbook audio track 2352 bytes/sector (588 samples)
    /// </summary>
    Audio = 7,

    /// <summary>
    /// special flag for cdrom_read_data: just return me whatever is there
    /// </summary>
    RawDontCare = 8,
}
