// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Metadata;

using System.Collections.Immutable;

public sealed record class AlbumMetadata
{
    public required string Album { get; init; }

    public required string Artist { get; init; }

    public required ImmutableArray<TrackMetadata> Tracks { get; init; }

    public required ImmutableArray<ArtMetadata> Images { get; init; }
}
