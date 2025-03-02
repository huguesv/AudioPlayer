// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Cue.Serialization;

using System;

public class CueSheetWriter
{
    private const string Quote = "\"";
    private const string NewLine = "\r\n";
    private const string IndentLevel0 = "";
    private const string IndentLevel1 = "  ";
    private const string IndentLevel2 = "    ";

    public string Write(CueSheet cueSheet)
    {
        ArgumentNullException.ThrowIfNull(cueSheet);

        StringWriter writer = new StringWriter();
        this.Write(cueSheet, writer);
        return writer.ToString();
    }

    public void Write(CueSheet cueSheet, TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(cueSheet);
        ArgumentNullException.ThrowIfNull(writer);

        WriteProperty(writer, IndentLevel0, "CATALOG", cueSheet.Catalog);
        WriteProperty(writer, IndentLevel0, "CDTEXTFILE", cueSheet.CdTextFile, quote: true);
        WriteProperty(writer, IndentLevel0, "TITLE", cueSheet.Title, quote: true);
        WriteProperty(writer, IndentLevel0, "ARRANGER", cueSheet.Arranger, quote: true);
        WriteProperty(writer, IndentLevel0, "COMPOSER", cueSheet.Composer, quote: true);
        WriteProperty(writer, IndentLevel0, "MESSAGE", cueSheet.Message, quote: true);
        WriteProperty(writer, IndentLevel0, "PERFORMER", cueSheet.Performer, quote: true);
        WriteProperty(writer, IndentLevel0, "SONGWRITER", cueSheet.Songwriter, quote: true);
        WriteProperty(writer, IndentLevel0, "CDTEXTFILE", cueSheet.CdTextFile, quote: true);

        foreach (var file in cueSheet.Files)
        {
            foreach (var comment in file.Comments)
            {
                WriteProperty(writer, IndentLevel0, "REM", comment.Text);
            }

            WriteProperty(writer, IndentLevel0, "FILE", $"{Quote}{file.FileName}{Quote} {file.FileFormat}");

            foreach (var track in file.Tracks)
            {
                WriteProperty(writer, IndentLevel1, "TRACK", $"{track.TrackNumber:D2} {track.TrackMode}");

                WriteProperty(writer, IndentLevel2, "TITLE", track.Title, quote: true);
                WriteProperty(writer, IndentLevel2, "ARRANGER", track.Arranger, quote: true);
                WriteProperty(writer, IndentLevel2, "COMPOSER", track.Composer, quote: true);
                WriteProperty(writer, IndentLevel2, "PERFORMER", track.Performer, quote: true);
                WriteProperty(writer, IndentLevel2, "SONGWRITER", track.Songwriter, quote: true);
                WriteProperty(writer, IndentLevel2, "ISRC", track.ISRC);
                WriteProperty(writer, IndentLevel2, "PREGAP", track.PreGap?.ToString());
                WriteProperty(writer, IndentLevel2, "POSTGAP", track.PostGap?.ToString());

                if (track.Flags.Count > 0)
                {
                    WriteProperty(writer, IndentLevel2, "FLAGS", string.Join(" ", track.Flags));
                }

                foreach (var index in track.Indexes)
                {
                    WriteProperty(writer, IndentLevel2, "INDEX", $"{index.IndexNumber:D2} {index.Time}");
                }
            }
        }
    }

    private static void WriteProperty(TextWriter writer, string indent, string name, string? value, bool quote = false)
    {
        if (value is not null)
        {
            string quoteChar = quote ? Quote : string.Empty;
            writer.Write($"{indent}{name} {quoteChar}{value}{quoteChar}{NewLine}");
        }
    }
}
