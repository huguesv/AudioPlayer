// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Services;

public interface IBitmapCacheService
{
    Task<string?> GetLocalFileAsync(Uri uri);

    Task<Uri?> GetLocalUriAsync(Uri uri);
}
