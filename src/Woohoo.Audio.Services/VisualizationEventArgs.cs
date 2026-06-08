// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Services;

using System;
using Woohoo.Audio.Core.Playback;

public sealed class VisualizationEventArgs : EventArgs
{
    public VisualizationEventArgs(IAudioPlayerVisualization visualization)
    {
        this.Visualization = visualization;
    }

    public IAudioPlayerVisualization Visualization { get; }
}
