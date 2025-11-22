// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Internal.LrcLibWeb.Models;

public sealed class LrcLibResponse
{
    public required long Id { get; set; }

    public string? Name { get; set; }

    public string? TrackName { get; set; }

    public string? ArtistName { get; set; }

    public string? AlbumName { get; set; }

    public double Duration { get; set; }

    public bool Instrumental { get; set; }

    public string? PlainLyrics { get; set; }

    public string? SyncedLyrics { get; set; }
}
