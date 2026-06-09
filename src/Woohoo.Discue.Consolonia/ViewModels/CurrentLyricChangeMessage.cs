// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Consolonia.ViewModels;

public sealed class CurrentLyricChangeMessage
{
    public required LyricsLineViewModel Line { get; init; }

    public required int Index { get; init; }

    public required bool AutoScroll { get; init; }

    public required int LineCount { get; init; }
}
