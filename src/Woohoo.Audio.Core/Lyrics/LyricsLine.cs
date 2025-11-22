// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Lyrics;

using System;

public sealed record class LyricsLine
{
    public required TimeSpan Timestamp { get; init; }

    public required string Text { get; init; }
}
