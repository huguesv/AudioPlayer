// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.Chd.Internal;

using System;
using System.Text.RegularExpressions;

internal partial class CdRomTocReader
{
    private const string CdRomMetadata1Format = @"^\s*TRACK\s*:\s*(\d+)\s+TYPE\s*:\s*([^\s]+)\s+SUBTYPE\s*:\s*([^\s]+)\s+FRAMES\s*:\s*(\d+)\s*$";
    private const string CdRomMetadata2Format = @"^\s*TRACK\s*:\s*(\d+)\s+TYPE\s*:\s*([^\s]+)\s+SUBTYPE\s*:\s*([^\s]+)\s+FRAMES\s*:\s*(\d+)\s+PREGAP\s*:\s*(\d+)\s+PGTYPE\s*:\s*([^\s]+)\s+PGSUB\s*:\s*([^\s]+)\s+POSTGAP\s*:\s*(\d+)\s*$";
    private const string GdRomMetadataFormat = @"^\s*TRACK\s*:\s*(\d+)\s+TYPE\s*:\s*([^\s]+)\s+SUBTYPE\s*:\s*([^\s]+)\s+FRAMES\s*:\s*(\d+)\s+PAD\s*:\s*(\d+)\s+PREGAP\s*:\s*(\d+)\s+PGTYPE\s*:\s*([^\s]+)\s+PGSUB\s*:\s*([^\s]+)\s+POSTGAP\s*:\s*(\d+)\s*$";

    private static readonly Regex CdRomMetadata1Regex = CdRomMetadata1CompiledRegex();
    private static readonly Regex CdRomMetadata2Regex = CdRomMetadata2CompiledRegex();
    private static readonly Regex GdRomMetadataRegex = GdRomMetadataCompiledRegex();

    [GeneratedRegex(CdRomMetadata1Format, RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex CdRomMetadata1CompiledRegex();

    [GeneratedRegex(CdRomMetadata2Format, RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex CdRomMetadata2CompiledRegex();

    [GeneratedRegex(GdRomMetadataFormat, RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex GdRomMetadataCompiledRegex();

    public static CdRomToc ReadToc(ChdFile chd)
    {
        ArgumentNullException.ThrowIfNull(chd);

        var toc = ParseMetadata(chd);
        ComputeFrames(toc);
        return toc;
    }

    private static CdRomToc ParseMetadata(ChdFile chd)
    {
        var toc = new CdRomToc();
        toc.NumSessions = 1;

        for (uint i = 0; i < CdRomConstants.MaxTracks; i++)
        {
            toc.NumTracks = i;

            int trackNum = -1;
            int frames = 0;
            int pregap = 0;
            int postgap = 0;
            int padFrames = 0;
            string type = string.Empty;
            string subtype = string.Empty;
            string pgtype = string.Empty;
            string pgsub = string.Empty;

            if (chd.TryGetMetadata(ChdFile.CdRomTrackMetadataTag, i, out string? metadata1))
            {
                ParseCdRomMetadata1(metadata1, ref trackNum, ref frames, ref type, ref subtype);
            }
            else if (chd.TryGetMetadata(ChdFile.CdRomTrackMetadata2Tag, i, out string? metadata2))
            {
                ParseCdRomMetadata2(metadata2, ref trackNum, ref frames, ref pregap, ref postgap, ref type, ref subtype, ref pgtype, ref pgsub);
            }
            else
            {
                string? metadataGd;
                if (chd.TryGetMetadata(ChdFile.GdRomTrackMetadataOldTag, i, out metadataGd))
                {
                    toc.Flags |= CdRomTocFlag.GdRomLE;
                }
                else if (chd.TryGetMetadata(ChdFile.GdRomTrackMetadataTag, i, out metadataGd))
                {
                }
                else
                {
                    // No metadata, so this is the end of the tracks
                    break;
                }

                ParseGdRomMetadata(metadataGd, ref trackNum, ref frames, ref padFrames, ref pregap, ref postgap, ref type, ref subtype, ref pgtype, ref pgsub);

                toc.Flags |= CdRomTocFlag.GdRom;
            }

            if (trackNum == 0 || trackNum > CdRomConstants.MaxTracks)
            {
                throw new IOException($"Invalid track number {trackNum}.");
            }

            var track = toc.Tracks[trackNum - 1];

            track.TrackType = MapTrackType(type);
            track.DataSize = DataSizeFor(track.TrackType);
            track.SubType = MapSubType(subtype);
            track.SubSize = SubSizeFor(track.SubType);
            track.Frames = (uint)frames;
            track.PadFrames = (uint)padFrames;
            uint padded = (uint)(frames + CdRomConstants.TrackPadding - 1) / CdRomConstants.TrackPadding;
            track.ExtraFrames = (padded * CdRomConstants.TrackPadding) - (uint)frames;

            track.Pregap = (uint)pregap;
            track.PregapType = CdRomTrackType.Mode1;
            track.PregapSub = CdRomSubType.None;
            track.PregapDataSize = 0;
            track.PregapSubSize = 0;

            if (track.Pregap > 0)
            {
                if (pgtype.Length > 0 && pgtype[0] == 'V')
                {
                    track.PregapType = MapTrackType(pgtype[1..]);
                    track.PregapDataSize = DataSizeFor(track.PregapType);
                }

                track.PregapSub = MapSubType(pgsub);
                track.PregapSubSize = SubSizeFor(track.PregapSub);
            }

            track.Postgap = (uint)postgap;
        }

        if (toc.NumTracks == 0)
        {
            // We could (may have to?) support this in the future
            throw new NotImplementedException("Old-style and GD-ROM metadata parsing not implemented.");
        }

        return toc;

        static CdRomTrackType MapTrackType(string val)
        {
            switch (val)
            {
                case "MODE1": return CdRomTrackType.Mode1;
                case "MODE1/2048": return CdRomTrackType.Mode1;
                case "MODE1_RAW": return CdRomTrackType.Mode1Raw;
                case "MODE1/2352": return CdRomTrackType.Mode1Raw;
                case "MODE2": return CdRomTrackType.Mode2;
                case "MODE2/2336": return CdRomTrackType.Mode2;
                case "MODE2_FORM1": return CdRomTrackType.Mode2Form1;
                case "MODE2/2048": return CdRomTrackType.Mode2Form1;
                case "MODE2_FORM2": return CdRomTrackType.Mode2Form2;
                case "MODE2/2324": return CdRomTrackType.Mode2Form2;
                case "MODE2_FORM_MIX": return CdRomTrackType.Mode2FormMix;
                case "MODE2_RAW": return CdRomTrackType.Mode2Raw;
                case "MODE2/2352": return CdRomTrackType.Mode2Raw;
                case "CDI/2352": return CdRomTrackType.Mode2Raw;
                case "AUDIO": return CdRomTrackType.Audio;
                default: throw new ArgumentException($"Invalid TYPE metadata '{val}'.");
            }
        }

        static CdRomSubType MapSubType(string val)
        {
            switch (val)
            {
                case "RW": return CdRomSubType.Normal;
                case "RW_RAW": return CdRomSubType.Raw;
                case "NONE": return CdRomSubType.None;
                default: throw new ArgumentException($"Invalid SUBTYPE metadata '{val}'.");
            }
        }

        static uint DataSizeFor(CdRomTrackType trackType)
        {
            return trackType switch
            {
                CdRomTrackType.Mode1 => 2048,
                CdRomTrackType.Mode1Raw => 2352,
                CdRomTrackType.Mode2 => 2336,
                CdRomTrackType.Mode2Form1 => 2048,
                CdRomTrackType.Mode2Form2 => 2324,
                CdRomTrackType.Mode2FormMix => 2336,
                CdRomTrackType.Mode2Raw => 2352,
                CdRomTrackType.Audio => 2352,
                _ => throw new ArgumentException("Invalid track type."),
            };
        }

        static uint SubSizeFor(CdRomSubType st)
        {
            return st switch
            {
                CdRomSubType.Normal => 96,
                CdRomSubType.Raw => 96,
                CdRomSubType.None => 0,
                _ => throw new ArgumentException("Invalid sub type."),
            };
        }
    }

    private static void ParseCdRomMetadata1(string metadata, ref int trackNum, ref int frames, ref string type, ref string subtype)
    {
        var m = CdRomMetadata1Regex.Match(metadata);
        if (m.Success)
        {
            // groups: 1=track,2=type,3=subtype,4=frames
            if (!int.TryParse(m.Groups[1].Value, out trackNum))
            {
                throw new IOException($"Invalid value for TRACK in metadata '{metadata}'.");
            }

            type = m.Groups[2].Value;
            subtype = m.Groups[3].Value;

            if (!int.TryParse(m.Groups[4].Value, out frames))
            {
                throw new IOException($"Invalid value for FRAMES in metadata '{metadata}'.");
            }
        }
    }

    private static void ParseCdRomMetadata2(string metadata, ref int trackNum, ref int frames, ref int pregap, ref int postgap, ref string type, ref string subtype, ref string pgtype, ref string pgsub)
    {
        var m = CdRomMetadata2Regex.Match(metadata);
        if (m.Success)
        {
            // groups: 1=track,2=type,3=subtype,4=frames,5=pregap,6=pgtype,7=pgsub,8=postgap
            if (!int.TryParse(m.Groups[1].Value, out trackNum))
            {
                throw new IOException($"Invalid value for TRACK in metadata '{metadata}'.");
            }

            type = m.Groups[2].Value;
            subtype = m.Groups[3].Value;

            if (!int.TryParse(m.Groups[4].Value, out frames))
            {
                throw new IOException($"Invalid value for FRAMES in metadata '{metadata}'.");
            }

            if (!int.TryParse(m.Groups[5].Value, out pregap))
            {
                throw new IOException($"Invalid value for PREGAP in metadata '{metadata}'.");
            }

            pgtype = m.Groups[6].Value;
            pgsub = m.Groups[7].Value;

            if (!int.TryParse(m.Groups[8].Value, out postgap))
            {
                throw new IOException($"Invalid value for POSTGAP in metadata '{metadata}'.");
            }
        }
    }

    private static void ParseGdRomMetadata(string metadata, ref int trackNum, ref int frames, ref int pad, ref int pregap, ref int postgap, ref string type, ref string subtype, ref string pgtype, ref string pgsub)
    {
        var m = GdRomMetadataRegex.Match(metadata);
        if (m.Success)
        {
            // groups: 1=track,2=type,3=subtype,4=frames,5=pad,6=pregap,7=pgtype,8=pgsub,9=postgap
            if (!int.TryParse(m.Groups[1].Value, out trackNum))
            {
                throw new IOException($"Invalid value for TRACK in metadata '{metadata}'.");
            }

            type = m.Groups[2].Value;
            subtype = m.Groups[3].Value;

            if (!int.TryParse(m.Groups[4].Value, out frames))
            {
                throw new IOException($"Invalid value for FRAMES in metadata '{metadata}'.");
            }

            if (!int.TryParse(m.Groups[5].Value, out pad))
            {
                throw new IOException($"Invalid value for PAD in metadata '{metadata}'.");
            }

            if (!int.TryParse(m.Groups[6].Value, out pregap))
            {
                throw new IOException($"Invalid value for PREGAP in metadata '{metadata}'.");
            }

            pgtype = m.Groups[7].Value;
            pgsub = m.Groups[8].Value;

            if (!int.TryParse(m.Groups[9].Value, out postgap))
            {
                throw new IOException($"Invalid value for POSTGAP in metadata '{metadata}'.");
            }
        }
    }

    private static void ComputeFrames(CdRomToc toc)
    {
        // calculate the starting frame for each track, keeping in mind that CHDMAN
        // pads tracks out with extra frames to fit 4 - frame size boundries
        uint physOffset = 0;
        uint chdOffset = 0;
        uint logOffset = 0;

        for (uint i = 0; i < toc.NumTracks; i++)
        {
            var track = toc.Tracks[i];
            track.LogFrameOffset = 0;

            if (track.PregapDataSize == 0)
            {
                // Anything that isn't cue.
                // toc (cdrdao): Pregap data seems to be included at the end of previous track.
                // START/PREGAP is only issued in special cases, for instance alongside ZERO commands.
                // ZERO and SILENCE commands are supposed to generate additional data that's not included
                // in the image directly, so the total logofs value must be offset to point to index 1.
                logOffset += track.Pregap;
            }
            else
            {
                // cues: Pregap is the difference between index 0 and index 1 unless PREGAP is specified.
                // The data is assumed to be in the bin and not generated separately, so the pregap should
                // only be added to the current track's lba to offset it to index 1.
                track.LogFrameOffset = track.Pregap;
            }

            track.PhysFrameOffset = physOffset;
            track.ChdFrameOffset = chdOffset;
            track.LogFrameOffset += logOffset;
            track.LogFrames = track.Frames - track.Pregap;

            // postgap counts against the next track
            logOffset += track.Postgap;

            physOffset += track.Frames;
            chdOffset += track.Frames;
            chdOffset += track.ExtraFrames;
            logOffset += track.Frames;
        }

        // fill out dummy entries for the last track to help our search
        var lastTrack = toc.Tracks[toc.NumTracks];
        lastTrack.PhysFrameOffset = physOffset;
        lastTrack.LogFrameOffset = logOffset;
        lastTrack.ChdFrameOffset = chdOffset;
        lastTrack.LogFrames = 0;
    }
}
