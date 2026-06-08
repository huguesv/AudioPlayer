// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Services;

using Woohoo.Audio.Core.Playback;

public interface IAudioPlayerProvider
{
    IAudioPlayer GetAudioPlayer();
}
