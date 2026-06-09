// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Avalonia.Services.DesignTime;

using System;
using global::Avalonia.Media;
using Woohoo.Discue.Shared.Avalonia.Services;

internal class NullAvaloniaBitmapCacheService : IAvaloniaBitmapCacheService
{
    public Task<IImage?> GetLocalImageAsync(Uri uri)
    {
        return Task.FromResult<IImage?>(null);
    }
}
