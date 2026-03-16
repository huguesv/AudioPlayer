// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.Chd.Internal;

internal class CdRomConstants
{
    /// <summary>
    /// tracks are padded to a multiple of this many frames
    /// </summary>
    public const uint TrackPadding = 4;

    public const uint MaxTracks = 99;
    public const uint MaxSectorData = 2352;
    public const uint MaxSubCodeData = 96;
    public const uint MaxIndex = 99;
    public const uint FrameSize = MaxSectorData + MaxSubCodeData;
    public const uint FramesPerHunk = 8;
    public const uint MetadataWords = 1 + (MaxTracks * 6);
}
