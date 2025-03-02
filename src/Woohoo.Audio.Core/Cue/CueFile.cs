// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Cue;

public record class CueFile
{
    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file format.
    /// </summary>
    public string FileFormat { get; set; } = string.Empty;

    /// <summary>
    /// Gets the file tracks.
    /// </summary>
    public List<CueTrack> Tracks { get; init; } = [];

    /// <summary>
    /// Gets the comments preceding the file.
    /// </summary>
    public List<CueComment> Comments { get; init; } = [];
}
