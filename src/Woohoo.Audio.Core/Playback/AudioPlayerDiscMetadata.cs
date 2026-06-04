// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Playback;

using System.Collections.Immutable;
using Woohoo.Audio.Core.Metadata;

public sealed record class AudioPlayerDiscMetadata
{
    public string AlbumTitle { get; init; } = string.Empty;

    public string AlbumPerformer { get; init; } = string.Empty;

    public string Year { get; init; } = string.Empty;

    public ImmutableArray<ArtMetadata> AlbumArtImages { get; init; } = [];

    public string? GetPrimaryArtUrl()
    {
        return this.AlbumArtImages.FirstOrDefault(md => md.IsPrimary)?.Url;
    }
}
