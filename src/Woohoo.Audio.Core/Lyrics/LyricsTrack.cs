// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Lyrics;

using System.Collections.Immutable;

public sealed class LyricsTrack
{
    public ImmutableArray<LyricsLine> SyncedLines { get; init; } = [];

    public string PlainText { get; init; } = string.Empty;

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
