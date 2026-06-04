// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Services;

public sealed class SettingChangedEventArgs(string settingKey) : EventArgs
{
    public string SettingKey { get; } = settingKey;
}
