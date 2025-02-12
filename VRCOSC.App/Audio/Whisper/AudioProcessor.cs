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
    private WhisperProcessor? whisper;

    private const int default_samples_to_check = 24000; // sample rate is 16000 so check the last 1.5 seconds of audio

    private SpeechResult? speechResult;
    private bool isProcessing;

    public AudioProcessor(MMDevice device)
    {
        try
        {
            audioCapture = new AudioCapture(device);
        }
        catch (Exception e)
        {
            audioCapture = null;
            ExceptionHandler.Handle(e);
        }
    }

    private void buildWhisperProcessor()
    {
        var modelFilePath = SettingsManager.GetInstance().GetValue<string>(VRCOSCSetting.SpeechModelPath);

        try
        {
            var builder = WhisperFactory.FromPath(modelFilePath).CreateBuilder();

            builder = builder.WithProbabilities()
                             .WithThreads(8)
                             .WithNoContext()
                             .WithSingleSegment()
                             .WithMaxSegmentLength(int.MaxValue)
                             .WithLanguageDetection();

            if (SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.SpeechTranslate))
            {
                builder.WithLanguage("en");
            }

            // translation for any-to-any technically works but is unsupported
            // for future reference, to get any-to-any to work you have to do:
            // WithLanguageDetection
            // WithLanguage(targetLanguage)

            whisper = builder.Build();
        }
        catch (Exception e)
        {
            whisper = null;
            ExceptionHandler.Handle(e, "The Whisper model path is empty or incorrect. Please go into the app's speech settings and restore the model by clicking 'Auto Install Model'");
        }
    }

    public void Start()
    {
        speechResult = null;
        isProcessing = false;

        buildWhisperProcessor();

        if (whisper is null || audioCapture is null) return;

        audioCapture.ClearBuffer();
        audioCapture.StartCapture();
    }

    public async Task Stop()
    {
        audioCapture?.StopCapture();

        if (whisper is not null)
        {
            await whisper.DisposeAsync();
            whisper = null;
        }
    }

    public async Task<SpeechResult?> GetResultAsync()
    {
        if (isProcessing) return null;
        if (audioCapture is null || !audioCapture.IsCapturing) return null;

        var data = audioCapture.GetBufferedData();
        if (data.Length == 0) return null;

        // adjust volume as some microphones output quiet streams
        adjustVolume(data, SettingsManager.GetInstance().GetValue<float>(VRCOSCSetting.SpeechMicVolumeAdjustment));

        if (isSilent(data))
        {
            SpeechResult? finalSpeechResult = null;

            if (speechResult is not null)
            {
                isProcessing = true;
                finalSpeechResult = await processWithWhisper(data, true);
                isProcessing = false;
            }

            if (finalSpeechResult is not null)
            {
                Logger.Log($"Final result: {finalSpeechResult.Text} - {finalSpeechResult.Confidence}", LoggingTarget.Information);
            }

            speechResult = null;
            audioCapture.ClearBuffer();

            return finalSpeechResult;
        }

        isProcessing = true;
        speechResult = await processWithWhisper(data, false);
        isProcessing = false;

        Logger.Log($"Result: {speechResult?.Text}", LoggingTarget.Information);

        return speechResult;
    }

    public void ClearBuffer() => audioCapture?.ClearBuffer();

    private void adjustVolume(float[] samples, float multiplier)
    {
        // multiplier is 0 to 3. Increase it to be 0 to 30 to get more range
        multiplier *= 10;

        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] *= multiplier;

            samples[i] = samples[i] switch
            {
                > 1.0f => 1.0f,
                < -1.0f => -1.0f,
                _ => samples[i]
            };
        }
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

        var rms = Math.Sqrt(sum / samplesToCheck);
        Logger.Log($"RMS: {rms}", LoggingTarget.Information);
        return rms < SettingsManager.GetInstance().GetValue<float>(VRCOSCSetting.SpeechNoiseCutoff);
    }

    private async Task<SpeechResult?> processWithWhisper(float[] data, bool final)
    {
        try
        {
            if (whisper is null) return null;

            var segmentData = new List<SegmentData>();

            await foreach (var result in whisper.ProcessAsync(data))
            {
                segmentData.Add(result);
            }

            if (segmentData.Count == 0) return null;

            var segment = segmentData.Last();
            var text = segment.Text.Trim();
            var confidence = segment.Probability;
            return new SpeechResult(final, text, confidence);
        }
        catch (Exception e)
        {
            Logger.Error(e, "Exception when attempting to process with Whisper");
            return null;
        }
    }
}