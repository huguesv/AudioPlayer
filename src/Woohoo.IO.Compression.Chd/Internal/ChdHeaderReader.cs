namespace Woohoo.IO.Compression.Chd.Internal;

using System.Collections.Generic;
using System.IO;
using System.Text;
using Woohoo.IO.Compression.Chd.Internal.Utils;

internal static class ChdHeaderReader
{
    public static ChdHeader ReadHeaderV1(Stream file)
    {
        var header = new ChdHeader();

        using BinaryReader br = new BinaryReader(file, Encoding.UTF8, true);

        header.CodecTypes = [ChdCodecType.Zlib];
        uint flags = br.ReadUInt32BE();
        uint compression = br.ReadUInt32BE();
        header.BlockSize = br.ReadUInt32BE();
        header.TotalBlocks = br.ReadUInt32BE();
        uint cylinders = br.ReadUInt32BE();
        uint heads = br.ReadUInt32BE();
        uint sectors = br.ReadUInt32BE();
        header.Md5 = br.ReadBytes(16);
        header.ParentMd5 = br.ReadBytes(16);


        const int HARD_DISK_SECTOR_SIZE = 512;
        header.Totalbytes = cylinders * heads * sectors * HARD_DISK_SECTOR_SIZE;
        header.BlockSize = header.BlockSize * HARD_DISK_SECTOR_SIZE;

        header.MapEntries = new ChdMapEntry[header.TotalBlocks];

        Dictionary<ulong, int> mapBack = new Dictionary<ulong, int>();

        for (int i = 0; i < header.TotalBlocks; i++)
        {
            ulong tmpu = br.ReadUInt64BE();
            header.MapEntries[i] = new ChdMapEntry();


            if (mapBack.TryGetValue(tmpu, out int v))
            {
                header.MapEntries[i].CompressedDataOffsetOrSelfBlockIndex = (uint)v;
                header.MapEntries[i].CompressedDataLength = 0;
                header.MapEntries[i].CompressionType = ChdCompressionType.Self;
                continue;
            }

            mapBack.Add(tmpu, i);

            header.MapEntries[i].CompressedDataOffsetOrSelfBlockIndex = tmpu & 0xfffffffffff;
            header.MapEntries[i].CompressedDataLength = (uint)(tmpu >> 44);
            header.MapEntries[i].CompressionType = (header.MapEntries[i].CompressedDataLength == header.BlockSize)
                           ? ChdCompressionType.None
                           : ChdCompressionType.Type0;
        }

        return header;
    }

    public static ChdHeader ReadHeaderV2(Stream file)
    {
        var header = new ChdHeader();

        using BinaryReader br = new BinaryReader(file, Encoding.UTF8, true);

        header.CodecTypes = [ChdCodecType.Zlib];
        uint flags = br.ReadUInt32BE();
        uint compression = br.ReadUInt32BE();
        uint blocksizeOld = br.ReadUInt32BE(); // this is now unused
        header.TotalBlocks = br.ReadUInt32BE();
        uint cylinders = br.ReadUInt32BE();
        uint heads = br.ReadUInt32BE();
        uint sectors = br.ReadUInt32BE();
        header.Md5 = br.ReadBytes(16);
        header.ParentMd5 = br.ReadBytes(16);
        header.BlockSize = br.ReadUInt32BE(); // blocksize added to header in V2

        const int HARD_DISK_SECTOR_SIZE = 512;
        header.Totalbytes = cylinders * heads * sectors * HARD_DISK_SECTOR_SIZE;

        header.MapEntries = new ChdMapEntry[header.TotalBlocks];

        var mapBack = new Dictionary<ulong, int>();

        for (int i = 0; i < header.TotalBlocks; i++)
        {
            ulong tmpu = br.ReadUInt64BE();
            header.MapEntries[i] = new ChdMapEntry();


            if (mapBack.TryGetValue(tmpu, out int v))
            {
                header.MapEntries[i].CompressedDataOffsetOrSelfBlockIndex = (uint)v;
                header.MapEntries[i].CompressedDataLength = 0;
                header.MapEntries[i].CompressionType = ChdCompressionType.Self;
                continue;
            }

            mapBack.Add(tmpu, i);

            header.MapEntries[i].CompressedDataOffsetOrSelfBlockIndex = tmpu & 0xfffffffffff;
            header.MapEntries[i].CompressedDataLength = (uint)(tmpu >> 44);
            header.MapEntries[i].CompressionType = (header.MapEntries[i].CompressedDataLength == header.BlockSize)
                           ? ChdCompressionType.None
                           : ChdCompressionType.Type0;
        }

        return header;
    }

    public static ChdHeader ReadHeaderV3(Stream file)
    {
        var header = new ChdHeader();
        using BinaryReader br = new BinaryReader(file, Encoding.UTF8, true);

        uint flags = br.ReadUInt32BE();

        header.CodecTypes = [CompTypeConv(br.ReadUInt32BE())];
        header.TotalBlocks = br.ReadUInt32BE(); // total number of CHD Blocks

        header.Totalbytes = br.ReadUInt64BE();  // total byte size of the image
        header.MetaOffset = br.ReadUInt64BE();

        header.Md5 = br.ReadBytes(16);
        header.ParentMd5 = br.ReadBytes(16);
        header.BlockSize = br.ReadUInt32BE();    // length of a CHD Block
        header.RawSha1 = br.ReadBytes(20);
        header.ParentSha1 = br.ReadBytes(20);

        header.MapEntries = new ChdMapEntry[header.TotalBlocks];

        for (int i = 0; i < header.TotalBlocks; i++)
        {
            header.MapEntries[i] = new ChdMapEntry();
            header.MapEntries[i].CompressedDataOffsetOrSelfBlockIndex = br.ReadUInt64BE();
            header.MapEntries[i].Crc32 = br.ReadUInt32BE();
            header.MapEntries[i].CompressedDataLength = (uint)((br.ReadByte() << 8) | (br.ReadByte() << 0) | (br.ReadByte() << 16));
            ChdMapEntryFlags mapflag = (ChdMapEntryFlags)br.ReadByte();
            header.MapEntries[i].CompressionType = ConvMapFlagstoCompressionType(mapflag);
            if ((mapflag & ChdMapEntryFlags.FlagNoCrc) != 0)
            {
                header.MapEntries[i].Crc32 = null;
            }
        }

        return header;
    }

    public static ChdHeader ReadHeaderV4(Stream file)
    {
        var header = new ChdHeader();
        using BinaryReader br = new BinaryReader(file, Encoding.UTF8, true);

        uint flags = br.ReadUInt32BE();

        header.CodecTypes = [CompTypeConv(br.ReadUInt32BE())];
        header.TotalBlocks = br.ReadUInt32BE(); // total number of CHD Blocks

        header.Totalbytes = br.ReadUInt64BE();  // total byte size of the image
        header.MetaOffset = br.ReadUInt64BE();

        header.BlockSize = br.ReadUInt32BE();    // length of a CHD Block
        header.Sha1 = br.ReadBytes(20);
        header.ParentSha1 = br.ReadBytes(20);
        header.RawSha1 = br.ReadBytes(20);

        header.MapEntries = new ChdMapEntry[header.TotalBlocks];

        for (int i = 0; i < header.TotalBlocks; i++)
        {
            header.MapEntries[i] = new ChdMapEntry();
            header.MapEntries[i].CompressedDataOffsetOrSelfBlockIndex = br.ReadUInt64BE();
            header.MapEntries[i].Crc32 = br.ReadUInt32BE();
            header.MapEntries[i].CompressedDataLength = (uint)(br.ReadUInt16BE() | (br.ReadByte() << 16));
            ChdMapEntryFlags mapflag = (ChdMapEntryFlags)br.ReadByte();
            header.MapEntries[i].CompressionType = ConvMapFlagstoCompressionType(mapflag);
            header.MapEntries[i].Crc32 = null;
        }

        return header;
    }

    public static ChdHeader ReadHeaderV5(Stream file)
    {
        var header = new ChdHeader();
        using BinaryReader br = new BinaryReader(file, Encoding.UTF8, true);

        header.CodecTypes = new ChdCodecType[4];
        for (int i = 0; i < 4; i++)
        {
            header.CodecTypes[i] = (ChdCodecType)br.ReadUInt32BE();
        }

        header.Totalbytes = br.ReadUInt64BE();  // total byte size of the image
        ulong mapoffset = br.ReadUInt64BE();
        header.MetaOffset = br.ReadUInt64BE();

        header.BlockSize = br.ReadUInt32BE();    // length of a CHD Hunk (Block)
        uint unitbytes = br.ReadUInt32BE();
        header.RawSha1 = br.ReadBytes(20);
        header.Sha1 = br.ReadBytes(20);
        header.ParentSha1 = br.ReadBytes(20);

        header.TotalBlocks = (uint)((header.Totalbytes + header.BlockSize - 1) / header.BlockSize);

        bool isCompressed = header.CodecTypes[0] != ChdCodecType.None;
        if (isCompressed)
        {
            header.MapEntries = compressed_v5_map(br, mapoffset, header.TotalBlocks, header.BlockSize, unitbytes);
        }
        else
        {
            header.MapEntries = uncompressed_v5_map(br, mapoffset, header.TotalBlocks, header.BlockSize);
        }

        return header;
    }

    private static ChdMapEntry[] uncompressed_v5_map(BinaryReader br, ulong mapoffset, uint totalblocks, uint blocksize)
    {
        var map = new ChdMapEntry[totalblocks];

        br.BaseStream.Seek((long)mapoffset, SeekOrigin.Begin);

        for (int blockIndex = 0; blockIndex < totalblocks; blockIndex++)
        {
            map[blockIndex] = new ChdMapEntry();
            map[blockIndex].CompressionType = ChdCompressionType.None;
            map[blockIndex].CompressedDataLength = blocksize;
            map[blockIndex].CompressedDataOffsetOrSelfBlockIndex = br.ReadUInt32BE() * blocksize;
        }

        return map;
    }

    private static ChdMapEntry[] compressed_v5_map(BinaryReader br, ulong mapoffset, uint totalBlocks, uint blocksize, uint unitbytes)
    {
        var map = new ChdMapEntry[totalBlocks];

        // read the reader
        br.BaseStream.Seek((long)mapoffset, SeekOrigin.Begin);
        uint mapbytes = br.ReadUInt32BE();   //0
        ulong firstoffs = br.ReadUInt48BE(); //4
        ushort mapcrc = br.ReadUInt16BE();   //10
        byte lengthbits = br.ReadByte();     //12
        byte selfbits = br.ReadByte();       //13
        byte parentbits = br.ReadByte();     //14
        br.ReadByte();                       //15 not used

        byte[] compressed_arr = new byte[mapbytes];
        br.BaseStream.ReadExactly(compressed_arr, 0, (int)mapbytes);

        BitStream bitbuf = new BitStream(compressed_arr, 0, (int)mapbytes);

        // first decode the compression types
        HuffmanDecoder decoder = new HuffmanDecoder(16, 8, bitbuf);
        huffman_error err = decoder.ImportTreeRLE();
        if (err != huffman_error.HUFFERR_NONE)
        {
            throw new InvalidDataException();
        }

        int repcount = 0;
        ChdCompressionType lastcomp = 0;
        for (uint blockIndex = 0; blockIndex < totalBlocks; blockIndex++)
        {
            map[blockIndex] = new ChdMapEntry();
            if (repcount > 0)
            {
                map[blockIndex].CompressionType = lastcomp;
                repcount--;
            }
            else
            {
                ChdCompressionType val = (ChdCompressionType)decoder.DecodeOne();
                if (val == ChdCompressionType.RleSmall)
                {
                    map[blockIndex].CompressionType = lastcomp;
                    repcount = 2 + (int)decoder.DecodeOne();
                }
                else if (val == ChdCompressionType.RleLarge)
                {
                    map[blockIndex].CompressionType = lastcomp;
                    repcount = 2 + 16 + ((int)decoder.DecodeOne() << 4);
                    repcount += (int)decoder.DecodeOne();
                }
                else
                {
                    map[blockIndex].CompressionType = lastcomp = val;
                }
            }
        }

        // then iterate through the hunks and extract the needed data
        uint last_self = 0;
        ulong last_parent = 0;
        ulong curoffset = firstoffs;
        for (uint blockIndex = 0; blockIndex < totalBlocks; blockIndex++)
        {
            ulong offset = curoffset;
            uint length = 0;
            ushort crc16 = 0;
            switch (map[blockIndex].CompressionType)
            {
                // base types
                case ChdCompressionType.Type0:
                case ChdCompressionType.Type1:
                case ChdCompressionType.Type2:
                case ChdCompressionType.Type3:
                    curoffset += length = bitbuf.Read(lengthbits);
                    crc16 = (ushort)bitbuf.Read(16);
                    break;

                case ChdCompressionType.None:
                    curoffset += length = blocksize;
                    crc16 = (ushort)bitbuf.Read(16);
                    break;

                case ChdCompressionType.Self:
                    last_self = (uint)(offset = bitbuf.Read(selfbits));
                    break;

                // pseudo-types; convert into base types
                case ChdCompressionType.Self1:
                    last_self++;
                    goto case ChdCompressionType.Self0;

                case ChdCompressionType.Self0:
                    map[blockIndex].CompressionType = ChdCompressionType.Self;
                    offset = last_self;
                    break;

                case ChdCompressionType.ParentSelf:
                    map[blockIndex].CompressionType = ChdCompressionType.Parent;
                    last_parent = offset = ((ulong)blockIndex) * ((ulong)blocksize) / unitbytes;
                    break;

                case ChdCompressionType.Parent:
                    offset = bitbuf.Read(parentbits);
                    last_parent = offset;
                    break;

                case ChdCompressionType.Parent1:
                    last_parent += blocksize / unitbytes;
                    goto case ChdCompressionType.Parent0;
                case ChdCompressionType.Parent0:
                    map[blockIndex].CompressionType = ChdCompressionType.Parent;
                    offset = last_parent;
                    break;
            }

            map[blockIndex].CompressedDataLength = length;
            map[blockIndex].CompressedDataOffsetOrSelfBlockIndex = offset;
            map[blockIndex].Crc16 = crc16;
        }


        // verify the final CRC
        byte[] rawmap = new byte[totalBlocks * 12];
        for (int blockIndex = 0; blockIndex < totalBlocks; blockIndex++)
        {
            if (map[blockIndex].Crc16 is null)
            {
                throw new InvalidDataException();
            }

            int rawmapIndex = blockIndex * 12;
            rawmap[rawmapIndex] = (byte)map[blockIndex].CompressionType;
            rawmap.PutUInt24BE(rawmapIndex + 1, map[blockIndex].CompressedDataLength);
            rawmap.PutUInt48BE(rawmapIndex + 4, map[blockIndex].CompressedDataOffsetOrSelfBlockIndex);
            rawmap.PutUInt16BE(rawmapIndex + 10, (uint)map[blockIndex].Crc16!);
        }

        if (CRC16.calc(rawmap, (int)totalBlocks * 12) != mapcrc)
        {
            throw new InvalidDataException("Checksum mismatch.");
        }

        return map;
    }

    private static ChdCodecType CompTypeConv(uint ct)
    {
        switch (ct)
        {
            case 1: return ChdCodecType.Zlib;
            case 2: return ChdCodecType.Zlib;
            case 3: return ChdCodecType.Avhuff;
            default:
                return ChdCodecType.Error;
        }
    }

    // Converts V3 & V4 mapFlags to V5 compression_type
    private static ChdCompressionType ConvMapFlagstoCompressionType(ChdMapEntryFlags mapFlags)
    {
        switch (mapFlags & ChdMapEntryFlags.TypeMask)
        {
            case ChdMapEntryFlags.Invalid: return ChdCompressionType.Error;
            case ChdMapEntryFlags.Compressed: return ChdCompressionType.Type0;
            case ChdMapEntryFlags.Uncompressed: return ChdCompressionType.None;
            case ChdMapEntryFlags.Mini: return ChdCompressionType.Mini;
            case ChdMapEntryFlags.SelfHunk: return ChdCompressionType.Self;
            case ChdMapEntryFlags.ParentHunk: return ChdCompressionType.Parent;
            default:
                return ChdCompressionType.Error;
        }
    }
}
