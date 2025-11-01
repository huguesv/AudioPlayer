// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Cue;

using System;

/// <summary>
/// Time in MSF format (minutes, seconds, frames).
/// One frame is 1/75 of a second.
/// </summary>
public record class CueTime
{
    /// <summary>
    /// Gets or sets the minutes.
    /// </summary>
    public int Minutes { get; set; }

    /// <summary>
    /// Gets or sets the seconds.
    /// </summary>
    public int Seconds { get; set; }

    /// <summary>
    /// Gets or sets the frames. One frame is 1/75 of a second.
    /// </summary>
    public int Frames { get; set; }

    public int ToSectors()
    {
        return (((this.Minutes * 60) + this.Seconds) * 75) + this.Frames;
    }

    public override string ToString()
    {
        return $"{this.Minutes:D2}:{this.Seconds:D2}:{this.Frames:D2}";
    }
}
