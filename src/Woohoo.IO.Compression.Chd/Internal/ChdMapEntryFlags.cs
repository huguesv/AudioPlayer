namespace Woohoo.IO.Compression.Chd.Internal;

using System;

[Flags]
internal enum ChdMapEntryFlags
{
    /// <summary>
    /// what type of hunk
    /// </summary>
    TypeMask = 0x000f,

    /// <summary>
    /// no CRC is present
    /// </summary>
    FlagNoCrc = 0x0010,

    /// <summary>
    /// invalid type
    /// </summary>
    Invalid = 0x0000,

    /// <summary>
    /// standard compression
    /// </summary>
    Compressed = 0x0001,

    /// <summary>
    /// uncompressed data
    /// </summary>
    Uncompressed = 0x0002,

    /// <summary>
    /// mini: use offset as raw data
    /// </summary>
    Mini = 0x0003,

    /// <summary>
    /// same as another hunk in this file
    /// </summary>
    SelfHunk = 0x0004,

    /// <summary>
    /// same as a hunk in the parent file
    /// </summary>
    ParentHunk = 0x0005,
}
