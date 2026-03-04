namespace Woohoo.IO.Compression.Chd.Internal;

internal class ChdHeader
{
    public ChdCodecType[] CodecTypes { get; set; } = [];
    public ChdHunkReader?[] HunkReaders { get; set; } = [];

    public ulong Totalbytes { get; set; }
    public uint BlockSize { get; set; }
    public uint TotalBlocks { get; set; }

    public ChdMapEntry[] MapEntries { get; set; } = [];

    // just compressed data
    public byte[]? Md5 { get; set; }

    // just compressed data
    public byte[]? RawSha1 { get; set; }

    // includes the meta data
    public byte[]? Sha1 { get; set; }

    public byte[]? ParentMd5 { get; set; }
    public byte[]? ParentSha1 { get; set; }

    public ulong MetaOffset { get; set; }
}
