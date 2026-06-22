// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Services;

using System.Text.Json.Serialization;

public interface ILocalSettingsService
{
    event EventHandler<SettingChangedEventArgs>? SettingChanged;

    public string FilePath { get; }

    T? ReadSetting<T>(string key);

    void SaveSetting<T>(string key, T value);

    void RegisterType(Type type, JsonSerializerContext serializerContext);
}
