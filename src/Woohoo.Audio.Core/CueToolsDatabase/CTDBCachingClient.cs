// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.CueToolsDatabase;

using System;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Woohoo.Audio.Core.CueToolsDatabase.Models;

internal sealed class CTDBCachingClient
{
    private static readonly TimeSpan CacheExpirationAge = TimeSpan.FromDays(7);
    private readonly CTDBClient internalClient = new();
    private readonly string cacheFolder;

    public CTDBCachingClient(string cacheFolder)
    {
        this.cacheFolder = cacheFolder;
    }

    public async Task<CTDBResponse?> QueryAsync(string toc, CancellationToken cancellationToken)
    {
        var tocEncodedBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(toc));
        var cacheFilePath = Path.Combine(this.cacheFolder, tocEncodedBase64 + ".xml");

        if (this.TryReadFromCache(cacheFilePath, out var cachedResponse))
        {
            return cachedResponse;
        }

        var response = await this.internalClient.QueryAsync(toc, cancellationToken);
        if (response is not null)
        {
            this.WriteToCache(cacheFilePath, response);
        }

        return response;
    }

    private static void SafeDelete(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch
        {
            // Ignore deletion errors
        }
    }

    private bool TryReadFromCache(string cacheFilePath, out CTDBResponse? response)
    {
        response = null;

        var cacheFileInfo = new FileInfo(cacheFilePath);
        if (!cacheFileInfo.Exists)
        {
            return false;
        }

        // If it's older than 7 days, ignore the cache
        if (DateTime.UtcNow - cacheFileInfo.LastWriteTimeUtc > CacheExpirationAge)
        {
            SafeDelete(cacheFilePath);
            return false;
        }

        try
        {
            var serializer = new XmlSerializer(typeof(CTDBResponse));
            using var fileStream = File.OpenRead(cacheFilePath);
            response = serializer.Deserialize(fileStream) as CTDBResponse;
            return response is not null;
        }
        catch
        {
            // Ignore cache read errors
            SafeDelete(cacheFilePath);
        }

        return false;
    }

    private void WriteToCache(string cacheFilePath, CTDBResponse? result)
    {
        try
        {
            Directory.CreateDirectory(this.cacheFolder);
            var serializer = new XmlSerializer(typeof(CTDBResponse));
            using var fileStream = File.Create(cacheFilePath);
            serializer.Serialize(fileStream, result);
        }
        catch
        {
            // Ignore cache write errors
        }
    }
}
