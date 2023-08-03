// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace VRCOSC.Game.Providers.SpeechToText;

public class MicrophoneHook
{
    private MMDevice? currentCaptureDevice;

    public WasapiCapture? AudioCapture;

    public Action<byte[], int>? BufferCallback;
    public Action<string>? OnLog;

    public MMDevice? Hook()
    {
        currentCaptureDevice = WasapiCapture.GetDefaultCaptureDevice();
        AudioCapture = new WasapiCapture(currentCaptureDevice);
        AudioCapture.WaveFormat = new WaveFormat(AudioCapture.WaveFormat.SampleRate, 16, 1);
        AudioCapture.DataAvailable += handleAudioCaptureBuffer;
        AudioCapture.StartRecording();
        return currentCaptureDevice;
    }

    public void Update()
    {
        var defaultCaptureDevice = WasapiCapture.GetDefaultCaptureDevice();

        if (currentCaptureDevice?.ID != defaultCaptureDevice.ID)
        {
            UnHook();
            var newMic = Hook();
            OnLog?.Invoke($"Default mic updated. New mic: {newMic}");
        }
    }

    public void UnHook()
    {
        AudioCapture?.StopRecording();
        AudioCapture = null;
    }

    private void handleAudioCaptureBuffer(object? sender, WaveInEventArgs e)
    {
        var copy = e.Buffer.NewCopy(e.BytesRecorded);
        BufferCallback?.Invoke(copy, e.BytesRecorded);
    }
}
