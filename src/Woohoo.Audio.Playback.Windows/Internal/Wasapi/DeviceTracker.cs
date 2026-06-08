// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Playback.Windows.Internal.Wasapi;

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using global::Windows.Win32.Foundation;
using global::Windows.Win32.Media.Audio;
using winmdroot = global::Windows.Win32;

[SupportedOSPlatform("windows6.0.6000")]
internal sealed class DeviceTracker : IMMNotificationClient
{
    private IMMDeviceEnumerator? deviceEnumerator;
    private AudioCapture capture;

    public DeviceTracker(AudioCapture captureInstance)
    {
        this.capture = captureInstance ?? throw new ArgumentNullException(nameof(captureInstance));
    }

    public void Start()
    {
        this.deviceEnumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
        this.deviceEnumerator.RegisterEndpointNotificationCallback(this);

        this.deviceEnumerator.GetDefaultAudioEndpoint(winmdroot.Media.Audio.EDataFlow.eRender, winmdroot.Media.Audio.ERole.eConsole, out IMMDevice device);

        this.capture.Initialize(device!);
        this.capture.Start();
    }

    public void Stop(bool final = false)
    {
        this.capture.Stop();

        if (final && this.deviceEnumerator != null)
        {
            this.deviceEnumerator.UnregisterEndpointNotificationCallback(this);
            Marshal.ReleaseComObject(this.deviceEnumerator);
            this.deviceEnumerator = null;
        }
    }


    void IMMNotificationClient.OnDeviceStateChanged(PCWSTR pwstrDeviceId, DEVICE_STATE dwNewState)
    {
    }

    void IMMNotificationClient.OnDeviceAdded(PCWSTR pwstrDeviceId)
    {
    }

    void IMMNotificationClient.OnDeviceRemoved(PCWSTR pwstrDeviceId)
    {
    }

    void IMMNotificationClient.OnDefaultDeviceChanged(EDataFlow flow, ERole role, [Optional] PCWSTR pwstrDefaultDeviceId)
    {
        if (flow == winmdroot.Media.Audio.EDataFlow.eRender && role == winmdroot.Media.Audio.ERole.eConsole)
        {
            this.capture.Stop();
            this.deviceEnumerator!.GetDefaultAudioEndpoint(winmdroot.Media.Audio.EDataFlow.eRender, winmdroot.Media.Audio.ERole.eConsole, out IMMDevice device);
            this.capture.Initialize(device!);
            this.capture.Start();
        }
    }

    void IMMNotificationClient.OnPropertyValueChanged(PCWSTR pwstrDeviceId, PROPERTYKEY key)
    {
    }
}
