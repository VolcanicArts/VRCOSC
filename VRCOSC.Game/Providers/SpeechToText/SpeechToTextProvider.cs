// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Vosk;

namespace VRCOSC.Game.Providers.SpeechToText;

public class SpeechToTextProvider
{
    public Action? OnBeforeAnalysis;
    public Action<bool, string>? OnFinalResult;
    public Action<string>? OnPartialResult;
    public Action<string>? OnLog;

    private readonly object analyseLock = new();
    private CaptureDeviceWrapper captureDeviceWrapper = null!;
    private Model? model;
    private VoskRecognizer? recogniser;
    private bool readyToAccept;

    private string modelDirectoryPath = null!;

    public bool AnalysisEnabled { get; set; } = true;
    public float RequiredConfidence { get; set; }

    public SpeechToTextProvider()
    {
        Vosk.Vosk.SetLogLevel(-1);
    }

    public void Initialise(string modelDirectoryPath)
    {
        if (!Directory.Exists(modelDirectoryPath))
        {
            OnLog?.Invoke("Model directory not found");
            return;
        }

        this.modelDirectoryPath = modelDirectoryPath;

        Task.Run(() =>
        {
            lock (analyseLock)
            {
                initialiseMicrophoneCapture();
                initialiseVosk();

                readyToAccept = true;
            }
        });
    }

    public void Update()
    {
        captureDeviceWrapper.Update();
    }

    public void Teardown()
    {
        if (!readyToAccept) return;

        lock (analyseLock)
        {
            readyToAccept = false;
            captureDeviceWrapper.Teardown();

            model?.Dispose();
            recogniser?.Dispose();
        }
    }

    private void initialiseMicrophoneCapture()
    {
        captureDeviceWrapper = new CaptureDeviceWrapper();
        captureDeviceWrapper.OnNewData += analyseAudio;
        captureDeviceWrapper.OnLog += OnLog;
        captureDeviceWrapper.Initialise();

        OnLog?.Invoke($"Listening to microphone {captureDeviceWrapper.CurrentCaptureDevice?.DeviceFriendlyName.Trim()}");
    }

    private void initialiseVosk()
    {
        if (captureDeviceWrapper.AudioCapture is null)
        {
            OnLog?.Invoke("Could not initialise Vosk. No default microphone found");
            return;
        }

        model = new Model(modelDirectoryPath);
        recogniser = new VoskRecognizer(model, captureDeviceWrapper.AudioCapture.WaveFormat.SampleRate);
        recogniser.SetWords(true);
    }

    private void analyseAudio(byte[] buffer, int bytesRecorded)
    {
        OnBeforeAnalysis?.Invoke();

        if (!AnalysisEnabled || !readyToAccept) return;

        lock (analyseLock)
        {
            if (recogniser is null) return;

            var isFinalResult = recogniser.AcceptWaveform(buffer, bytesRecorded);

            if (isFinalResult)
                handleFinalRecognition();
            else
                handlePartialRecognition();
        }
    }

    private void handlePartialRecognition()
    {
        var partialResult = JsonConvert.DeserializeObject<PartialRecognition>(recogniser!.PartialResult())?.Text;
        if (string.IsNullOrEmpty(partialResult)) return;

        OnPartialResult?.Invoke(partialResult);
    }

    private void handleFinalRecognition()
    {
        var result = JsonConvert.DeserializeObject<Recognition>(recogniser!.Result());

        if (result is not null)
        {
            if (result.IsValid && result.AverageConfidence >= RequiredConfidence)
            {
                OnLog?.Invoke($"Recognised '{result.Text}'");
                OnFinalResult?.Invoke(true, result.Text);
            }
        }
        else
        {
            OnFinalResult?.Invoke(false, string.Empty);
        }

        recogniser?.Reset();
    }

    private class Recognition
    {
        [JsonProperty("text")]
        public string Text = string.Empty;

        [JsonProperty("result")]
        public List<WordResult>? Result;

        public float AverageConfidence => Result is null || !Result.Any() ? 0f : Result.Average(wordResult => wordResult.Confidence);
        public bool IsValid => (AverageConfidence != 0f || !string.IsNullOrEmpty(Text)) && Text != "huh";
    }

    private class WordResult
    {
        [JsonProperty("conf")]
        public float Confidence;
    }

    private class PartialRecognition
    {
        [JsonProperty("partial")]
        public string Text = string.Empty;
    }
}
