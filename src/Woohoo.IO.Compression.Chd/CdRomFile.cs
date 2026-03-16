// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.Chd;

using System;
using System.Diagnostics;
using System.IO;
using Woohoo.IO.Compression.Chd.Internal;

public sealed class CdRomFile
{
    /// <summary>
    /// CD file.
    /// </summary>
    private readonly ChdFile chd;

    public CdRomFile(ChdFile chd)
    {
        ArgumentNullException.ThrowIfNull(chd);

        this.chd = chd;
        this.Toc = CdRomTocReader.ReadToc(chd);
    }

    /// <summary>
    /// CD table of contents.
    /// </summary>
    public CdRomToc Toc { get; }

    public uint LastSession => this.Toc.NumSessions != 0 ? this.Toc.NumSessions : 1;

    public int LastTrack => (int)this.Toc.NumTracks;

    /// <summary>
    /// Find the CHD LBA and the track number.
    /// </summary>
    public uint PhysicalToChdLba(uint physicalLba, out uint trackNum)
    {
        trackNum = 0;

        // loop until our current LBA is less than the start LBA of the next track
        for (uint track = 0; track < this.Toc.NumTracks; track++)
        {
            if (physicalLba < this.Toc.Tracks[track + 1].PhysFrameOffset)
            {
                uint chdLba = physicalLba - this.Toc.Tracks[track].PhysFrameOffset + this.Toc.Tracks[track].ChdFrameOffset;
                trackNum = track;
                return chdLba;
            }
        }

        return physicalLba;
    }

    /// <summary>
    /// Find the CHD LBA and the track number.
    /// </summary>
    public uint LogicalToChdLba(uint logicalLba, out uint trackNum)
    {
        trackNum = 0;

        // loop until our current LBA is less than the start LBA of the next track
        for (uint track = 0; track < this.Toc.NumTracks; track++)
        {
            if (logicalLba < this.Toc.Tracks[track + 1].LogFrameOffset)
            {
                // convert to physical and proceed
                uint physLba = this.Toc.Tracks[track].PhysFrameOffset + (logicalLba - this.Toc.Tracks[track].LogFrameOffset);
                uint chdLba = physLba - this.Toc.Tracks[track].PhysFrameOffset + this.Toc.Tracks[track].ChdFrameOffset;
                trackNum = track;
                return chdLba;
            }
        }

        return logicalLba;
    }

    public Stream OpenStream(int track)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(track);

        return new CdRomTrackStream(
            this,
            this.Toc.Tracks[track].TrackType,
            track);
    }

    public string[] ExtractTracksTo(string outputFolder, string? trackFileNameFormat = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputFolder);

        Directory.CreateDirectory(outputFolder);
        var filePaths = new List<string>();

        trackFileNameFormat ??= this.Toc.NumTracks < 10
            ? "Track{0}.bin"
            : "Track{0:D2}.bin";

        ArgumentException.ThrowIfNullOrWhiteSpace(trackFileNameFormat);

        for (int i = 0; i < this.Toc.NumTracks; i++)
        {
            var fileName = string.Format(trackFileNameFormat, i + 1);
            var filePath = Path.Combine(outputFolder, fileName);
            filePaths.Add(filePath);

            using var outputStream = new FileStream(filePath, FileMode.Create);
            using var inputStream = this.OpenStream(i);
            inputStream.CopyTo(outputStream);
        }

        return [.. filePaths];
    }

    public void ReadData(uint lbaSector, byte[] buffer, CdRomTrackType dataType, bool isPhysical)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        uint chdSector = isPhysical
            ? this.PhysicalToChdLba(lbaSector, out var trackNum)
            : this.LogicalToChdLba(lbaSector, out trackNum);

        var trackType = this.Toc.Tracks[trackNum].TrackType;
        if (trackType == dataType || dataType == CdRomTrackType.RawDontCare)
        {
            Debug.Assert(this.Toc.Tracks[trackNum].DataSize != 0, "Track data size is zero");
            this.ReadPartialSector(buffer, lbaSector, chdSector, trackNum, 0, this.Toc.Tracks[trackNum].DataSize, isPhysical);
        }
        else
        {
            if (dataType == CdRomTrackType.Mode1 && trackType == CdRomTrackType.Mode1Raw)
            {
                // return 2048 bytes of mode 1 data from a 2352 byte mode 1 raw sector
                this.ReadPartialSector(buffer, lbaSector, chdSector, trackNum, 16, 2048, isPhysical);
            }

            if (dataType == CdRomTrackType.Mode1Raw && trackType == CdRomTrackType.Mode1)
            {
                throw new NotImplementedException("Conversion of 2048 byte mode 1 data to 2352 byte mode 1 raw is not implemented");
            }

            if (dataType == CdRomTrackType.Mode1 && (trackType == CdRomTrackType.Mode2Form1 || trackType == CdRomTrackType.Mode2Raw))
            {
                // return 2048 bytes of mode 1 data from a mode2 form1 or raw sector
                this.ReadPartialSector(buffer, lbaSector, chdSector, trackNum, 24, 2048, isPhysical);
            }

            if (dataType == CdRomTrackType.Mode1 && trackType == CdRomTrackType.Mode2FormMix)
            {
                // return 2048 bytes of mode 1 data from a mode2 form2 or XA sector
                this.ReadPartialSector(buffer, lbaSector, chdSector, trackNum, 8, 2048, isPhysical);
            }

            if (dataType == CdRomTrackType.Mode2 && (trackType == CdRomTrackType.Mode1Raw || trackType == CdRomTrackType.Mode2Raw))
            {
                // return mode 2 2336 byte data from a 2352 byte mode 1 or 2 raw sector (skip the header)
                this.ReadPartialSector(buffer, lbaSector, chdSector, trackNum, 16, 2336, isPhysical);
            }

            throw new NotSupportedException($"Conversion from track type {trackType} to {dataType} is not supported");
        }
    }

    public void ReadPartialSector(byte[] dest, uint lbaSector, uint chdSector, uint trackNum, uint startOffs, uint length, bool isPhys)
    {
        ArgumentNullException.ThrowIfNull(dest);
        ArgumentOutOfRangeException.ThrowIfGreaterThan((int)length, dest.Length);

        bool needsSwap = false;
        if (!isPhys)
        {
            if (this.Toc.Tracks[trackNum].PregapDataSize == 0 && lbaSector < this.Toc.Tracks[trackNum].LogFrameOffset)
            {
                Trace.WriteLine($"PG missing sector: LBA {lbaSector}, trklog {this.Toc.Tracks[trackNum].LogFrameOffset}, trphys {this.Toc.Tracks[trackNum].PhysFrameOffset}");
                Array.Fill<byte>(dest, 0, 0, (int)length);
                return;
            }
        }

        if (!isPhys && this.Toc.Tracks[trackNum].PregapDataSize != 0)
        {
            // chdman (phys=true) relies on chdframeofs to point to index 0 instead of index 1 for extractcd.
            // Actually playing CDs requires it to point to index 1 instead of index 0, so adjust the offset when phys=false.
            chdSector += this.Toc.Tracks[trackNum].Pregap;
        }

        this.chd.ReadBytes((chdSector * CdRomConstants.FrameSize) + startOffs, dest, length);

        if (this.Toc.Flags.HasFlag(CdRomTocFlag.GdRomLE) && this.Toc.Tracks[trackNum].TrackType == CdRomTrackType.Audio)
        {
            // swap CDDA in the case of LE GDROMs
            needsSwap = true;
        }

        if (needsSwap)
        {
            throw new NotImplementedException("Swapping CDDA data for LE GDROMs is not implemented.");
        }
    }

    public bool ReadSubCode(uint lbaSector, byte[] buffer, bool isPhysical)
    {
        uint chdSector = isPhysical
            ? this.PhysicalToChdLba(lbaSector, out var trackNum)
            : this.LogicalToChdLba(lbaSector, out trackNum);

        if (this.Toc.Tracks[trackNum].SubSize == 0)
        {
            return false;
        }

        this.ReadPartialSector(buffer, lbaSector, chdSector, trackNum, this.Toc.Tracks[trackNum].DataSize, this.Toc.Tracks[trackNum].SubSize, isPhysical);
        return true;
    }

    public uint GetTrack(uint frame)
    {
        this.LogicalToChdLba(frame, out uint trackNum);
        return trackNum;
    }

    public uint GetTrackStartLog(int track)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(track);

        return this.Toc.Tracks[track == 0xaa ? this.Toc.NumTracks : track].LogFrameOffset;
    }

    public uint GetTrackStartPhys(int track)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(track);

        return this.Toc.Tracks[track == 0xaa ? this.Toc.NumTracks : track].PhysFrameOffset;
    }
}
