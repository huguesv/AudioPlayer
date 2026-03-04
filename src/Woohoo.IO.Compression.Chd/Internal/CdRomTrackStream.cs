// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.Chd.Internal;

using System;

internal class CdRomTrackStream : Stream
{
    private readonly CdRomFile cdRomFile;
    private readonly CdRomTrackType dataType;
    private readonly CdRomTrackInfo trackInfo;
    private readonly int trackIndex;
    private readonly byte[] sectorBuffer;
    private long position;

    public CdRomTrackStream(CdRomFile cdRomFile, CdRomTrackType dataType, int trackIndex)
    {
        ArgumentNullException.ThrowIfNull(cdRomFile);
        ArgumentOutOfRangeException.ThrowIfNegative(trackIndex);

        this.cdRomFile = cdRomFile;
        this.dataType = dataType;
        this.trackInfo = this.cdRomFile.Toc.Tracks[trackIndex];
        this.trackIndex = trackIndex;
        this.sectorBuffer = new byte[this.trackInfo.DataSize];
    }

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length => this.trackInfo.DataSize * this.trackInfo.Frames;

    public override long Position
    {
        get => this.position;
        set => this.position = value;
    }

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var phyStart = this.cdRomFile.GetTrackStartPhys(this.trackIndex);

        var sectorNum = this.position / this.trackInfo.DataSize;
        var sectorOffset = this.position % this.trackInfo.DataSize;

        int remaining = count;
        while (remaining > 0)
        {
            if (sectorNum >= this.trackInfo.Frames)
            {
                // Reached the end
                break;
            }

            this.cdRomFile.ReadData((uint)(phyStart + sectorNum), this.sectorBuffer, this.dataType, isPhysical: true);

            if (this.trackInfo.TrackType == CdRomTrackType.Audio)
            {
                // Audio track needs to be byte swapped
                for (int i = 0; i < this.sectorBuffer.Length; i += 2)
                {
                    (this.sectorBuffer[i + 1], this.sectorBuffer[i]) = (this.sectorBuffer[i], this.sectorBuffer[i + 1]);
                }
            }

            int copyCount = Math.Min(remaining, (int)(this.sectorBuffer.Length - sectorOffset));
            Array.Copy(this.sectorBuffer, sectorOffset, buffer, offset, copyCount);

            offset += copyCount;
            remaining -= copyCount;
            sectorNum++;
            sectorOffset = 0;
        }

        int readCount = count - remaining;
        this.position += readCount;
        return readCount;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                this.position = offset;
                break;
            case SeekOrigin.Current:
                this.position += offset;
                break;
            case SeekOrigin.End:
                this.position = this.Length + offset;
                break;
            default:
                break;
        }

        return this.position;
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }
}
