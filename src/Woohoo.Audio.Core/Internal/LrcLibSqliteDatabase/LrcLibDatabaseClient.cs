// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Internal.LrcLibSqliteDatabase;

using System;
using Microsoft.EntityFrameworkCore;
using Woohoo.Audio.Core.Internal.LrcLibSqliteDatabase.Models;

public sealed class LrcLibDatabaseClient
{
    private readonly LrcLibDatabaseConnection connection;

    public LrcLibDatabaseClient(LrcLibDatabaseConnection connection)
    {
        this.connection = connection;
    }

    public async Task<Lyric?> QueryAsync(string albumTitle, string artistName, string trackTitle, TimeSpan duration, CancellationToken cancellationToken)
    {
        var allTracks = await this.connection
            .Context
            .Tracks
            .AsNoTracking()
            .Where(t => t.AlbumNameLower == albumTitle.ToLower() && t.NameLower == trackTitle.ToLower() && t.ArtistNameLower == artistName.ToLower())
            .Include(t => t.LastLyrics)
            .Where(t => t.Duration.HasValue)
            .OrderBy(t => Math.Abs(t.Duration!.Value - duration.TotalSeconds))
            .ToListAsync(cancellationToken);

        var firstSyncMatch = allTracks.FirstOrDefault(t => t.LastLyrics?.HasSyncedLyrics == true && !string.IsNullOrEmpty(t.LastLyrics.SyncedLyrics));
        var firstPlainMatch = allTracks.FirstOrDefault(t => t.LastLyrics?.HasPlainLyrics == true && !string.IsNullOrEmpty(t.LastLyrics.PlainLyrics));

        return firstSyncMatch?.LastLyrics ?? firstPlainMatch?.LastLyrics;
    }
}
