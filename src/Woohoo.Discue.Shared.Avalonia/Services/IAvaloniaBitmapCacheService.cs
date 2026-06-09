// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Shared.Avalonia.Services;

using global::Avalonia.Media;

public interface IAvaloniaBitmapCacheService
{
    Task<IImage?> GetLocalImageAsync(Uri uri);
}
