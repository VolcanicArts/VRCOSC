// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Audio;

public class CaptureDeviceWrapper
{
    public WasapiCapture? AudioCapture { get; private set; }

    public Action<byte[], int>? OnNewData;

    public void Initialise(MMDevice? device)
    {
        if (device is null) return;

        AudioCapture = new WasapiCapture(device);
        AudioCapture.WaveFormat = new WaveFormat(AudioCapture.WaveFormat.SampleRate, 16, 1);

        AudioCapture.DataAvailable += (_, e) =>
        {
            var copy = e.Buffer.NewCopy(e.BytesRecorded);
            OnNewData?.Invoke(copy, e.BytesRecorded);
        };

        AudioCapture.ShareMode = AudioClientShareMode.Shared;
        AudioCapture.StartRecording();
    }

    public void ChangeDevice(MMDevice? newCaptureDevice)
    {
        AudioCapture?.Dispose();
        AudioCapture = null;

        if (newCaptureDevice is not null)
            Initialise(newCaptureDevice);
    }

    public void Teardown()
    {
        AudioCapture?.Dispose();
        AudioCapture = null;
    }
}
