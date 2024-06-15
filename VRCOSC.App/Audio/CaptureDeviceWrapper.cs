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

    public void Initialise(MMDevice device)
    {
        AudioCapture = new WasapiCapture(device);
        AudioCapture.WaveFormat = new WaveFormat(AudioCapture.WaveFormat.SampleRate, 16, 1);

        AudioCapture.DataAvailable += (_, e) =>
        {
            var copy = e.Buffer.NewCopy(e.BytesRecorded);
            OnNewData?.Invoke(copy, e.BytesRecorded);
        };

        AudioCapture.StartRecording();
    }

    public void ChangeDevice(MMDevice newCaptureDevice)
    {
        AudioCapture?.Dispose();
        Initialise(newCaptureDevice);
    }

    public void Teardown()
    {
        if (AudioCapture is null) return;

        AudioCapture.RecordingStopped += (_, _) => AudioCapture?.Dispose();
        AudioCapture.StopRecording();
    }
}
