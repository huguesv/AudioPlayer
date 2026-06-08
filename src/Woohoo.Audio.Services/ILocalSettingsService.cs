// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Services;

public interface ILocalSettingsService
{
    event EventHandler<SettingChangedEventArgs>? SettingChanged;

    public string FilePath { get; }

    T? ReadSetting<T>(string key);

    void SaveSetting<T>(string key, T value);
}
