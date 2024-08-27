// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using VRCOSC.App.Settings;
using VRCOSC.App.Utils;
using Whisper.net;

namespace VRCOSC.App.Audio.Whisper;

internal class AudioProcessor
{
    private readonly AudioCapture? audioCapture;
    private readonly WhisperProcessor? whisper;

    private const int default_samples_to_check = 24000; // sample rate is 16000 so check the last 1.5 seconds of audio
    private const float silence_threshold = 0.04f;

    private SpeechResult? speechResult;

    public AudioProcessor(MMDevice device)
    {
        var modelFilePath = SettingsManager.GetInstance().GetValue<string>(VRCOSCSetting.Whisper_ModelPath);

        try
        {
            var builder = WhisperFactory.FromPath(modelFilePath);

            whisper = builder.CreateBuilder()
                             .WithProbabilities()
                             .WithThreads(8)
                             .WithNoContext()
                             .WithSingleSegment()
                             .WithMaxSegmentLength(int.MaxValue)
                             .Build();
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, "Please make sure the model path for Whisper is correct");
        }

        try
        {
            audioCapture = new AudioCapture(device);
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e);
        }
    }

    public void Start()
    {
        speechResult = null;

        audioCapture?.ClearBuffer();
        audioCapture?.StartCapture();
    }

    public void Stop()
    {
        audioCapture?.StopCapture();
    }

    public async Task<SpeechResult?> GetResultAsync()
    {
        if (audioCapture is null || !audioCapture.IsCapturing) return null;

        var data = audioCapture.GetBufferedData();
        if (data.Length == 0) return null;

        if (isSilent(data))
        {
            SpeechResult? finalSpeechResult = null;

            if (speechResult is not null)
            {
                finalSpeechResult = new SpeechResult(speechResult);
            }

            speechResult = null;
            audioCapture.ClearBuffer();

#if DEBUG
            if (finalSpeechResult is not null)
            {
                Logger.Log("Final result: " + finalSpeechResult.Text + " - " + finalSpeechResult.Confidence);
            }
#endif

            return finalSpeechResult;
        }

        speechResult = await processWithWhisper(data, false);

#if DEBUG
        Logger.Log("Result: " + speechResult?.Text);
#endif

        return speechResult;
    }

    private bool isSilent(float[] buffer)
    {
        var samplesToCheck = buffer.Length < default_samples_to_check ? buffer.Length : default_samples_to_check;

        var sum = 0d;

        for (int i = buffer.Length - samplesToCheck; i < buffer.Length; i++)
        {
            var sample = buffer[i];
            sum += sample * sample;
        }

        var rms = Math.Sqrt(sum / samplesToCheck) * 100d;
#if DEBUG
        Logger.Log($"RMS: {rms}");
#endif
        return rms < silence_threshold;
    }

    private async Task<SpeechResult?> processWithWhisper(float[] data, bool final)
    {
        var segmentData = new List<SegmentData>();

        await foreach (var result in whisper!.ProcessAsync(data))
        {
            segmentData.Add(result);
        }

        if (segmentData.Count == 0) return null;

        var segment = segmentData.Last();
        var text = segment.Text.Trim();
        var confidence = segment.Probability;
        return new SpeechResult(final, text, confidence);
    }
}
