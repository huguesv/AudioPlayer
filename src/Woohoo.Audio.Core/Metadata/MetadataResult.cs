// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Metadata;

using System.Collections.Immutable;

public sealed record class MetadataResult
{
    public required AlbumMetadata Album { get; init; }

    public required ImmutableArray<TrackMetadata> Tracks { get; init; }
}
