// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Cue;

public record class CueIndex
{
    /// <summary>
    /// Gets or sets the index number. Index numbers are in the range 0-99.
    /// </summary>
    public required int IndexNumber { get; set; }

    /// <summary>
    /// Gets or sets the time.
    /// </summary>
    public required CueTime Time { get; set; }
}
