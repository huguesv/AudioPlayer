// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Lyrics;

public sealed class LyricsProviderOptions
{
    public bool UseDatabase { get; init; }

    public bool UseWeb { get; init; }

    public bool UseWebExternalSources { get; init; }

    public string? DatabaseFilePath { get; init; }
}
