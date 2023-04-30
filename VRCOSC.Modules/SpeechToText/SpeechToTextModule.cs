// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using Vosk;
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.ChatBox;

namespace VRCOSC.Modules.SpeechToText;

public class SpeechToTextModule : ChatBoxModule
{
    public override string Title => "Speech To Text";
    public override string Description => "Speech to text using VOSK's local processing for VRChat's ChatBox";
    public override string Author => "VolcanicArts";
    public override ModuleType Type => ModuleType.General;

    private readonly MicrophoneInterface micInterface = new();
    private Model? model;
    private VoskRecognizer recogniser = null!;

    private readonly object dataWrite = new();

    private readonly Bindable<bool> talking = new();

    private bool readyToAccept;
    private bool listening;
    private bool shouldAnalyse => readyToAccept && listening && (!GetSetting<bool>(SpeechToTextSetting.FollowMute) || Player.IsMuted.GetValueOrDefault());

    public SpeechToTextModule()
    {
        Vosk.Vosk.SetLogLevel(-1);
    }

    protected override void CreateAttributes()
    {
        CreateSetting(SpeechToTextSetting.ModelLocation, "Model Location", "The folder location of the speech model you'd like to use\nRecommended default: vosk-model-small-en-us-0.15", string.Empty, "Download a model", () => OpenUrlExternally("https://alphacephei.com/vosk/models"));
        CreateSetting(SpeechToTextSetting.FollowMute, "Follow Mute", "Only run recognition when you're muted", false);

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

        Log("Model loading...");
        readyToAccept = false;
        listening = true;
        talking.Value = false;

        talking.BindValueChanged(onTalkingChanged);

        micInterface.BufferCallback = onNewBuffer;
        var captureDevice = micInterface.Hook();
        Log($"Hooked into microphone {captureDevice.DeviceFriendlyName.Trim()}");

        Task.Run(() =>
        {
            model = new Model(GetSetting<string>(SpeechToTextSetting.ModelLocation));
            Log("Model loaded!");

            lock (dataWrite)
            {
                recogniser = new VoskRecognizer(model, micInterface.AudioCapture!.WaveFormat.SampleRate);
                recogniser.SetMaxAlternatives(0);
                recogniser.SetWords(true);
            }

            readyToAccept = true;
        }).ConfigureAwait(false);

        SetChatBoxTyping(false);
        SetVariableValue(SpeechToTextVariable.Text, string.Empty);
        ChangeStateTo(SpeechToTextState.Idle);
        SendParameter(SpeechToTextParameter.Listen, listening);
    }

    private void onTalkingChanged(ValueChangedEvent<bool> e)
    {
        if (!e.NewValue) handleFinalResult();
    }

    protected override void OnModuleStop()
    {
        SetChatBoxTyping(false);
        micInterface.UnHook();
        talking.UnbindAll();

        lock (dataWrite)
        {
            recogniser.Dispose();
        }
    }

    protected override void OnBoolParameterReceived(Enum key, bool value)
    {
        switch (key)
        {
            case SpeechToTextParameter.Reset when value:
                SetChatBoxTyping(false);
                ChangeStateTo(SpeechToTextState.Idle);
                SetVariableValue(SpeechToTextVariable.Text, string.Empty);
                break;

            case SpeechToTextParameter.Listen:
                listening = value;
                break;
        }
    }

    private void handleFinalResult()
    {
        lock (dataWrite)
        {
            var finalResult = JsonConvert.DeserializeObject<Recognition>(recogniser.FinalResult())?.Text ?? string.Empty;

            if (!string.IsNullOrEmpty(finalResult))
            {
                finalResult = finalResult[..1].ToUpper(CultureInfo.CurrentCulture) + finalResult[1..];
                Log($"Recognised: \"{finalResult}\"");

                SetVariableValue(SpeechToTextVariable.Text, finalResult);
                ChangeStateTo(SpeechToTextState.TextGenerated);
                TriggerEvent(SpeechToTextEvent.TextGenerated);
            }

            SetChatBoxTyping(false);
        }
    }

    private void onNewBuffer(byte[] buffer, int bytesRecorded)
    {
        if (!shouldAnalyse) return;

        lock (dataWrite)
        {
            if (!recogniser.AcceptWaveform(buffer, bytesRecorded))
            {
                var partialResult = JsonConvert.DeserializeObject<PartialRecognition>(recogniser.PartialResult())?.Text;

                if (!string.IsNullOrEmpty(partialResult))
                {
                    if (partialResult.Length > 1)
                    {
                        partialResult = partialResult[..1].ToUpper(CultureInfo.CurrentCulture) + partialResult[1..];
                    }

                    ChangeStateTo(SpeechToTextState.TextGenerating);
                    SetVariableValue(SpeechToTextVariable.Text, partialResult);
                    SetChatBoxTyping(true);
                    talking.Value = true;
                }
            }
            else
            {
                talking.Value = false;
            }
        }
    }

    private class Recognition
    {
        [JsonProperty("text")]
        public string Text = null!;
    }

    private class PartialRecognition
    {
        [JsonProperty("partial")]
        public string Text = null!;
    }

    private enum SpeechToTextSetting
    {
        ModelLocation,
        FollowMute
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
