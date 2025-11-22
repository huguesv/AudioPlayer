// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Internal.LrcLibSqliteDatabase.Models;

public partial class Lyric
{
    public int Id { get; set; }

    public string? PlainLyrics { get; set; }

    public string? SyncedLyrics { get; set; }

    public int? TrackId { get; set; }

    public bool? HasPlainLyrics { get; set; }

    public bool? HasSyncedLyrics { get; set; }

    public bool? Instrumental { get; set; }

    public string? Source { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public virtual Track? Track { get; set; }

    public virtual ICollection<Track> Tracks { get; set; } = [];
}
