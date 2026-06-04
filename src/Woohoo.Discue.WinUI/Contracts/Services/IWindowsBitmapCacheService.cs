// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Contracts.Services;

using Microsoft.UI.Xaml.Media.Imaging;

public interface IWindowsBitmapCacheService
{
    Task<BitmapImage?> GetLocalImageAsync(Uri uri);
}
