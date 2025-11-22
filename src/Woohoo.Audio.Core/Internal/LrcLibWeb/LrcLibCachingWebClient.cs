// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Internal.LrcLibWeb;

using System;
using Woohoo.Audio.Core.Internal.LrcLibWeb.Models;

public sealed class LrcLibCachingWebClient
{
    private static readonly TimeSpan CacheExpirationAge = TimeSpan.FromDays(7);
    private readonly LrcLibWebClient internalClient = new();
    private readonly string cacheFolder;

    public LrcLibCachingWebClient(string cacheFolder)
    {
        this.cacheFolder = cacheFolder;
    }

    public async Task<LrcLibResponse?> QueryAsync(string albumTitle, string artistName, string trackTitle, TimeSpan duration, bool allowExternalSources, CancellationToken cancellationToken)
    {
        var cache = new LrcLibResponseCache(this.cacheFolder, albumTitle, artistName, trackTitle, duration);

        if (cache.Exists &&
            cache.Age < CacheExpirationAge &&
            cache.TryReadFromCache(out var cachedResponse))
        {
            return cachedResponse;
        }

        try
        {
            var response = await this.internalClient.QueryAsync(albumTitle, artistName, trackTitle, duration, allowExternalSources, cancellationToken);
            cache.WriteToCache(response);
            return response;
        }
        catch
        {
            // We could not get a response from the server.
            // Client/server may be offline or whatever.
            // Use the expired cache when available.
            if (cache.Exists &&
                cache.TryReadFromCache(out var expiredCachedResponse))
            {
                return expiredCachedResponse;
            }

            throw;
        }
    }
}
