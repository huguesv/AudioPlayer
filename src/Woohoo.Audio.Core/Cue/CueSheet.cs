// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Cue;

using System.Collections.Generic;

public record class CueSheet
{
    /// <summary>
    /// Gets the input files.
    /// </summary>
    public List<CueFile> Files { get; init; } = [];

    /// <summary>
    /// Gets or sets the catalog number of the CD. Format: 13 digits.
    /// </summary>
    public string? Catalog { get; set; }

    /// <summary>
    /// Gets or sets an external file for CD-TEXT data.
    /// </summary>
    public string? CdTextFile { get; set; }

    /// <summary>
    /// Gets or sets the arranger name.
    /// </summary>
    public string? Arranger { get; set; }

    /// <summary>
    /// Gets or sets the composer name.
    /// </summary>
    public string? Composer { get; set; }

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
    /// Gets or sets the album title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the album UPC/EAN code.
    /// </summary>
    public string? UpcEan { get; set; }
}
