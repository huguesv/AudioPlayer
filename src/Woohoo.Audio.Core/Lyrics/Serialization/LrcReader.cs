// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Lyrics.Serialization;

using System;
using System.Collections.Generic;

public static class LrcReader
{
    public static LyricsTrack Parse(string lrcContent)
    {
        var lines = new List<LyricsLine>();
        var lrcLines = lrcContent.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lrcLines)
        {
            var parts = line.Split([']'], 2);
            if (parts.Length != 2 || !parts[0].StartsWith('['))
            {
                continue; // Invalid line format
            }

            var timestampStr = parts[0].TrimStart('[');
            if (TimeSpan.TryParseExact(timestampStr, @"mm\:ss\.ff", null, out var timestamp) ||
                TimeSpan.TryParseExact(timestampStr, @"mm\:ss", null, out timestamp))
            {
                var text = parts[1].Trim();
                lines.Add(new LyricsLine { Timestamp = timestamp, Text = text });
            }
        }

        return new LyricsTrack
        {
            SyncedLines = [.. lines],
        };
    }
}
