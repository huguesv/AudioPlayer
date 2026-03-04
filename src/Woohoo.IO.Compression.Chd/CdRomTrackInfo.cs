// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.Chd;

public sealed class CdRomTrackInfo
{
    /// <summary>
    /// Track type.
    /// </summary>
    public CdRomTrackType TrackType { get; internal set; }

    /// <summary>
    /// Subcode data type.
    /// </summary>
    public CdRomSubType SubType { get; internal set; }

    /// <summary>
    /// Size of data in each sector of this track.
    /// </summary>
    public uint DataSize { get; internal set; }

    /// <summary>
    /// Size of subchannel data in each sector of this track.
    /// </summary>
    public uint SubSize { get; internal set; }

    /// <summary>
    /// Number of frames in this track.
    /// </summary>
    public uint Frames { get; internal set; }

    /// <summary>
    /// Number of "spillage" frames in this track.
    /// </summary>
    public uint ExtraFrames { get; internal set; }

    /// <summary>
    /// Number of pregap frames.
    /// </summary>
    public uint Pregap { get; internal set; }

    /// <summary>
    /// Number of postgap frames.
    /// </summary>
    public uint Postgap { get; internal set; }

    /// <summary>
    /// Type of sectors in pregap.
    /// </summary>
    public CdRomTrackType PregapType { get; internal set; }

    /// <summary>
    /// Type of subchannel data in pregap.
    /// </summary>
    public CdRomSubType PregapSub { get; internal set; }

    /// <summary>
    /// Size of data in each sector of the pregap.
    /// </summary>
    public uint PregapDataSize { get; internal set; }

    /// <summary>
    /// Size of subchannel data in each sector of the pregap.
    /// </summary>
    public uint PregapSubSize { get; internal set; }

    /// <summary>
    /// Metadata flags associated with each track.
    /// </summary>
    /// <remarks>
    /// Only used for cue sheets scenario, not chd.
    /// </remarks>
    public uint ControlFlags { get; internal set; }

    /// <summary>
    /// Session number.
    /// </summary>
    /// <remarks>
    /// Only used for cue sheets scenario, not chd.
    /// </remarks>
    public uint Session { get; internal set; }

    /// <summary>
    /// Number of frames of padding to add to the end of the track; needed for GDI.
    /// </summary>
    /// <remarks>
    /// Used in CHDMAN only.
    /// </remarks>
    public uint PadFrames { get; internal set; }

    /// <summary>
    /// Number of frames from the next file to add to the end of the current
    /// track after padding; needed for Redump split-bin GDI.
    /// </summary>
    /// <remarks>
    /// Used in CHDMAN only.
    /// </remarks>
    public uint SplitFrames { get; internal set; }

    /// <summary>
    /// Logical frame of actual track data - offset by pregap size if pregap
    /// not physically present.
    /// </summary>
    /// <remarks>
    /// Used in MAME/MESS only.
    /// </remarks>
    public uint LogFrameOffset { get; internal set; }

    /// <summary>
    /// Physical frame of actual track data in CHD data.
    /// </summary>
    /// <remarks>
    /// Used in MAME/MESS only.
    /// </remarks>
    public uint PhysFrameOffset { get; internal set; }

    /// <summary>
    /// Frame number this track starts at on the CHD.
    /// </summary>
    /// <remarks>
    /// Used in MAME/MESS only.
    /// </remarks>
    public uint ChdFrameOffset { get; internal set; }

    /// <summary>
    /// Number of frames from logframeofs until end of track data.
    /// </summary>
    /// <remarks>
    /// Used in MAME/MESS only.
    /// </remarks>
    public uint LogFrames { get; internal set; }

    /// <summary>
    /// Fields used in multi-cue GDI.
    /// </summary>
    public uint MultiCueArea { get; internal set; }
}
