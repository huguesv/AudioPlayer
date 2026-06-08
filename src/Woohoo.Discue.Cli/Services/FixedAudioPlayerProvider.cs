// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Cli.Services;

using Woohoo.Audio.Core.Playback;
using Woohoo.Audio.Playback.Sdl3;
using Woohoo.Audio.Services;

internal sealed class FixedAudioPlayerProvider : IAudioPlayerProvider
{
    public IAudioPlayer GetAudioPlayer()
    {
        return new Sdl3AudioPlayer();
    }
}
