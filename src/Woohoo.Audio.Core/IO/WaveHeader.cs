// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.IO;

using System.Text;

public static class WaveHeader
{
    public static byte[] Create(int dataSize)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(dataSize + 36); // File size minus 8 bytes
        writer.Write(Encoding.ASCII.GetBytes("WAVE"));
        writer.Write(Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16); // Subchunk1Size for PCM
        writer.Write((short)1); // AudioFormat (1 for PCM)
        writer.Write((short)2); // NumChannels
        writer.Write(44100); // SampleRate
        writer.Write(44100 * 2 * 16 / 8); // ByteRate
        writer.Write((short)(2 * 16 / 8)); // BlockAlign
        writer.Write((short)16); // BitsPerSample
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(dataSize); // Subchunk2Size
        return ms.ToArray();
    }
}
