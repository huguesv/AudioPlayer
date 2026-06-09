// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Services.Impl;

using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Woohoo.Audio.Services;

public sealed class BitmapCacheService : IBitmapCacheService
{
    private readonly IHttpClientFactory httpClientFactory;

    public BitmapCacheService(IHttpClientFactory httpClientFactory)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);

        this.httpClientFactory = httpClientFactory;
    }

    public required string CacheFolderPath { get; init; }

    public async Task<Uri?> GetLocalUriAsync(Uri uri)
    {
        var filePath = await this.GetLocalFileAsync(uri);
        return filePath is not null ? CreateLocalUri(filePath) : null;
    }

    public async Task<string?> GetLocalFileAsync(Uri uri)
    {
        var filePath = Path.Combine(this.CacheFolderPath, ComputeLocalFileName(uri));
        if (File.Exists(filePath))
        {
            return filePath;
        }

        var httpClient = this.httpClientFactory.CreateClient();
        try
        {
            var result = await httpClient.GetByteArrayAsync(uri);

            Directory.CreateDirectory(this.CacheFolderPath);
            await File.WriteAllBytesAsync(filePath, result);

            return filePath;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"{nameof(BitmapCacheService)}: Error downloading '{uri}':\n{ex}");
            return null;
        }
    }

    private static Uri CreateLocalUri(string filePath)
    {
        var localUri = new Uri($"file:///{filePath.Replace("\\", "/")}");
        return localUri;
    }

    private static string ComputeLocalFileName(Uri uri)
    {
        var name = GetMD5(uri.ToString()) + GetExtension(uri.ToString());
        return name;
    }

    private static string GetExtension(string uri)
    {
        var lastDotIndex = uri.LastIndexOf('.');
        if (lastDotIndex >= 0)
        {
            return uri[lastDotIndex..];
        }

        return string.Empty;
    }

    private static string GetMD5(string input)
    {
        var inputBytes = Encoding.ASCII.GetBytes(input);
        var hashBytes = MD5.HashData(inputBytes);
        return Convert.ToHexString(hashBytes);
    }
}
