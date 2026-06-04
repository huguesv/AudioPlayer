// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Services;

using System;
using Microsoft.UI.Xaml.Media.Imaging;
using Woohoo.Audio.Services;
using Woohoo.Discue.Contracts.Services;

public sealed class WindowsBitmapCacheService : IWindowsBitmapCacheService
{
    private readonly IBitmapCacheService bitmapCacheService;
    private readonly Dictionary<Uri, BitmapImage?> memoryCache;

    public WindowsBitmapCacheService(IBitmapCacheService bitmapCacheService)
    {
        ArgumentNullException.ThrowIfNull(bitmapCacheService);

        this.bitmapCacheService = bitmapCacheService;
        this.memoryCache = [];
    }

    public async Task<BitmapImage?> GetLocalImageAsync(Uri uri)
    {
        if (this.memoryCache.TryGetValue(uri, out var cachedImage))
        {
            return cachedImage;
        }

        var filePath = await this.bitmapCacheService.GetLocalFileAsync(uri);
        if (File.Exists(filePath))
        {
            var localUri = CreateLocalUri(filePath);
            var bitmap = new BitmapImage(localUri);
            this.memoryCache[uri] = bitmap;
            return bitmap;
        }

        this.memoryCache[uri] = null;
        return null;
    }

    private static Uri CreateLocalUri(string filePath)
    {
        var localUri = new Uri($"file:///{filePath.Replace("\\", "/")}");
        return localUri;
    }
}
