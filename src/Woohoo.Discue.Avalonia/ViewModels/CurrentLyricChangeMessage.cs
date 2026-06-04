// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.ViewModels;

public sealed class CurrentLyricChangeMessage
{
    public required LyricsLineViewModel Line { get; init; }

    public required int Index { get; init; }

    public required bool AutoScroll { get; init; }
}
