// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Internal.CueToolsDatabase;

using System;
using System.Threading.Tasks;
using Woohoo.Audio.Core.Internal.CueToolsDatabase.Models;

internal sealed class CTDBCachingWebClient : ICTDBWebClient
{
    private static readonly TimeSpan CacheExpirationAge = TimeSpan.FromDays(7);
    private readonly ICTDBWebClient innerClient;
    private readonly string cacheFolder;

    public CTDBCachingWebClient(string cacheFolder, ICTDBWebClient innerClient)
    {
        this.cacheFolder = cacheFolder;
        this.innerClient = innerClient;
    }

    public async Task<CTDBResponse?> QueryAsync(string toc, CancellationToken cancellationToken)
    {
        var cache = new CTDBResponseCache(this.cacheFolder, toc);

        if (cache.Exists &&
            cache.Age < CacheExpirationAge &&
            cache.TryReadFromCache(out var cachedResponse))
        {
            return cachedResponse;
        }

        try
        {
            var response = await this.innerClient.QueryAsync(toc, cancellationToken);
            if (response is not null)
            {
                cache.WriteToCache(response);
            }

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
