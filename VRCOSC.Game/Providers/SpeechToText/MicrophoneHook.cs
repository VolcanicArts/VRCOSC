// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace VRCOSC.Game.Providers.SpeechToText;

public class MicrophoneHook
{
    public WasapiCapture? AudioCapture;

    public Action<byte[], int>? BufferCallback;

    public MMDevice? Hook()
    {
        try
        {
            var defaultCaptureDevice = WasapiCapture.GetDefaultCaptureDevice();
            AudioCapture = new WasapiCapture(defaultCaptureDevice);
            AudioCapture.WaveFormat = new WaveFormat(AudioCapture.WaveFormat.SampleRate, 16, 1);
            AudioCapture.DataAvailable += handleAudioCaptureBuffer;
            AudioCapture.StartRecording();
            return defaultCaptureDevice;
        }
        catch (Exception)
        {
            return null;
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
