// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Metadata;

public sealed record class TrackMetadata
{
    public required string Name { get; init; }

    public required string Artist { get; init; }
}
