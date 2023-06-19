// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;
using Newtonsoft.Json;
using Vosk;
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.ChatBox;

namespace VRCOSC.Modules.SpeechToText;

public class SpeechToTextModule : ChatBoxModule
{
    public override string Title => "Speech To Text";
    public override string Description => "Speech to text using VOSK's local processing for VRChat's ChatBox";
    public override string Author => "VolcanicArts";
    public override ModuleType Type => ModuleType.Accessibility;

    private readonly MicrophoneInterface micInterface = new();
    private Model? model;
    private VoskRecognizer? recogniser;
    private readonly object analyseLock = new();

    private bool readyToAccept;
    private bool listening;
    private bool playerMuted;
    private bool shouldAnalyse => readyToAccept && listening && (!GetSetting<bool>(SpeechToTextSetting.FollowMute) || playerMuted);

    public SpeechToTextModule()
    {
        Vosk.Vosk.SetLogLevel(-1);
    }

    protected override void CreateAttributes()
    {
        CreateSetting(SpeechToTextSetting.ModelLocation, "Model Location", "The folder location of the speech model you'd like to use\nRecommended default: vosk-model-small-en-us-0.15", string.Empty, "Download a model", () => OpenUrlExternally("https://alphacephei.com/vosk/models"));
        CreateSetting(SpeechToTextSetting.FollowMute, "Follow Mute", "Only run recognition when you're muted", false);
        CreateSetting(SpeechToTextSetting.Confidence, "Confidence", "How confident should VOSK be to push a result to the ChatBox? (%)", 75, 0, 100);

        CreateParameter<bool>(SpeechToTextParameter.Reset, ParameterMode.Read, "VRCOSC/SpeechToText/Reset", "Reset", "Manually reset the state to idle to remove the generated text from the ChatBox");
        CreateParameter<bool>(SpeechToTextParameter.Listen, ParameterMode.ReadWrite, "VRCOSC/SpeechToText/Listen", "Listen", "Whether Speech To Text is currently listening");

        CreateVariable(SpeechToTextVariable.Text, "Text", "text");

        CreateState(SpeechToTextState.Idle, "Idle", string.Empty);
        CreateState(SpeechToTextState.TextGenerating, "Text Generating", $"{GetVariableFormat(SpeechToTextVariable.Text)}");
        CreateState(SpeechToTextState.TextGenerated, "Text Generated", $"{GetVariableFormat(SpeechToTextVariable.Text)}");

        CreateEvent(SpeechToTextEvent.TextGenerated, "Text Generated", $"{GetVariableFormat(SpeechToTextVariable.Text)}", 20);
    }

    protected override void OnModuleStart()
    {
        if (!Directory.Exists(GetSetting<string>(SpeechToTextSetting.ModelLocation)))
        {
            Log("Please enter a valid model folder path");
            return;
        }

        readyToAccept = false;
        listening = true;

        initialiseMicrophone();
        initialiseVosk();
        resetState();

        SendParameter(SpeechToTextParameter.Listen, listening);
    }

    protected override void OnPlayerUpdate()
    {
        var isPlayerMuted = Player.IsMuted.GetValueOrDefault();
        if (playerMuted == isPlayerMuted) return;

        playerMuted = isPlayerMuted;
        if (GetSetting<bool>(SpeechToTextSetting.FollowMute)) resetState();
    }

    private void initialiseMicrophone() => Task.Run(() =>
    {
        Log("Hooking into default microphone...");

        micInterface.BufferCallback += analyseAudio;
        var captureDevice = micInterface.Hook();

        if (captureDevice is null)
        {
            Log("Failed to hook into default microphone. Please restart the module");
            return;
        }

        LogDebug($"Microphone format:\n{micInterface.AudioCapture!.WaveFormat}");

        Log($"Hooked into microphone {captureDevice.DeviceFriendlyName.Trim()}");
    });

    private void initialiseVosk() => Task.Run(() =>
    {
        Log("Model loading...");

        model = new Model(GetSetting<string>(SpeechToTextSetting.ModelLocation));

        lock (analyseLock)
        {
            recogniser = new VoskRecognizer(model, micInterface.AudioCapture!.WaveFormat.SampleRate);
            recogniser.SetWords(true);
        }

        Log("Model loaded!");
        readyToAccept = true;
    });

    private void analyseAudio(byte[] buffer, int bytesRecorded)
    {
        if (!HasStarted || !shouldAnalyse) return;

        LogDebug($"Analysing audio. Total bytes: {bytesRecorded}");

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
        LogDebug("Partial recognition has occurred");

        var partialResult = JsonConvert.DeserializeObject<PartialRecognition>(recogniser!.PartialResult())?.Text;
        if (string.IsNullOrEmpty(partialResult)) return;

        LogDebug("Partial result is valid");

        var partialText = formatTextForChatBox(partialResult);

        ChangeStateTo(SpeechToTextState.TextGenerating);
        SetVariableValue(SpeechToTextVariable.Text, partialText);
        SetChatBoxTyping(true);
    }

    private void handleFinalRecognition()
    {
        LogDebug("Final recognition has occurred");

        var result = JsonConvert.DeserializeObject<Recognition>(recogniser!.Result());

        if (result is not null)
        {
            LogDebug($"Final recognition deserialised successfully\nText: {result.Text}\nConfidence: {result.AverageConfidence}");

            if (result.IsValid)
            {
                LogDebug("Result is valid");

                if (result.AverageConfidence >= GetSetting<int>(SpeechToTextSetting.Confidence) / 100f)
                {
                    LogDebug("Confidence confirmed. Pushing result");

                    var finalText = formatTextForChatBox(result.Text);
                    Log($"Recognised: \"{finalText}\"");

                    SetVariableValue(SpeechToTextVariable.Text, finalText);
                    ChangeStateTo(SpeechToTextState.TextGenerated);
                    TriggerEvent(SpeechToTextEvent.TextGenerated);
                }
                else
                {
                    LogDebug("Confidence too low. Clearing partial result");
                    resetState();
                }
            }
            else
            {
                LogDebug("Result is invalid. Skipping");
            }
        }
        else
        {
            LogDebug("Final recognition deserialised incorrectly");
            resetState();
        }

        LogDebug("Resetting recogniser");

        recogniser.Reset();
        SetChatBoxTyping(false);
    }

    private static string formatTextForChatBox(string text) => text.Length > 1 ? text[..1].ToUpper(CultureInfo.CurrentCulture) + text[1..] : text.ToUpper(CultureInfo.CurrentCulture);

    protected override void OnModuleStop()
    {
        micInterface.BufferCallback -= analyseAudio;
        micInterface.UnHook();

        lock (analyseLock)
        {
            model?.Dispose();
            recogniser?.Dispose();
        }
    }

    protected override void OnBoolParameterReceived(Enum key, bool value)
    {
        switch (key)
        {
            case SpeechToTextParameter.Reset when value:
                resetState();
                break;

            case SpeechToTextParameter.Listen:
                listening = value;
                break;
        }
    }

    private void resetState()
    {
        LogDebug("Resetting module state");
        SetChatBoxTyping(false);
        ChangeStateTo(SpeechToTextState.Idle);
        SetVariableValue(SpeechToTextVariable.Text, string.Empty);
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

    private enum SpeechToTextSetting
    {
        ModelLocation,
        FollowMute,
        Confidence
    }

    private enum SpeechToTextParameter
    {
        Reset,
        Listen
    }

    private enum SpeechToTextState
    {
        Idle,
        TextGenerating,
        TextGenerated
    }

    private enum SpeechToTextEvent
    {
        TextGenerated
    }

    private enum SpeechToTextVariable
    {
        Text
    }
}
