// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Cli;

internal record class Track
{
    public string FileName { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public int TrackNumber { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Performer { get; set; } = string.Empty;
}
