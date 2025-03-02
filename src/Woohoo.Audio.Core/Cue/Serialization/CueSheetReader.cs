// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Cue.Serialization;

using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

public class CueSheetReader
{
    public CueSheet Parse(string data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var lines = data.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var sheet = new CueSheet();

        List<CueComment> comments = new();
        CueFile? file = null;
        CueTrack? track = null;
        foreach (var line in lines)
        {
            if (line.StartsWith("REM"))
            {
                comments.Add(new CueComment { Text = line[4..] });
            }
            else if (line.StartsWith("CATALOG"))
            {
                sheet.Catalog = line[8..];
            }
            else if (line.StartsWith("TITLE"))
            {
                sheet.Title = line[7..^1];
            }
            else if (line.StartsWith("ARRANGER"))
            {
                sheet.Arranger = line[10..^1];
            }
            else if (line.StartsWith("COMPOSER"))
            {
                sheet.Composer = line[10..^1];
            }
            else if (line.StartsWith("PERFORMER"))
            {
                sheet.Performer = line[11..^1];
            }
            else if (line.StartsWith("SONGWRITER"))
            {
                sheet.Songwriter = line[12..^1];
            }
            else if (line.StartsWith("CDTEXTFILE"))
            {
                sheet.CdTextFile = line[12..^1];
            }
            else if (line.StartsWith("FILE"))
            {
                if (file is not null)
                {
                    if (track is not null)
                    {
                        file.Tracks.Add(track);
                    }

                    sheet.Files.Add(file);
                }

                file = new CueFile();
                track = null;
                if (TryParseFile(line, out var fileName, out var fileFormat))
                {
                    file.FileName = fileName;
                    file.FileFormat = fileFormat;
                }

                file.Comments.AddRange(comments);
                comments.Clear();
            }
            else if (line.StartsWith("  TRACK"))
            {
                if (TryParseTrack(line, out var trackNumber, out var trackMode))
                {
                    Debug.Assert(file is not null, "TRACK without previous FILE");
                    if (file is not null)
                    {
                        if (track is not null)
                        {
                            file.Tracks.Add(track);
                        }

                        track = new CueTrack
                        {
                            TrackNumber = trackNumber,
                            TrackMode = trackMode,
                        };
                    }
                }
            }
            else if (line.StartsWith("    ARRANGER"))
            {
                Debug.Assert(track is not null, "ARRANGER without previous TRACK");
                if (track is not null)
                {
                    track.Arranger = line[14..^1];
                }
            }
            else if (line.StartsWith("    COMPOSER"))
            {
                Debug.Assert(track is not null, "COMPOSER without previous TRACK");
                if (track is not null)
                {
                    track.Composer = line[14..^1];
                }
            }
            else if (line.StartsWith("    ISRC"))
            {
                Debug.Assert(track is not null, "ISRC without previous TRACK");
                if (track is not null)
                {
                    track.ISRC = line[9..];
                }
            }
            else if (line.StartsWith("    MESSAGE"))
            {
                Debug.Assert(track is not null, "MESSAGE without previous TRACK");
                if (track is not null)
                {
                    track.Message = line[13..^1];
                }
            }
            else if (line.StartsWith("    PERFORMER"))
            {
                Debug.Assert(track is not null, "PERFORMER without previous TRACK");
                if (track is not null)
                {
                    track.Performer = line[15..^1];
                }
            }
            else if (line.StartsWith("    SONGWRITER"))
            {
                Debug.Assert(track is not null, "SONGWRITER without previous TRACK");
                if (track is not null)
                {
                    track.Songwriter = line[16..^1];
                }
            }
            else if (line.StartsWith("    TITLE"))
            {
                Debug.Assert(track is not null, "TITLE without previous TRACK");
                if (track is not null)
                {
                    track.Title = line[11..^1];
                }
            }
            else if (line.StartsWith("    INDEX"))
            {
                Debug.Assert(track is not null, "INDEX without previous TRACK");
                if (track is not null)
                {
                    if (TryParseIndex(line, out var number, out var time))
                    {
                        track.Indexes.Add(new CueIndex { IndexNumber = number, Time = time });
                    }
                }
            }
            else if (line.StartsWith("    PREGAP"))
            {
                Debug.Assert(track is not null, "PREGAP without previous TRACK");
                if (track is not null)
                {
                    if (TryParseGap(line, out var time))
                    {
                        track.PreGap = time;
                    }
                }
            }
            else if (line.StartsWith("    POSTGAP"))
            {
                Debug.Assert(track is not null, "POSTGAP without previous TRACK");
                if (track is not null)
                {
                    if (TryParseGap(line, out var time))
                    {
                        track.PostGap = time;
                    }
                }
            }
            else if (line.StartsWith("    FLAGS"))
            {
                Debug.Assert(track is not null, "FLAGS without previous TRACK");
                if (track is not null)
                {
                    string values = line[10..];
                    track.Flags.AddRange(values.Split(' ', StringSplitOptions.RemoveEmptyEntries));
                }
            }
        }

        if (file is not null)
        {
            if (track is not null)
            {
                file.Tracks.Add(track);
            }

            sheet.Files.Add(file);
        }

        return sheet;
    }

    private static bool TryParseTrack(string line, out int trackNumber, out string trackMode)
    {
        var pattern = @"TRACK\s(\d+)\s([\w/]+)";
        var match = Regex.Match(line, pattern);
        if (match.Success)
        {
            trackNumber = int.Parse(match.Groups[1].Value);
            trackMode = match.Groups[2].Value;
            return true;
        }

        trackNumber = 0;
        trackMode = string.Empty;
        return false;
    }

    private static bool TryParseFile(string line, out string fileName, out string fileFormat)
    {
        var pattern = @"FILE\s""([^""]+)""\s(\w+)";
        var match = Regex.Match(line, pattern);
        if (match.Success)
        {
            fileName = match.Groups[1].Value;
            fileFormat = match.Groups[2].Value;
            return true;
        }

        fileName = string.Empty;
        fileFormat = string.Empty;
        return false;
    }

    private static bool TryParseIndex(string line, out int number, out CueTime time)
    {
        var pattern = @"\sINDEX\s(\d+)\s(\d+):(\d+):(\d+)";
        var match = Regex.Match(line, pattern);
        if (match.Success)
        {
            number = int.Parse(match.Groups[1].Value);
            time = new CueTime
            {
                Minutes = int.Parse(match.Groups[2].Value),
                Seconds = int.Parse(match.Groups[3].Value),
                Frames = int.Parse(match.Groups[4].Value),
            };
            return true;
        }

        number = 0;
        time = new CueTime();
        return false;
    }

    private static bool TryParseGap(string line, out CueTime time)
    {
        var pattern = @"\s(PREGAP|POSTGAP)\s(\d+):(\d+):(\d+)";
        var match = Regex.Match(line, pattern);
        if (match.Success)
        {
            time = new CueTime
            {
                Minutes = int.Parse(match.Groups[2].Value),
                Seconds = int.Parse(match.Groups[3].Value),
                Frames = int.Parse(match.Groups[4].Value),
            };
            return true;
        }

        time = new CueTime();
        return false;
    }
}
