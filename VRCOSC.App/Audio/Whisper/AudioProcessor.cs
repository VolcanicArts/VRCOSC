// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using Whisper.net;

namespace VRCOSC.App.Audio.Whisper;

internal class AudioProcessor
{
    private readonly AudioCapture audioCapture;
    private readonly WhisperProcessor whisper;

    public bool IsProcessing => audioCapture.IsCapturing;

    public AudioProcessor(MMDevice device)
    {
        // TODO: Replace with runtime/whisper/model.bin file and give the user the option to select a model to download
        var modelFilePath = @"";

        var builder = WhisperFactory.FromPath(modelFilePath);

        whisper = builder.CreateBuilder()
                         .WithProbabilities()
                         .WithNoContext()
                         .Build();

        audioCapture = new AudioCapture(device);
    }

    public void Start()
    {
        audioCapture.ClearBuffer();
        audioCapture.StartCapture();
    }

    public void Stop()
    {
        audioCapture.StopCapture();
    }

    public void SaveAudioBufferToFile(string filePath)
    {
        audioCapture.SaveBufferToFile(filePath);
    }

    public async Task<SpeechResult> GetResult()
    {
        var data = audioCapture.GetBufferedData();
        return data.Length > 0 ? await processWithWhisper(data) : new SpeechResult(string.Empty, 0f);
    }

    private async Task<SpeechResult> processWithWhisper(float[] data)
    {
        var segmentData = new List<SegmentData>();

        await foreach (var result in whisper.ProcessAsync(data))
        {
            segmentData.Add(result);
        }

        if (segmentData.Count == 0) return new SpeechResult(string.Empty, 0f);

        var segment = segmentData.Last();
        var text = segment.Text.Trim();
        var confidence = segment.Probability;
        return new SpeechResult(text, confidence);
    }

    public void ClearBuffer()
    {
        audioCapture.ClearBuffer();
    }
}
