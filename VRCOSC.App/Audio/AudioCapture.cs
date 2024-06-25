// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace VRCOSC.App.Audio;

internal class AudioCapture
{
    private readonly WasapiCapture capture;
    private readonly MemoryStream buffer;
    private readonly object lockObject = new();

    public bool IsCapturing => capture.CaptureState == CaptureState.Capturing;

    public AudioCapture(MMDevice device)
    {
        capture = new WasapiCapture(device);
        capture.WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(16000, 1);
        buffer = new MemoryStream();

        capture.DataAvailable += OnDataAvailable;
        capture.RecordingStopped += OnRecordingStopped;
    }

    public void StartCapture()
    {
        capture.StartRecording();
    }

    public void StopCapture()
    {
        capture.StopRecording();
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        lock (lockObject)
        {
            buffer.Write(e.Buffer, 0, e.BytesRecorded);
        }
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        capture.Dispose();
    }

    public float[] GetBufferedData()
    {
        lock (lockObject)
        {
            var byteArray = buffer.ToArray();
            var floatArray = new float[byteArray.Length / sizeof(float)];
            Buffer.BlockCopy(byteArray, 0, floatArray, 0, byteArray.Length);
            return floatArray;
        }
    }

    public void ClearBuffer()
    {
        lock (lockObject)
        {
            buffer.SetLength(0);
            buffer.Position = 0;
        }
    }
}
