namespace Woohoo.IO.Compression.Chd.Internal;

internal enum ChdCompressionType
{
    /// <summary>
    /// Codec 0
    /// </summary>
    Type0 = 0,

    /// <summary>
    /// Codec 1
    /// </summary>
    Type1 = 1,

    /// <summary>
    /// Codec 2
    /// </summary>
    Type2 = 2,

    /// <summary>
    /// Codec 3
    /// </summary>
    Type3 = 3,

    /// <summary>
    /// no compression; implicit length = hunkbytes
    /// </summary>
    None = 4,

    /// <summary>
    /// same as another block in this chd
    /// </summary>
    Self = 5,

    /// <summary>
    /// same as a hunk's worth of units in the parent chd
    /// </summary>
    Parent = 6,

    /// <summary>
    /// start of small RLE run (4-bit length)
    /// </summary>
    RleSmall = 7,

    /// <summary>
    /// start of large RLE run (8-bit length)
    /// </summary>
    RleLarge = 8,

    /// <summary>
    /// same as the last Self block
    /// </summary>
    Self0 = 9,

    /// <summary>
    /// same as the last Self block + 1
    /// </summary>
    Self1 = 10,

    /// <summary>
    /// same block in the parent
    /// </summary>
    ParentSelf = 11,

    /// <summary>
    /// same as the last Parent block
    /// </summary>
    Parent0 = 12,

    /// <summary>
    /// same as the last Parent block + 1
    /// </summary>
    Parent1 = 13,

    /// <summary>
    /// used in CHD V3 and V4
    /// </summary>
    Mini = 100,

    /// <summary>
    /// an internal error state
    /// </summary>
    Error = 101,
}
