// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Audio;

internal class AudioCapture
{
    private readonly WasapiCapture capture;
    private readonly MemoryStream buffer;
    private readonly object lockObject = new();

    public bool IsCapturing => capture.CaptureState == CaptureState.Capturing;

    public AudioCapture(MMDevice device)
    {
        capture = new WasapiCapture(device)
        {
            ShareMode = AudioClientShareMode.Shared
        };

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
            try
            {
                var targetFormat = WaveFormat.CreateIeeeFloatWaveFormat(16000, 1);
                var inputFormat = capture.WaveFormat;

                var bufferArray = buffer.ToArray();
                var bytesRecorded = bufferArray.Length;

                using var memoryStream = new MemoryStream(bufferArray, 0, bytesRecorded);
                using var waveStream = new RawSourceWaveStream(memoryStream, inputFormat);
                using var resampler = new MediaFoundationResampler(waveStream, targetFormat);
                resampler.ResamplerQuality = 60;

                var maxBytesNeeded = (int)(buffer.Length * (targetFormat.SampleRate / (float)inputFormat.SampleRate) * (targetFormat.BitsPerSample / (float)inputFormat.BitsPerSample) * (targetFormat.Channels / (float)inputFormat.Channels));
                var resampledBuffer = new byte[maxBytesNeeded];
                var bytesRead = resampler.Read(resampledBuffer, 0, maxBytesNeeded);

                Array.Resize(ref resampledBuffer, bytesRead);

                var floatArray = new float[resampledBuffer.Length / sizeof(float)];
                Buffer.BlockCopy(resampledBuffer, 0, floatArray, 0, resampledBuffer.Length);
                return floatArray;
            }
            catch (Exception e)
            {
                Logger.Error(e, "The selected microphone has provided bad data");
                return Array.Empty<float>();
            }
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

    public void SaveBufferToFile(string filePath)
    {
        lock (lockObject)
        {
            buffer.Position = 0;
            using var waveFileWriter = new WaveFileWriter(filePath, capture.WaveFormat);
            buffer.CopyTo(waveFileWriter);
        }
    }

    public void SaveConvertedToFile(float[] data, string filePath)
    {
        var byteArray = new byte[data.Length * sizeof(float)];
        Buffer.BlockCopy(data, 0, byteArray, 0, byteArray.Length);

        var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(16000, 1);
        using var waveFileWriter = new WaveFileWriter(filePath, waveFormat);
        waveFileWriter.Write(byteArray, 0, byteArray.Length);
    }
}