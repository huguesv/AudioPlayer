// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Playback;

using System;

public sealed record class AudioPlayerTrack
{
    public required Guid Id { get; init; }

    public required Guid DiscId { get; init; }

    public int TrackNumber { get; init; }

    public bool IsAudio { get; init; }

    public int TrackSize { get; init; }

    public bool TrackFileNotFound { get; init; }

    public TimeSpan Duration { get; init; } = TimeSpan.Zero;
}
