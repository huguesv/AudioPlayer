// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Services;

using Windows.Storage;
using Woohoo.Audio.Services;

internal sealed class CacheLocationProviderService : ICacheLocationProviderService
{
    public string CacheFolderPath { get; } = ApplicationData.Current.LocalCacheFolder.Path;
}
