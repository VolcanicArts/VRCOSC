// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace VRCOSC.Game.Providers.SpeechToText;

public class CaptureDeviceWrapper
{
    public MMDevice? CurrentCaptureDevice { get; private set; }
    public WasapiCapture? AudioCapture { get; private set; }

    public Action<byte[], int>? OnNewData;
    public Action<string>? OnLog;

    public void Initialise(MMDevice? device = null)
    {
        device ??= WasapiCapture.GetDefaultCaptureDevice();

        AudioCapture = new WasapiCapture(device);
        AudioCapture.WaveFormat = new WaveFormat(AudioCapture.WaveFormat.SampleRate, 16, 1);

        AudioCapture.DataAvailable += (_, e) =>
        {
            var copy = e.Buffer.NewCopy(e.BytesRecorded);
            OnNewData?.Invoke(copy, e.BytesRecorded);
        };

        AudioCapture.StartRecording();
        CurrentCaptureDevice = device;
    }

    public async void Update()
    {
        var newCaptureDevice = WasapiCapture.GetDefaultCaptureDevice();
        if (CurrentCaptureDevice?.ID == newCaptureDevice.ID) return;

        OnLog?.Invoke($"Microphone changed to {newCaptureDevice.DeviceFriendlyName.Trim()}");

        // if we have no audio capture at all, initialise the new capture device
        if (AudioCapture is null)
        {
            Initialise(newCaptureDevice);
            return;
        }

        switch (AudioCapture.CaptureState)
        {
            // if it's starting or capturing, wait for the capture to stop
            case CaptureState.Capturing or CaptureState.Starting:
                AudioCapture.RecordingStopped += (_, _) =>
                {
                    AudioCapture?.Dispose();
                    Initialise(newCaptureDevice);
                };
                AudioCapture.StopRecording();
                break;

            // if the current capture is stopped, hook immediately
            case CaptureState.Stopped:
                AudioCapture?.Dispose();
                Initialise(newCaptureDevice);
                break;

            // if it's already stopping, wait and then hook
            case CaptureState.Stopping:
                while (AudioCapture.CaptureState != CaptureState.Stopped) await Task.Delay(1);
                AudioCapture?.Dispose();
                Initialise(newCaptureDevice);
                break;

            default:
                throw new InvalidOperationException($"Unknown capture state {AudioCapture.CaptureState}");
        }
    }

    public void Teardown()
    {
        if (AudioCapture is null) return;

        AudioCapture.RecordingStopped += (_, _) => AudioCapture?.Dispose();
        AudioCapture.StopRecording();
    }
}
