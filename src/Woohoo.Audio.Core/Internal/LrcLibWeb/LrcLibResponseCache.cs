// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Internal.LrcLibWeb;

using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Woohoo.Audio.Core.Internal.LrcLibWeb.Models;

internal sealed class LrcLibResponseCache
{
    private static readonly JsonSerializerOptions SerializationOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly string filePath;

    public LrcLibResponseCache(string cacheFolder, string albumTitle, string artistName, string trackTitle, TimeSpan duration)
    {
        this.filePath = Path.Combine(cacheFolder, CreateMD5($"{albumTitle}:{artistName}:{trackTitle}:{(long)duration.TotalSeconds}") + ".json");
    }

    public TimeSpan? Age
    {
        get => GetCacheAge(this.filePath);
    }

    public bool Exists
    {
        get => File.Exists(this.filePath);
    }

    public bool TryReadFromCache(out LrcLibResponse? response)
    {
        return TryReadFromCache(this.filePath, out response);
    }

    public void WriteToCache(LrcLibResponse? result)
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

    private static bool TryReadFromCache(string cacheFilePath, out LrcLibResponse? response)
    {
        response = null;

        try
        {
            // Empty file indicates no result
            var fileInfo = new FileInfo(cacheFilePath);
            if (fileInfo.Length == 0)
            {
                return true;
            }

            using var fileStream = File.OpenRead(cacheFilePath);
            response = JsonSerializer.Deserialize<LrcLibResponse>(fileStream, SerializationOptions);
            return response is not null;
        }
        catch
        {
            // Delete corrupted cache
            SafeDelete(cacheFilePath);
        }

        return false;
    }

    private static void WriteToCache(string cacheFilePath, LrcLibResponse? result)
    {
        try
        {
            var folderPath = Path.GetDirectoryName(cacheFilePath);
            if (folderPath is null)
            {
                return;
            }

            Directory.CreateDirectory(folderPath);

            using var fileStream = File.Create(cacheFilePath);

            // Empty file indicates no result
            if (result is not null)
            {
                JsonSerializer.Serialize(fileStream, result, SerializationOptions);
            }
        }
        catch
        {
            // Ignore cache write errors
        }
    }
}
