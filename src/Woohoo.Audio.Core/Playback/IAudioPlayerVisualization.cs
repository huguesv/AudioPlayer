// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Playback;

public interface IAudioPlayerVisualization
{
    double[] PowerSpectrumDensity { get; }

    double[] PowerSpectrumDensityBands { get; }

    double[] Waveform { get; }

    void CopyTo(double[] plotPsd, double[] plotBands, double[] plotWave);
}
