// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Vosk;
using VRCOSC.App.Settings;

namespace VRCOSC.App.Audio;

public class VoskSpeechEngine : SpeechEngine
{
    public Action<string>? OnLog;

    private readonly object analyseLock = new();
    private CaptureDeviceWrapper? captureDeviceWrapper;
    private Model? model;
    private VoskRecognizer? recogniser;

    public bool Running { get; private set; }

    public VoskSpeechEngine()
    {
        Vosk.Vosk.SetLogLevel(-1);
    }

    public override void Initialise()
    {
        var modelDirectoryPath = SettingsManager.GetInstance().GetValue<string>(VRCOSCSetting.VOSK_ModelDirectory);

        if (!Directory.Exists(modelDirectoryPath))
        {
            OnLog?.Invoke("Model directory not found");
            return;
        }

        if (!(Directory.GetDirectories(modelDirectoryPath).FirstOrDefault()?.EndsWith("am") ?? false))
        {
            OnLog?.Invoke("Model directory invalid");
            return;
        }

        Task.Run(() =>
        {
            lock (analyseLock)
            {
                initialiseMicrophoneCapture();
                initialiseVosk(modelDirectoryPath);

                Running = true;
            }
        });
    }

    public override void Teardown()
    {
        if (!Running) return;

        lock (analyseLock)
        {
            Running = false;

            SettingsManager.GetInstance().GetObservable(VRCOSCSetting.SelectedInputDeviceID).PropertyChanged -= handleInputDeviceChange;

            captureDeviceWrapper?.Teardown();
            captureDeviceWrapper = null;

            model?.Dispose();
            model = null;
            recogniser?.Dispose();
            recogniser = null;
        }
    }

    private void initialiseMicrophoneCapture()
    {
        captureDeviceWrapper = new CaptureDeviceWrapper();
        captureDeviceWrapper.OnNewData += analyseAudio;

        var device = AudioHelper.GetDeviceByID(SettingsManager.GetInstance().GetValue<string>(VRCOSCSetting.SelectedInputDeviceID));
        captureDeviceWrapper.Initialise(device);

        SettingsManager.GetInstance().GetObservable(VRCOSCSetting.SelectedInputDeviceID).PropertyChanged += handleInputDeviceChange;
    }

    private void handleInputDeviceChange(object? sender, PropertyChangedEventArgs e)
    {
        var newDeviceId = SettingsManager.GetInstance().GetValue<string>(VRCOSCSetting.SelectedInputDeviceID);
        var device = AudioHelper.GetDeviceByID(newDeviceId);
        captureDeviceWrapper?.ChangeDevice(device);
    }

    private void initialiseVosk(string modelDirectoryPath)
    {
        if (captureDeviceWrapper?.AudioCapture is null)
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
        if (!Running) return;

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
            if (result.IsValid)
            {
                OnLog?.Invoke($"Recognised '{result.Text}'");
                OnFinalResult?.Invoke(new SpeechResult(true, result.AverageConfidence, result.Text));
            }
        }
        else
        {
            OnFinalResult?.Invoke(new SpeechResult(false, 0f, string.Empty));
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
