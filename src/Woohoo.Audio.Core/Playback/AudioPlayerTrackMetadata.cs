// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Playback;

public sealed record class AudioPlayerTrackMetadata
{
    public string TrackTitle { get; init; } = string.Empty;

    public string TrackPerformer { get; init; } = string.Empty;

    public string AlbumTitle { get; init; } = string.Empty;

    public string AlbumPerformer { get; init; } = string.Empty;
}
