// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Tui.Services;

using System;
using Woohoo.Audio.Services;

internal class CacheLocationProviderService : ICacheLocationProviderService
{
    private static readonly string DataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Woohoo.Discue.Consolonia");

    public string CacheFolderPath => Path.Combine(DataFolder, "Cache");
}
