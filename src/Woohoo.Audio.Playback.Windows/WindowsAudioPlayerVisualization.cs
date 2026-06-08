// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Playback.Windows;

using Woohoo.Audio.Core.Playback;
using Woohoo.Audio.Playback.Windows.Internal.Wasapi;

internal sealed class WindowsAudioPlayerVisualization : IAudioPlayerVisualization
{
    private readonly AudioCapture capture;

    public WindowsAudioPlayerVisualization(AudioCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);

        this.capture = capture;
    }

    public double[] PowerSpectrumDensity => [];

    public double[] PowerSpectrumDensityBands => [];

    public double[] Waveform
    {
        get
        {
            var result = this.capture.GetRecentSamples();
            return result.Length > 512 ? [.. result.Take(512)] : result;
        }
    }

    public void CopyTo(double[] plotPsd, double[] plotBands, double[] plotWave)
    {
    }
}
