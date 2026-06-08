// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Playback;

public sealed class AudioPlayerTrackEventArgs : EventArgs
{
    public AudioPlayerTrackEventArgs(AudioPlayerTrack track)
    {
        this.Track = track;
    }

    public AudioPlayerTrack Track { get; }
}
