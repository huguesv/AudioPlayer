// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Lyrics;

using System;

public interface ILyricsProvider
{
    Task<LyricsTrack?> QueryAsync(string albumTitle, string artistName, string trackTitle, TimeSpan duration, CancellationToken cancellationToken);
}
