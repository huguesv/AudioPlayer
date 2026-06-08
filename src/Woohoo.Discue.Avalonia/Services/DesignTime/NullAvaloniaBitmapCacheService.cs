// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Services;

using System;
using Avalonia.Media;
using Woohoo.Discue.Contracts.Services;

internal class NullAvaloniaBitmapCacheService : IAvaloniaBitmapCacheService
{
    public Task<IImage?> GetLocalImageAsync(Uri uri)
    {
        return Task.FromResult<IImage?>(null);
    }
}
