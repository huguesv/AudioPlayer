// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Internal.LrcLibSqliteDatabase.Models;

public partial class Track
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? NameLower { get; set; }

    public string? ArtistName { get; set; }

    public string? ArtistNameLower { get; set; }

    public string? AlbumName { get; set; }

    public string? AlbumNameLower { get; set; }

    public double? Duration { get; set; }

    public int? LastLyricsId { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public virtual Lyric? LastLyrics { get; set; }

    public virtual ICollection<Lyric> Lyrics { get; set; } = [];
}
