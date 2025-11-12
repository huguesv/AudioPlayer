// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Internal.CueToolsDatabase;

using System;
using System.Text;
using System.Xml.Serialization;
using Woohoo.Audio.Core.Internal.CueToolsDatabase.Models;

internal sealed class CTDBResponseCache
{
    private readonly string filePath;

    public CTDBResponseCache(string cacheFolder, string toc)
    {
        this.filePath = Path.Combine(cacheFolder, CreateMD5(toc) + ".xml");
    }

    public TimeSpan? Age
    {
        get => GetCacheAge(this.filePath);
    }

    public bool Exists
    {
        get => File.Exists(this.filePath);
    }

    public bool TryReadFromCache(out CTDBResponse? response)
    {
        return CTDBResponseCache.TryReadFromCache(this.filePath, out response);
    }

    public void WriteToCache(CTDBResponse? result)
    {
        WriteToCache(this.filePath, result);
    }

    private static string CreateMD5(string input)
    {
        var inputBytes = Encoding.ASCII.GetBytes(input);
        var hashBytes = MD5.HashData(inputBytes);
        return Convert.ToHexString(hashBytes);
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

    private static TimeSpan? GetCacheAge(string cacheFilePath)
    {
        var cacheFileInfo = new FileInfo(cacheFilePath);
        if (!cacheFileInfo.Exists)
        {
            return null;
        }

        return DateTime.UtcNow - cacheFileInfo.LastWriteTimeUtc;
    }

    private static bool TryReadFromCache(string cacheFilePath, out CTDBResponse? response)
    {
        response = null;

        try
        {
            var serializer = new XmlSerializer(typeof(CTDBResponse));
            using var fileStream = File.OpenRead(cacheFilePath);
            response = serializer.Deserialize(fileStream) as CTDBResponse;
            return response is not null;
        }
        catch
        {
            // Delete corrupted cache
            SafeDelete(cacheFilePath);
        }

        return false;
    }

    private static void WriteToCache(string cacheFilePath, CTDBResponse? result)
    {
        try
        {
            var folderPath = Path.GetDirectoryName(cacheFilePath);
            if (folderPath is null)
            {
                return;
            }

            Directory.CreateDirectory(folderPath);

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
