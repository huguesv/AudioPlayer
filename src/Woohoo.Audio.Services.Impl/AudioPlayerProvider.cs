// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Services.Impl;

using System;
using Woohoo.Audio.Core.Playback;

public sealed class AudioPlayerProvider : IAudioPlayerProvider
{
    private readonly Dictionary<string, Func<IAudioPlayer>> factoryMap;
    private readonly Lazy<IAudioPlayer> audioPlayer;

    public AudioPlayerProvider(
        ILocalSettingsService localSettingsService,
        string defaultEngine,
        Dictionary<string, Func<IAudioPlayer>> factoryMap)
    {
        ArgumentNullException.ThrowIfNull(localSettingsService);
        ArgumentNullException.ThrowIfNull(defaultEngine);
        ArgumentNullException.ThrowIfNull(factoryMap);

        this.factoryMap = factoryMap;
        this.audioPlayer = new Lazy<IAudioPlayer>(() =>
        {
            var engine = localSettingsService.ReadSetting<string>(KnownSettingKeys.AudioEngine) ?? defaultEngine;
            if (this.factoryMap.TryGetValue(engine, out var factory))
            {
                return factory();
            }

            throw new NotSupportedException($"Unsupported audio engine: {engine}");
        });
    }

    public IAudioPlayer GetAudioPlayer()
    {
        return this.audioPlayer.Value;
    }
}
