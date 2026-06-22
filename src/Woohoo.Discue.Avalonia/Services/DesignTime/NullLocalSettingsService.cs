// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Avalonia.Services.DesignTime;

using System;
using System.Text.Json.Serialization;
using Woohoo.Audio.Services;

internal class NullLocalSettingsService : ILocalSettingsService
{
#pragma warning disable CS0067 // The event is never used
    public event EventHandler<SettingChangedEventArgs>? SettingChanged;
#pragma warning restore CS0067

    public string FilePath => throw new NotImplementedException();

    public T? ReadSetting<T>(string key)
    {
        switch (key)
        {
            case nameof(KnownSettingKeys.AudioEngine):
                return (T?)(object)"Null Media Player";
            case nameof(KnownSettingKeys.QueryMetadataOnline):
                return (T?)(object)true;
            case nameof(KnownSettingKeys.QueryLyricsOnline):
                return (T?)(object)true;
            case nameof(KnownSettingKeys.QueryLyricsOffline):
                return (T?)(object)false;
        }

        return (T?)(object?)null;
    }

    public void SaveSetting<T>(string key, T value)
    {
    }

    public void RegisterType(Type type, JsonSerializerContext serializerContext)
    {
    }
}
