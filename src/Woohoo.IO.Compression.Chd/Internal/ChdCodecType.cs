namespace Woohoo.IO.Compression.Chd.Internal;

internal enum ChdCodecType
{
    None = 0,
    Zlib = 0x7A6C6962,
    Zstd = 0x7A737464,
    Lzma = 0x6C7A6D61,
    Huffman = 0x68756666,
    flac = 0x666C6163,
    CdZlib = 0x63647A6C,
    CdZstd = 0x63647A73,
    CdLzma = 0x63646C7A,
    CdFlac = 0x6364666C,
    Avhuff = 0x61766875,
    Error = 0x0eeeeeee,
}
