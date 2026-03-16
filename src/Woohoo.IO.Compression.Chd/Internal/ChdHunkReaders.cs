namespace Woohoo.IO.Compression.Chd.Internal;

using System;
using System.IO;
using System.IO.Compression;
using CUETools.Codecs;
using CUETools.Codecs.Flake;
using Woohoo.IO.Compression.Chd.Internal.Utils;

internal delegate void ChdHunkReader(byte[] buffIn, int buffInLength, byte[] buffOut, int buffOutLength, ChdCodec codec);

internal static partial class ChdHunkReaders
{
    private const int CdMaxSectorData = 2352;
    private const int CdMaxSubCodeData = 96;

    private static readonly int CdFrameSize = CdMaxSectorData + CdMaxSubCodeData;
    private static readonly byte[] CdSyncHeader = [0x00, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x00];

    internal static void Zlib(byte[] buffIn, int buffInLength, byte[] buffOut, int buffOutLength, ChdCodec codec)
    {
        Zlib(buffIn, 0, buffInLength, buffOut, buffOutLength);
    }

    private static void Zlib(byte[] buffIn, int buffInStart, int buffInLength, byte[] buffOut, int buffOutLength)
    {
        using var memStream = new MemoryStream(buffIn, buffInStart, buffInLength, false);
        using var compStream = new DeflateStream(memStream, CompressionMode.Decompress, true);

        int bytesRead = 0;
        while (bytesRead < buffOutLength)
        {
            int bytes = compStream.Read(buffOut, bytesRead, buffOutLength - bytesRead);
            if (bytes == 0)
            {
                throw new InvalidDataException();
            }

            bytesRead += bytes;
        }
    }

    internal static void Zstd(byte[] buffIn, int buffInLength, byte[] buffOut, int buffOutLength, ChdCodec codec)
    {
        Zstd(buffIn, 0, buffInLength, buffOut, buffOutLength);
    }

    private static void Zstd(byte[] buffIn, int buffInStart, int buffInLength, byte[] buffOut, int buffOutLength)
    {
        using var memStream = new MemoryStream(buffIn, buffInStart, buffInLength, false);
        using var compStream = new ZstdStream(memStream);

        int bytesRead = 0;
        while (bytesRead < buffOutLength)
        {
            int bytes = compStream.Read(buffOut, bytesRead, buffOutLength - bytesRead);
            if (bytes == 0)
            {
                throw new InvalidDataException();
            }

            bytesRead += bytes;
        }
    }

    internal static void Lzma(byte[] buffIn, int buffInLength, byte[] buffOut, int buffOutLength, ChdCodec codec)
    {
        Lzma(buffIn, 0, buffInLength, buffOut, buffOutLength, codec);
    }

    private static void Lzma(byte[] buffIn, int buffInStart, int compsize, byte[] buffOut, int buffOutLength, ChdCodec codec)
    {
        // hacky header creator
        byte[] properties = new byte[5];
        int posStateBits = 2;
        int numLiteralPosStateBits = 0;
        int numLiteralContextBits = 3;
        int dictionarySize = buffOutLength;
        properties[0] = (byte)((((posStateBits * 5) + numLiteralPosStateBits) * 9) + numLiteralContextBits);
        for (int j = 0; j < 4; j++)
        {
            properties[1 + j] = (byte)((dictionarySize >> (8 * j)) & 0xFF);
        }

        if (codec.blzma == null)
        {
            codec.blzma = new byte[dictionarySize];
        }

        using var memStream = new MemoryStream(buffIn, buffInStart, compsize, false);
        using var compStream = new LzmaStream(properties, memStream, -1, -1, null, false, codec.blzma);

        int bytesRead = 0;
        while (bytesRead < buffOutLength)
        {
            int bytes = compStream.Read(buffOut, bytesRead, buffOutLength - bytesRead);
            if (bytes == 0)
            {
                throw new InvalidDataException();
            }

            bytesRead += bytes;
        }
    }

    internal static void Huffman(byte[] buffIn, int buffInLength, byte[] buffOut, int buffOutLength, ChdCodec codec)
    {
        if (codec.bHuffman == null)
        {
            codec.bHuffman = new ushort[1 << 16];
        }

        BitStream bitbuf = new BitStream(buffIn, 0, buffInLength);
        HuffmanDecoder hd = new HuffmanDecoder(256, 16, bitbuf, codec.bHuffman);

        if (hd.ImportTreeHuffman() != huffman_error.HUFFERR_NONE)
        {
            throw new InvalidDataException();
        }

        for (int j = 0; j < buffOutLength; j++)
        {
            buffOut[j] = (byte)hd.DecodeOne();
        }
    }

    internal static void Flac(byte[] buffIn, int buffInLength, byte[] buffOut, int buffOutLength, ChdCodec codec)
    {
        byte endianType = buffIn[0];
        // CHD adds a leading char to indicate endian. Not part of the flac format.
        bool swapEndian = endianType == 'B'; //'L'ittle / 'B'ig
        Flac(buffIn, 1, buffInLength, buffOut, buffOutLength, swapEndian, codec, out _);
    }

    private static void Flac(byte[] buffIn, int buffInStart, int buffInLength, byte[] buffOut, int buffOutLength, bool swapEndian, ChdCodec codec, out int srcPos)
    {
        codec.FLAC_settings ??= new AudioPCMConfig(16, 2, 44100);
        codec.FLAC_audioDecoder ??= new AudioDecoder(codec.FLAC_settings);
        codec.FLAC_audioBuffer ??= new AudioBuffer(codec.FLAC_settings, buffOutLength); //audio buffer to take decoded samples and read them to bytes.

        srcPos = buffInStart;
        int dstPos = 0;

        // this may require some error handling. Hopefully the while condition is reliable
        while (dstPos < buffOutLength)
        {
            int read = codec.FLAC_audioDecoder.DecodeFrame(buffIn, srcPos, buffInLength - srcPos);
            codec.FLAC_audioDecoder.Read(codec.FLAC_audioBuffer, (int)codec.FLAC_audioDecoder.Remaining);
            Array.Copy(codec.FLAC_audioBuffer.Bytes, 0, buffOut, dstPos, codec.FLAC_audioBuffer.ByteLength);
            dstPos += codec.FLAC_audioBuffer.ByteLength;
            srcPos += read;
        }

        // Nanook - hack to support 16bit byte flipping - tested passes hunk CRC test
        if (swapEndian)
        {
            byte tmp;
            for (int i = 0; i < buffOutLength; i += 2)
            {
                tmp = buffOut[i];
                buffOut[i] = buffOut[i + 1];
                buffOut[i + 1] = tmp;
            }
        }
    }

    internal static void CdZlib(byte[] buffIn, int buffInLength, byte[] buffOut, int buffOutLength, ChdCodec codec)
    {
        // determine header bytes
        int frames = buffOutLength / CdFrameSize;
        int complen_bytes = (buffOutLength < 65536) ? 2 : 3;
        int ecc_bytes = (frames + 7) / 8;
        int header_bytes = ecc_bytes + complen_bytes;

        // extract compressed length of base
        int complen_base = (buffIn[ecc_bytes + 0] << 8) | buffIn[ecc_bytes + 1];
        if (complen_bytes > 2)
        {
            complen_base = (complen_base << 8) | buffIn[ecc_bytes + 2];
        }

        codec.bSector ??= new byte[frames * CdMaxSectorData];
        codec.bSubcode ??= new byte[frames * CdMaxSubCodeData];

        Zlib(buffIn, (int)header_bytes, complen_base, codec.bSector, frames * CdMaxSectorData);
        Zlib(buffIn, header_bytes + complen_base, buffInLength - header_bytes - complen_base, codec.bSubcode, frames * CdMaxSubCodeData);

        // reassemble the data
        for (int framenum = 0; framenum < frames; framenum++)
        {
            Array.Copy(codec.bSector, framenum * CdMaxSectorData, buffOut, framenum * CdFrameSize, CdMaxSectorData);
            Array.Copy(codec.bSubcode, framenum * CdMaxSubCodeData, buffOut, (framenum * CdFrameSize) + CdMaxSectorData, CdMaxSubCodeData);

            // reconstitute the ECC data and sync header 
            int sectorStart = framenum * CdFrameSize;
            if ((buffIn[framenum / 8] & (1 << (framenum % 8))) != 0)
            {
                Array.Copy(CdSyncHeader, 0, buffOut, sectorStart, CdSyncHeader.Length);
                CdRom.ecc_generate(buffOut, sectorStart);
            }
        }
    }

    internal static void CdZstd(byte[] buffIn, int buffInLength, byte[] buffOut, int buffOutLength, ChdCodec codec)
    {
        // determine header bytes
        int frames = buffOutLength / CdFrameSize;
        int complen_bytes = (buffOutLength < 65536) ? 2 : 3;
        int ecc_bytes = (frames + 7) / 8;
        int header_bytes = ecc_bytes + complen_bytes;

        // extract compressed length of base
        int complen_base = (buffIn[ecc_bytes + 0] << 8) | buffIn[ecc_bytes + 1];
        if (complen_bytes > 2)
        {
            complen_base = (complen_base << 8) | buffIn[ecc_bytes + 2];
        }

        codec.bSector ??= new byte[frames * CdMaxSectorData];
        codec.bSubcode ??= new byte[frames * CdMaxSubCodeData];

        Zstd(buffIn, (int)header_bytes, complen_base, codec.bSector, frames * CdMaxSectorData);
        Zstd(buffIn, header_bytes + complen_base, buffInLength - header_bytes - complen_base, codec.bSubcode, frames * CdMaxSubCodeData);

        // reassemble the data
        for (int framenum = 0; framenum < frames; framenum++)
        {
            Array.Copy(codec.bSector, framenum * CdMaxSectorData, buffOut, framenum * CdFrameSize, CdMaxSectorData);
            Array.Copy(codec.bSubcode, framenum * CdMaxSubCodeData, buffOut, (framenum * CdFrameSize) + CdMaxSectorData, CdMaxSubCodeData);

            // reconstitute the ECC data and sync header 
            int sectorStart = framenum * CdFrameSize;
            if ((buffIn[framenum / 8] & (1 << (framenum % 8))) != 0)
            {
                Array.Copy(CdSyncHeader, 0, buffOut, sectorStart, CdSyncHeader.Length);
                CdRom.ecc_generate(buffOut, sectorStart);
            }
        }
    }

    internal static void CdLzma(byte[] buffIn, int buffInLength, byte[] buffOut, int buffOutLength, ChdCodec codec)
    {
        // determine header bytes
        int frames = buffOutLength / CdFrameSize;
        int complen_bytes = (buffOutLength < 65536) ? 2 : 3;
        int ecc_bytes = (frames + 7) / 8;
        int header_bytes = ecc_bytes + complen_bytes;

        // extract compressed length of base
        int complen_base = (buffIn[ecc_bytes + 0] << 8) | buffIn[ecc_bytes + 1];
        if (complen_bytes > 2)
        {
            complen_base = (complen_base << 8) | buffIn[ecc_bytes + 2];
        }

        codec.bSector ??= new byte[frames * CdMaxSectorData];
        codec.bSubcode ??= new byte[frames * CdMaxSubCodeData];

        Lzma(buffIn, header_bytes, complen_base, codec.bSector, frames * CdMaxSectorData, codec);
        Zlib(buffIn, header_bytes + complen_base, buffInLength - header_bytes - complen_base, codec.bSubcode, frames * CdMaxSubCodeData);

        // reassemble the data
        for (int framenum = 0; framenum < frames; framenum++)
        {
            Array.Copy(codec.bSector, framenum * CdMaxSectorData, buffOut, framenum * CdFrameSize, CdMaxSectorData);
            Array.Copy(codec.bSubcode, framenum * CdMaxSubCodeData, buffOut, (framenum * CdFrameSize) + CdMaxSectorData, CdMaxSubCodeData);

            // reconstitute the ECC data and sync header 
            int sectorStart = framenum * CdFrameSize;
            if ((buffIn[framenum / 8] & (1 << (framenum % 8))) != 0)
            {
                Array.Copy(CdSyncHeader, 0, buffOut, sectorStart, CdSyncHeader.Length);
                CdRom.ecc_generate(buffOut, sectorStart);
            }
        }
    }

    internal static void CdFlac(byte[] buffIn, int buffInLength, byte[] buffOut, int buffOutLength, ChdCodec codec)
    {
        int frames = buffOutLength / CdFrameSize;

        codec.bSector ??= new byte[frames * CdMaxSectorData];
        codec.bSubcode ??= new byte[frames * CdMaxSubCodeData];

        Flac(buffIn, 0, buffInLength, codec.bSector, frames * CdMaxSectorData, true, codec, out int pos);
        Zlib(buffIn, pos, buffInLength - pos, codec.bSubcode, frames * CdMaxSubCodeData);

        // reassemble the data
        for (int framenum = 0; framenum < frames; framenum++)
        {
            Array.Copy(codec.bSector, framenum * CdMaxSectorData, buffOut, framenum * CdFrameSize, CdMaxSectorData);
            Array.Copy(codec.bSubcode, framenum * CdMaxSubCodeData, buffOut, (framenum * CdFrameSize) + CdMaxSectorData, CdMaxSubCodeData);
        }
    }
}
