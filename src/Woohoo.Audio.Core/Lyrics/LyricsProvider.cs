// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Lyrics;

using System;
using System.Threading;
using System.Threading.Tasks;
using Woohoo.Audio.Core.Internal.LrcLibSqliteDatabase;
using Woohoo.Audio.Core.Internal.LrcLibWeb;
using Woohoo.Audio.Core.Lyrics.Serialization;

public class LyricsProvider : ILyricsProvider
{
    private readonly LrcLibDatabaseClient? databaseClient;
    private readonly LrcLibCachingWebClient webClient;
    private readonly LyricsProviderOptions options;

    public LyricsProvider(LyricsProviderOptions options)
    {
        this.databaseClient = File.Exists(options.DatabaseFilePath) && options.UseDatabase ? new LrcLibDatabaseClient(new LrcLibDatabaseConnection(options.DatabaseFilePath)) : null;
        this.webClient = new LrcLibCachingWebClient(Path.Combine(Path.GetTempPath(), "Woohoo.Audio", "LrcLibCache"));
        this.options = options;
    }

    public async Task<LyricsTrack?> QueryAsync(string albumTitle, string artistName, string trackTitle, TimeSpan duration, CancellationToken cancellationToken)
    {
        if (this.options.UseDatabase && this.databaseClient is not null)
        {
            try
            {
                var databaseResult = await this.databaseClient.QueryAsync(albumTitle, artistName, trackTitle, duration, cancellationToken);
                var databaseLyrics = TryCreate(databaseResult?.SyncedLyrics, databaseResult?.PlainLyrics);
                if (databaseLyrics is not null)
                {
                    return databaseLyrics;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LyricsProvider: Database query failed: {ex}");
            }
        }

        if (this.options.UseWeb)
        {
            try
            {
                var webResult = await this.webClient.QueryAsync(albumTitle, artistName, trackTitle, duration, this.options.UseWebExternalSources, cancellationToken);
                var webLyrics = TryCreate(webResult?.SyncedLyrics, webResult?.PlainLyrics);
                if (webLyrics is not null)
                {
                    return webLyrics;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LyricsProvider: Web query failed: {ex}");
            }
        }

        return null;
    }

    private static LyricsTrack? TryCreate(string? syncedLyrics, string? plainLyrics)
    {
        if (syncedLyrics is null && plainLyrics is null)
        {
            return null;
        }

        return new LyricsTrack
        {
            SyncedLines = syncedLyrics is not null ? LrcReader.Parse(syncedLyrics).SyncedLines : [],
            PlainText = plainLyrics ?? string.Empty,
        };
    }
}
