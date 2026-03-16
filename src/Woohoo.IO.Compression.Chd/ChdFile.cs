// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.Chd;

using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Woohoo.IO.Compression.Chd.Internal;
using Woohoo.IO.Compression.Chd.Internal.Utils;

public sealed class ChdFile : IDisposable
{
    public static readonly ChdMetadataTag CdRomTrackMetadataTag = new('C', 'H', 'T', 'R');
    public static readonly ChdMetadataTag CdRomTrackMetadata2Tag = new('C', 'H', 'T', '2');
    public static readonly ChdMetadataTag GdRomTrackMetadataOldTag = new('C', 'H', 'G', 'T');
    public static readonly ChdMetadataTag GdRomTrackMetadataTag = new('C', 'H', 'G', 'D');

    private static readonly uint[] ExpectedHeaderLengths = [0, 76, 80, 120, 108, 124];
    private static readonly byte[] ExpectedSignature = [(byte)'M', (byte)'C', (byte)'o', (byte)'m', (byte)'p', (byte)'r', (byte)'H', (byte)'D'];

    private readonly Stream stream;
    private readonly bool keepStreamOpen;
    private readonly ChdHeader header;
    private readonly ImmutableArray<MetadataTagAndValue> metadata;
    private readonly ArrayPool<byte> bufferPool;

    /// <summary>
    /// single-hunk cache for partial reads/writes
    /// </summary>
    private readonly byte[] cacheData;

    /// <summary>
    /// Which hunk is in the cache?
    /// </summary>
    private uint? cacheHunk;

    private bool disposedValue;

    private class MetadataTagAndValue
    {
        public ChdMetadataTag Tag { get; }
        public string? StringVal { get; }
        public byte[]? BinaryVal { get; }

        public MetadataTagAndValue(ChdMetadataTag tag, string val)
        {
            this.Tag = tag;
            this.StringVal = val;
        }

        public MetadataTagAndValue(ChdMetadataTag tag, byte[] val)
        {
            this.Tag = tag;
            this.BinaryVal = val;
        }
    }

    public ChdFile(string filePath)
        : this(new FileStream(filePath, FileMode.Open, FileAccess.Read), keepOpen: false)
    {
        this.FilePath = filePath;
    }

    public ChdFile(Stream chdStream, bool keepOpen)
    {
        ArgumentNullException.ThrowIfNull(chdStream);

        this.stream = chdStream;
        this.keepStreamOpen = keepOpen;

        this.header = ReadHeader(this.stream);

        this.bufferPool = ArrayPool<byte>.Shared;
        this.cacheData = new byte[this.header.BlockSize];

        this.metadata = [.. ReadAllMetaData(this.stream, this.header)];

        this.header.HunkReaders = new ChdHunkReader[this.header.CodecTypes.Length];
        for (int i = 0; i < this.header.CodecTypes.Length; i++)
        {
            this.header.HunkReaders[i] = GetHunkReaderFromCodec(this.header.CodecTypes[i]);
        }
    }

    public string? FilePath { get; }

    public void Dispose()
    {
        if (!this.disposedValue)
        {
            if (!this.keepStreamOpen)
            {
                this.stream.Dispose();
            }

            this.disposedValue = true;
        }

        GC.SuppressFinalize(this);
    }

    public void ReadBytes(ulong offset, byte[] buffer, uint count)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentOutOfRangeException.ThrowIfGreaterThan((int)count, buffer.Length);
        ObjectDisposedException.ThrowIf(this.disposedValue, this);

        uint firstHunk = (uint)(offset / this.header.BlockSize);
        uint lastHunk = (uint)(offset + count - 1) / this.header.BlockSize;

        uint bufferPos = 0;
        for (uint curHunk = firstHunk; curHunk <= lastHunk; curHunk++)
        {
            uint startOffs = curHunk == firstHunk ? (uint)(offset % this.header.BlockSize) : 0;
            uint endOffs = curHunk == lastHunk ? (uint)((offset + count - 1) % this.header.BlockSize) : this.header.BlockSize - 1;

            if (startOffs == 0 && endOffs == (this.header.BlockSize - 1) && curHunk != this.cacheHunk)
            {
                // if it's a full block, just read directly from disk unless it's the cached hunk
                this.ReadHunk(curHunk, buffer, bufferPos);
            }
            else
            {
                // otherwise, read from the cache
                if (curHunk != this.cacheHunk)
                {
                    this.ReadHunk(curHunk, this.cacheData);
                    this.cacheHunk = curHunk;
                }

                Array.Copy(this.cacheData, startOffs, buffer, bufferPos, endOffs + 1 - startOffs);
            }

            // advance
            bufferPos += endOffs + 1 - startOffs;
        }
    }

    public void ReadHunk(uint hunkNum, byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        ObjectDisposedException.ThrowIf(this.disposedValue, this);

        this.ReadHunk(hunkNum, buffer, 0);
    }

    public void ReadHunk(uint hunkNum, byte[] buffer, long offset)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(offset, buffer.Length);
        ObjectDisposedException.ThrowIf(this.disposedValue, this);

        var codec = new ChdCodec();

        var mapEntry = this.header.MapEntries[hunkNum];

        // read the block into the cache
        if (offset > 0)
        {
            // unfortunately right now ReadBlock does not allow specifying an
            // offset into the dest buffer so we have to use a temp buffer.
            var rentedBuffer = this.bufferPool.Rent((int)this.header.BlockSize);
            try
            {
                this.ReadBlock(mapEntry, this.header.HunkReaders, codec, rentedBuffer, (int)this.header.BlockSize);
                Array.Copy(rentedBuffer, 0, buffer, offset, this.header.BlockSize);
            }
            finally
            {
                this.bufferPool.Return(rentedBuffer);
            }
        }
        else
        {
            this.ReadBlock(mapEntry, this.header.HunkReaders, codec, buffer, (int)this.header.BlockSize);
        }
    }

    public bool TryGetMetadata(ChdMetadataTag searchTag, uint searchIndex, [NotNullWhen(true)] out string? output)
    {
        ObjectDisposedException.ThrowIf(this.disposedValue, this);

        if (searchIndex < this.metadata.Length)
        {
            var tag = this.metadata[(int)searchIndex].Tag;
            if (tag.Equals(searchTag))
            {
                output = this.metadata[(int)searchIndex].StringVal ?? string.Empty;
                return true;
            }
            else
            {
                output = null;
                return false;
            }
        }

        output = null;
        return false;
    }

    public bool TryGetMetadata(ChdMetadataTag searchTag, uint searchIndex, [NotNullWhen(true)] out byte[]? output)
    {
        ObjectDisposedException.ThrowIf(this.disposedValue, this);

        if (searchIndex < this.metadata.Length)
        {
            output = this.metadata[(int)searchIndex].BinaryVal ?? [];
            return true;
        }

        output = null;
        return false;
    }

    private void ReadBlock(ChdMapEntry mapEntry, ChdHunkReader?[] compression, ChdCodec codec, byte[] buffOut, int buffOutLength)
    {
        bool checkCrc = true;

        switch (mapEntry.CompressionType)
        {
            case ChdCompressionType.Type0:
            case ChdCompressionType.Type1:
            case ChdCompressionType.Type2:
            case ChdCompressionType.Type3:
                {
                    lock (mapEntry)
                    {
                        var buffIn = this.bufferPool.Rent((int)this.header.BlockSize);
                        try
                        {
                            this.stream.Seek((long)mapEntry.CompressedDataOffsetOrSelfBlockIndex, SeekOrigin.Begin);
                            this.stream.ReadExactly(buffIn, 0, (int)mapEntry.CompressedDataLength);

                            var chdHunkReader = compression[(int)mapEntry.CompressionType];
                            if (chdHunkReader is null)
                            {
                                throw new InvalidDataException();
                            }

                            chdHunkReader.Invoke(buffIn, (int)mapEntry.CompressedDataLength, buffOut, buffOutLength, codec);
                        }
                        finally
                        {
                            this.bufferPool.Return(buffIn);
                        }
                    }

                    break;
                }
            case ChdCompressionType.None:
                {
                    lock (mapEntry)
                    {
                        this.stream.Seek((long)mapEntry.CompressedDataOffsetOrSelfBlockIndex, SeekOrigin.Begin);
                        this.stream.ReadExactly(buffOut, 0, (int)Math.Min(mapEntry.CompressedDataLength, buffOutLength));
                    }

                    break;
                }

            case ChdCompressionType.Mini:
                {
                    byte[] tmp = BitConverter.GetBytes(mapEntry.CompressedDataOffsetOrSelfBlockIndex);
                    for (int i = 0; i < 8; i++)
                    {
                        buffOut[i] = tmp[7 - i];
                    }

                    for (int i = 8; i < buffOutLength; i++)
                    {
                        buffOut[i] = buffOut[i - 8];
                    }

                    break;
                }

            case ChdCompressionType.Self:
                {
                    var selfMapEntry = this.header.MapEntries[mapEntry.CompressedDataOffsetOrSelfBlockIndex];

                    this.ReadBlock(selfMapEntry, compression, codec, buffOut, buffOutLength);

                    // check CRC in the read_block_into_cache call
                    checkCrc = false;
                    break;
                }
            default:
                throw new InvalidDataException();
        }

        if (checkCrc)
        {
            if (mapEntry.Crc32 != null && !CRC.VerifyDigest((uint)mapEntry.Crc32, buffOut, 0, (uint)buffOutLength))
            {
                throw new InvalidDataException("Checksum mismatch.");
            }

            if (mapEntry.Crc16 != null && CRC16.calc(buffOut, (int)buffOutLength) != mapEntry.Crc16)
            {
                throw new InvalidDataException("Checksum mismatch.");
            }
        }
    }

    private static ChdHeader ReadHeader(Stream stream)
    {
        uint version = ReadPreHeader(stream);
        return version switch
        {
            1 => ChdHeaderReader.ReadHeaderV1(stream),
            2 => ChdHeaderReader.ReadHeaderV2(stream),
            3 => ChdHeaderReader.ReadHeaderV3(stream),
            4 => ChdHeaderReader.ReadHeaderV4(stream),
            5 => ChdHeaderReader.ReadHeaderV5(stream),
            _ => throw new NotSupportedException($"Unsupported CHD version: {version}"),
        };
    }

    private static uint ReadPreHeader(Stream stream)
    {
        for (int i = 0; i < ExpectedSignature.Length; i++)
        {
            byte b = (byte)stream.ReadByte();
            if (b != ExpectedSignature[i])
            {
                throw new InvalidDataException("CHD signature not found.");
            }
        }

        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        uint length = reader.ReadUInt32BE();
        uint version = reader.ReadUInt32BE();
        if (ExpectedHeaderLengths[version] != length)
        {
            throw new InvalidDataException("CHD header invalid.");
        }

        return version;
    }

    private static List<MetadataTagAndValue> ReadAllMetaData(Stream stream, ChdHeader header)
    {
        var result = new List<MetadataTagAndValue>();

        using var binReader = new BinaryReader(stream, Encoding.UTF8, true);

        // loop over the metadata, until metaoffset=0
        while (header.MetaOffset != 0)
        {
            stream.Seek((long)header.MetaOffset, SeekOrigin.Begin);
            uint metaTag = binReader.ReadUInt32BE();
            uint metaLength = binReader.ReadUInt32BE();
            ulong metaNext = binReader.ReadUInt64BE();
            uint metaFlags = metaLength >> 24;
            metaLength &= 0x00ffffff;

            byte[] metaData = new byte[metaLength];
            stream.ReadExactly(metaData, 0, metaData.Length);

            var tag = new ChdMetadataTag(metaTag);
            if (IsAscii(metaData))
            {
                string val = metaData[^1] == 0
                    ? Encoding.ASCII.GetString(metaData[..^1])
                    : Encoding.ASCII.GetString(metaData);
                var tagAndVal = new MetadataTagAndValue(tag, val);
                result.Add(tagAndVal);
            }
            else
            {
                var tagAndVal = new MetadataTagAndValue(tag, metaData);
                result.Add(tagAndVal);
            }

            // set location of next meta data entry in the CHD (set to 0 if finished.)
            header.MetaOffset = metaNext;
        }

        return result;

        static bool IsAscii(byte[] bytes)
        {
            return bytes.All(b => b == 0 || b >= 32);
        }
    }

    private static ChdHunkReader? GetHunkReaderFromCodec(ChdCodecType codec)
    {
        return codec switch
        {
            ChdCodecType.Zlib => ChdHunkReaders.Zlib,
            ChdCodecType.Zstd => ChdHunkReaders.Zstd,
            ChdCodecType.Lzma => ChdHunkReaders.Lzma,
            ChdCodecType.Huffman => ChdHunkReaders.Huffman,
            ChdCodecType.flac => ChdHunkReaders.Flac,
            ChdCodecType.CdZlib => ChdHunkReaders.CdZlib,
            ChdCodecType.CdZstd => ChdHunkReaders.CdZstd,
            ChdCodecType.CdLzma => ChdHunkReaders.CdLzma,
            ChdCodecType.CdFlac => ChdHunkReaders.CdFlac,
            ChdCodecType.Avhuff => ChdHunkReaders.avHuff,
            _ => null,
        };
    }
}
