// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Tui.Services;

using System;
using Woohoo.Audio.Services;

internal class MruLocationProviderService : IMruLocationProviderService
{
    private static readonly string DataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Woohoo.Discue.Consolonia");

    private static readonly string FileName = "mru.json";

    public string MruFilePath => Path.Combine(DataFolder, FileName);
}
