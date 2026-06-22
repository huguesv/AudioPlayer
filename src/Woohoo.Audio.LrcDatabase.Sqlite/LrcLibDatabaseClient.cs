// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.LrcDatabase.Sqlite;

using System;
using Dapper;
using Woohoo.Audio.LrcDatabase.Sqlite.Models;

public sealed class LrcLibDatabaseClient
{
    private readonly LrcLibDatabaseConnection connection;

    public LrcLibDatabaseClient(LrcLibDatabaseConnection connection)
    {
        this.connection = connection;
    }

    public async Task<LrcLibResponse?> QueryAsync(string albumTitle, string artistName, string trackTitle, TimeSpan duration, CancellationToken cancellationToken)
    {
        string selectSql = """
            SELECT
                t.id AS TrackId,
                t.name AS Name,
                t.artist_name AS ArtistName,
                t.album_name AS AlbumName,
                t.duration AS Duration,
                t.last_lyrics_id,
                l.id AS LastLyricsId,
                l.plain_lyrics AS PlainLyrics,
                l.synced_lyrics AS SyncedLyrics,
                l.plain_lyrics AS PlainLyrics,
                l.synced_lyrics AS SyncedLyrics,
                l.has_plain_lyrics AS HasPlainLyrics,
                l.has_synced_lyrics AS HasSyncedLyrics,
                l.instrumental AS Instrumental
            FROM tracks t
            INNER JOIN lyrics l ON t.last_lyrics_id = l.id
            WHERE t.duration IS NOT NULL
            AND t.album_name_lower = @AlbumLower
            AND t.artist_name_lower = @ArtistLower
            AND t.name_lower = @TrackLower
            """;

        var tracks = await this.connection.SqliteConnection.QueryAsync<LrcLibResponse>(
            selectSql,
            new
            {
                AlbumLower = albumTitle.ToLower(),
                ArtistLower = artistName.ToLower(),
                TrackLower = trackTitle.ToLower(),
            });

        var allTracks = tracks
            .OrderBy(t => Math.Abs(t.Duration!.Value - duration.TotalSeconds))
            .ToList();

        var firstSyncMatch = allTracks.FirstOrDefault(t => t.HasSyncedLyrics == true && !string.IsNullOrEmpty(t.SyncedLyrics));
        var firstPlainMatch = allTracks.FirstOrDefault(t => t.HasPlainLyrics == true && !string.IsNullOrEmpty(t.PlainLyrics));

        return firstSyncMatch ?? firstPlainMatch;
    }
}
