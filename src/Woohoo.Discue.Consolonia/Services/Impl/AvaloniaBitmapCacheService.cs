// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Consolonia.Services.Impl;

using Avalonia.Media;
using Avalonia.Media.Imaging;
using Woohoo.Audio.Services;

public sealed class AvaloniaBitmapCacheService : IAvaloniaBitmapCacheService
{
    private readonly IBitmapCacheService bitmapCacheService;
    private readonly Dictionary<Uri, IImage?> memoryCache;

    public AvaloniaBitmapCacheService(IBitmapCacheService bitmapCacheService)
    {
        ArgumentNullException.ThrowIfNull(bitmapCacheService);

        this.bitmapCacheService = bitmapCacheService;
        this.memoryCache = [];
    }

    public async Task<IImage?> GetLocalImageAsync(Uri uri)
    {
        if (this.memoryCache.TryGetValue(uri, out var cachedImage))
        {
            return cachedImage;
        }

        var filePath = await this.bitmapCacheService.GetLocalFileAsync(uri);
        if (File.Exists(filePath))
        {
            var bitmap = new Bitmap(filePath);
            this.memoryCache[uri] = bitmap;
            return bitmap;
        }

        this.memoryCache[uri] = null;
        return null;
    }
}
