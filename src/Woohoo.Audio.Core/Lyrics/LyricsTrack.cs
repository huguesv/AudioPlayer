// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Lyrics;

using System.Collections.Immutable;
using Woohoo.Audio.Core.Internal.LrcLibSqliteDatabase.Models;

public sealed class LyricsTrack
{
    public ImmutableArray<LyricsLine> SyncedLines { get; init; } = [];

    public string PlainText { get; init; } = string.Empty;

    public IEnumerable<LyricsLine> EnumerateLines()
    {
        if (this.SyncedLines.Length > 0)
        {
            foreach (var line in this.SyncedLines)
            {
                yield return line;
            }
        }
        else if (this.PlainText.Length > 0)
        {
            foreach (var line in this.PlainText.Split(new[] { '\r', '\n' }, StringSplitOptions.TrimEntries))
            {
                yield return new LyricsLine()
                {
                    Text = line,
                    Timestamp = TimeSpan.Zero,
                };
            }
        }
    }

    public string GetLineAt(TimeSpan time)
    {
        var index = this.GetLineIndexAt(time);
        if (index >= 0 && index < this.SyncedLines.Length)
        {
            return this.SyncedLines[index].Text;
        }

        return string.Empty;
    }

    public int GetLineIndexAt(TimeSpan time)
    {
        for (int i = 0; i < this.SyncedLines.Length; i++)
        {
            var line = this.SyncedLines[i];
            var nextLine = i + 1 < this.SyncedLines.Length ? this.SyncedLines[i + 1] : null;
            if (time >= line.Timestamp && (nextLine is null || time < nextLine.Timestamp))
            {
                return i;
            }
        }

        return -1;
    }
}
