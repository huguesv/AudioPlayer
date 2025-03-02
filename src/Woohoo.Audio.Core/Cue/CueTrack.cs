// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Cue;

public record class CueTrack
{
    /// <summary>
    /// Gets or sets the track number. Track numbers are in the range 1-99.
    /// </summary>
    public int TrackNumber { get; set; }

    /// <summary>
    /// Gets or sets the track mode.
    /// </summary>
    public string TrackMode { get; set; } = string.Empty;

    /// <summary>
    /// Gets the track flags.
    /// </summary>
    public List<string> Flags { get; init; } = [];

    /// <summary>
    /// Gets the track indexes.
    /// </summary>
    public List<CueIndex> Indexes { get; init; } = [];

    /// <summary>
    /// Gets or sets the track pregap. The pregap is filled with generated silence.
    /// </summary>
    public CueTime? PreGap { get; set; }

    /// <summary>
    /// Gets or sets the track postgap.
    /// </summary>
    public CueTime? PostGap { get; set; }

    /// <summary>
    /// Gets or sets the arranger name.
    /// </summary>
    public string? Arranger { get; set; }

    /// <summary>
    /// Gets or sets the composer name.
    /// </summary>
    public string? Composer { get; set; }

    /// <summary>
    /// Gets or sets the ISRC number. Format: CCOOOOYYSSSSS.
    /// </summary>
    public string? ISRC { get; set; }

    /// <summary>
    /// Gets or sets the message from the content provider and/or artist.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the performer name.
    /// </summary>
    public string? Performer { get; set; }

    /// <summary>
    /// Gets or sets the songwriter name.
    /// </summary>
    public string? Songwriter { get; set; }

    /// <summary>
    /// Gets or sets the track title.
    /// </summary>
    public string? Title { get; set; }
}
