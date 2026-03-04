namespace Woohoo.IO.Compression.Chd.Internal;

internal class ChdMapEntry
{
    public ChdCompressionType CompressionType { get; set; }

    public uint CompressedDataLength { get; set; }

    public ulong CompressedDataOffsetOrSelfBlockIndex { get; set; }

    // V3 & V4
    public uint? Crc32 { get; set; } = null;

    // V5
    public ushort? Crc16 { get; set; } = null;
}
