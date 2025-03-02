// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Playback;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using Woohoo.Sdl3;

public class SdlAudioPlayer
{
    public const int Channels = 2;
    public const int Frequency = 44100;
    public const int FormatSizeInBytes = 2;

    private readonly Lock lockObject = new();
    private readonly byte[] buffer;
    private readonly Action<byte[], int, int, bool> playedCallback;

    private SdlAudioStream? audioDeviceStream;

    private bool initialized;
    private Stream? fileStream;
    private byte[] fileData = [];
    private int fileDataIndex;
    private int fileDataLength;
    private int pendingVolume;

    public SdlAudioPlayer(Action<byte[], int, int, bool> playedCallback)
    {
        this.buffer = new byte[4096];
        this.playedCallback = playedCallback;
        this.pendingVolume = 100;
        this.Visualization = new();
    }

    public VisualizationData Visualization { get; private set; }

    public bool IsPlaying { get; private set; }

    public int Volume
    {
        get
        {
            if (this.initialized)
            {
                this.VerifyDeviceNotNull();
                return Math.Max(0, Math.Min(100, (int)((this.audioDeviceStream?.Gain ?? 1) * 100)));
            }
            else
            {
                return this.pendingVolume;
            }
        }

        set
        {
            if (this.initialized)
            {
                this.VerifyDeviceNotNull();
                this.audioDeviceStream.Gain = value / 100.0f;
            }
            else
            {
                this.pendingVolume = value;
            }
        }
    }

    public void Play(byte[] fileData)
    {
        this.IsPlaying = false;

        this.Initialize();

        this.fileStream = null;
        this.fileData = fileData;
        this.fileDataIndex = 0;
        this.fileDataLength = fileData.Length;

        this.Resume();
    }

    public void Play(Stream fileStream, int length)
    {
        this.IsPlaying = false;

        this.Initialize();

        this.fileStream = fileStream;
        this.fileData = [];
        this.fileDataIndex = 0;
        this.fileDataLength = length;

        this.Resume();
    }

    public void Pause()
    {
        this.VerifyDeviceNotNull();

        this.audioDeviceStream.Pause();

        this.IsPlaying = false;
    }

    public void Resume()
    {
        this.VerifyDeviceNotNull();

        this.audioDeviceStream.Resume();

        this.IsPlaying = true;
    }

    public void SkipBack(TimeSpan timeSpan)
    {
        this.VerifyDeviceNotNull();

        lock (this.lockObject)
        {
            int skipTime = (int)(timeSpan.TotalSeconds * Frequency * Channels * FormatSizeInBytes);
            this.fileDataIndex = Math.Max(0, this.fileDataIndex - skipTime);
            this.fileStream?.Seek(this.fileDataIndex, SeekOrigin.Begin);
        }
    }

    public void SkipForward(TimeSpan timeSpan)
    {
        this.VerifyDeviceNotNull();

        lock (this.lockObject)
        {
            int skipTime = (int)(timeSpan.TotalSeconds * Frequency * Channels * FormatSizeInBytes);
            this.fileDataIndex = Math.Min(this.fileDataLength, this.fileDataIndex + skipTime);
            this.fileStream?.Seek(this.fileDataIndex, SeekOrigin.Begin);
        }
    }

    private void Initialize()
    {
        if (this.initialized)
        {
            return;
        }

        SdlAudio.Initialize();

        this.audioDeviceStream = SdlAudio.DefaultDevices.Playback.OpenStream(SdlAudioFormat.SDL_AUDIO_S16LE, Channels, Frequency, this.AudioRequested);
        this.initialized = true;
        this.Volume = this.pendingVolume;
    }

    private void AudioRequested(SdlAudioStream device, int additionalAmount, int totalAmount)
    {
        lock (this.lockObject)
        {
            int total = Math.Min(additionalAmount, this.fileDataLength - this.fileDataIndex);
            total = (total / 2) * 2;
            if (total == 0)
            {
                this.playedCallback([], 0, this.fileDataIndex, true);
                return;
            }

            if (this.fileStream is not null)
            {
                this.fileStream.ReadExactly(this.buffer, 0, total);
            }
            else
            {
                Array.Copy(this.fileData, this.fileDataIndex, this.buffer, 0, total);
            }

            this.fileDataIndex += total;

            this.Visualization.AnalyzeBuffer(this.buffer, total);
            this.playedCallback(this.buffer, total, this.fileDataIndex, false);

            device.PutStreamData(this.buffer, total);
        }
    }

    [MemberNotNull(nameof(audioDeviceStream))]
    private void VerifyDeviceNotNull()
    {
        if (this.audioDeviceStream is null)
        {
            throw new InvalidOperationException("Stream device not set.");
        }
    }
}
