// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Services;

public static class KnownSettingKeys
{
    // Global settings
    public static readonly string QueryMetadataOnline = ToCamelCase(nameof(QueryMetadataOnline));
    public static readonly string QueryLyricsOnline = ToCamelCase(nameof(QueryLyricsOnline));
    public static readonly string QueryLyricsOffline = ToCamelCase(nameof(QueryLyricsOffline));
    public static readonly string QueryLyricsOfflineDatabasePath = ToCamelCase(nameof(QueryLyricsOfflineDatabasePath));
    public static readonly string LyricsAutoScroll = ToCamelCase(nameof(LyricsAutoScroll));
    public static readonly string AudioEngine = ToCamelCase(nameof(AudioEngine));
    public static readonly string Theme = ToCamelCase(nameof(Theme));
    public static readonly string LastBrowseFolder = ToCamelCase(nameof(LastBrowseFolder));

    private static string ToCamelCase(string text) => char.ToLowerInvariant(text[0]) + text[1..];
}
