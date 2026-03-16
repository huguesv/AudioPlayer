// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Media;

using System.Collections.Immutable;

public interface IAlbumMedia
{
    string CTDBToc { get; }

    ImmutableArray<IAlbumTrack> Tracks { get; }
}
