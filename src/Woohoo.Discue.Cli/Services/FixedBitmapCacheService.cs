// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Cli.Services;

using System;
using Woohoo.Audio.Services;

internal sealed class FixedBitmapCacheService : IBitmapCacheService
{
    public Task<string?> GetLocalFileAsync(Uri uri)
    {
        return Task.FromResult<string?>(null);
    }

    public Task<Uri?> GetLocalUriAsync(Uri uri)
    {
        return Task.FromResult<Uri?>(null);
    }
}
