// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.LrcDatabase.Sqlite.Models;

public sealed class LrcLibResponse
{
    public int TrackId { get; set; }

    public string? Name { get; set; }

    public string? ArtistName { get; set; }

    public string? AlbumName { get; set; }

    public double? Duration { get; set; }

    public int? LastLyricsId { get; set; }

    public string? PlainLyrics { get; set; }

    public string? SyncedLyrics { get; set; }

    public bool? HasPlainLyrics { get; set; }

    public bool? HasSyncedLyrics { get; set; }

    public bool? Instrumental { get; set; }
}
