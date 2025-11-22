// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Models;

public sealed class UserSettings
{
    public bool FetchOnlineMetadata { get; set; } = false;

    public bool ShowAlbumArt { get; set; } = false;

    public bool FetchLyrics { get; set; } = false;

    public string LyricsDatabaseFilePath { get; set; } = string.Empty;
}
