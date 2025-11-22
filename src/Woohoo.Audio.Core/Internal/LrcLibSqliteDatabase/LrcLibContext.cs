// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Internal.LrcLibSqliteDatabase;

using Microsoft.EntityFrameworkCore;
using Woohoo.Audio.Core.Internal.LrcLibSqliteDatabase.Models;

public partial class LrcLibContext : DbContext
{
    private readonly string filePath;

    public LrcLibContext(DbContextOptions<LrcLibContext> options, string filePath)
        : base(options)
    {
        this.filePath = filePath;
    }

    public virtual DbSet<Lyric> Lyrics { get; set; }

    public virtual DbSet<Track> Tracks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lyric>(entity =>
        {
            entity.ToTable("lyrics");

            entity.HasIndex(e => e.CreatedAt, "idx_lyrics_created_at");

            entity.HasIndex(e => e.HasPlainLyrics, "idx_lyrics_has_plain_lyrics");

            entity.HasIndex(e => e.HasSyncedLyrics, "idx_lyrics_has_synced_lyrics");

            entity.HasIndex(e => e.Source, "idx_lyrics_source");

            entity.HasIndex(e => e.TrackId, "idx_lyrics_track_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("DATETIME")
                .HasColumnName("created_at");
            entity.Property(e => e.HasPlainLyrics)
                .HasColumnType("BOOLEAN")
                .HasColumnName("has_plain_lyrics");
            entity.Property(e => e.HasSyncedLyrics)
                .HasColumnType("BOOLEAN")
                .HasColumnName("has_synced_lyrics");
            entity.Property(e => e.Instrumental)
                .HasColumnType("BOOLEAN")
                .HasColumnName("instrumental");
            entity.Property(e => e.PlainLyrics).HasColumnName("plain_lyrics");
            entity.Property(e => e.Source).HasColumnName("source");
            entity.Property(e => e.SyncedLyrics).HasColumnName("synced_lyrics");
            entity.Property(e => e.TrackId).HasColumnName("track_id");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("DATETIME")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Track).WithMany(p => p.Lyrics).HasForeignKey(d => d.TrackId);
        });

        modelBuilder.Entity<Track>(entity =>
        {
            entity.ToTable("tracks");

            entity.HasIndex(e => new { e.NameLower, e.ArtistNameLower, e.AlbumNameLower, e.Duration }, "IX_tracks_name_lower_artist_name_lower_album_name_lower_duration").IsUnique();

            entity.HasIndex(e => e.AlbumNameLower, "idx_tracks_album_name_lower");

            entity.HasIndex(e => e.ArtistNameLower, "idx_tracks_artist_name_lower");

            entity.HasIndex(e => e.Duration, "idx_tracks_duration");

            entity.HasIndex(e => e.LastLyricsId, "idx_tracks_last_lyrics_id");

            entity.HasIndex(e => e.NameLower, "idx_tracks_name_lower");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AlbumName).HasColumnName("album_name");
            entity.Property(e => e.AlbumNameLower).HasColumnName("album_name_lower");
            entity.Property(e => e.ArtistName).HasColumnName("artist_name");
            entity.Property(e => e.ArtistNameLower).HasColumnName("artist_name_lower");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("DATETIME")
                .HasColumnName("created_at");
            entity.Property(e => e.Duration)
                .HasColumnType("FLOAT")
                .HasColumnName("duration");
            entity.Property(e => e.LastLyricsId).HasColumnName("last_lyrics_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.NameLower).HasColumnName("name_lower");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("DATETIME")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.LastLyrics).WithMany(p => p.Tracks).HasForeignKey(d => d.LastLyricsId);
        });

        this.OnModelCreatingPartial(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Configure the connection string
        // The Data Source path is relative to the output directory (e.g., bin) by default
        optionsBuilder.UseSqlite($"Data Source={this.filePath}");
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
