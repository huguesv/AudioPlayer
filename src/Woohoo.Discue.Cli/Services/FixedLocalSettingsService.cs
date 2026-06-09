// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Cli.Services;

using System;
using Woohoo.Audio.Services;

internal sealed class FixedLocalSettingsService : ILocalSettingsService
{
#pragma warning disable CS0067 // The event is never used
    public event EventHandler<SettingChangedEventArgs>? SettingChanged;
#pragma warning restore CS0067

    public string FilePath => throw new NotImplementedException();

    public bool QueryMetadataOnline { get; init; }

    public bool QueryLyricsOnline { get; init; }

    public bool QueryLyricsOffline { get; init; }

    public T? ReadSetting<T>(string key)
    {
        if (key == KnownSettingKeys.QueryMetadataOnline)
        {
            return (T?)(object)this.QueryMetadataOnline;
        }
        else if (key == KnownSettingKeys.QueryLyricsOnline)
        {
            return (T?)(object)this.QueryLyricsOnline;
        }
        else if (key == KnownSettingKeys.QueryLyricsOffline)
        {
            return (T?)(object)this.QueryLyricsOffline;
        }

        return (T?)(object?)null;
    }

    public void SaveSetting<T>(string key, T value)
    {
    }
}
