// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;
using System.Speech.Recognition;
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
    public override ModuleType Type => ModuleType.General;

    private readonly SpeechRecognitionEngine speechRecognitionEngine = new();
    private VoskRecognizer? recogniser;
    private bool readyToAccept;
    private bool listening;
    private bool shouldAnalyse => readyToAccept && listening && (!GetSetting<bool>(SpeechToTextSetting.FollowMute) || Player.IsMuted.GetValueOrDefault());
    private readonly object processingLock = new();
    private byte[]? buffer;

    public SpeechToTextModule()
    {
        speechRecognitionEngine.LoadGrammar(new DictationGrammar());
        speechRecognitionEngine.MaxAlternates = 1;
        speechRecognitionEngine.SpeechHypothesized += speechHypothesised;
        speechRecognitionEngine.SpeechRecognized += speechRecognised;

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

        speechRecognitionEngine.SetInputToDefaultAudioDevice();
        speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
        buffer = new byte[1024 * 8];

        Log("Model loading...");
        readyToAccept = false;
        listening = true;

        Task.Run(() =>
        {
            var model = new Model(GetSetting<string>(SpeechToTextSetting.ModelLocation));

            lock (processingLock)
            {
                recogniser = new VoskRecognizer(model, 16000);
                recogniser.SetMaxAlternatives(0);
                recogniser.SetWords(true);
            }

            Log("Model loaded!");
            readyToAccept = true;
        }).ConfigureAwait(false);

        SetChatBoxTyping(false);
        SetVariableValue(SpeechToTextVariable.Text, string.Empty);
        ChangeStateTo(SpeechToTextState.Idle);
        SendParameter(SpeechToTextParameter.Listen, listening);
    }

    protected override void OnModuleStop()
    {
        SetChatBoxTyping(false);

        lock (processingLock)
        {
            speechRecognitionEngine.RecognizeAsyncStop();
            recogniser?.Dispose();
            recogniser = null;
        }

        buffer = null;
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

    private void speechHypothesised(object? sender, SpeechHypothesizedEventArgs e)
    {
        if (!shouldAnalyse) return;

        SetChatBoxTyping(true);
    }

    private void speechRecognised(object? sender, SpeechRecognizedEventArgs e)
    {
        if (!shouldAnalyse) return;

        if (buffer is null) return;

        lock (processingLock)
        {
            if (e.Result.Audio is null || recogniser is null)
            {
                reset();
                return;
            }

            var memoryStream = new MemoryStream();
            e.Result.Audio.WriteToWaveStream(memoryStream);

            // using a 2nd memory stream as GetBuffer() must be called
            var wavStream = new MemoryStream(memoryStream.GetBuffer());
            memoryStream.Dispose();

            buffer.Initialize();
            int bytesRead;

            while ((bytesRead = wavStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                recogniser.AcceptWaveform(buffer, bytesRead);
            }

            wavStream.Dispose();

            var finalResult = JsonConvert.DeserializeObject<Recognition>(recogniser.FinalResult())?.Text ?? string.Empty;

            if (string.IsNullOrEmpty(finalResult))
            {
                reset();
                return;
            }

            finalResult = finalResult[..1].ToUpper(CultureInfo.CurrentCulture) + finalResult[1..];
            Log($"Recognised: \"{finalResult}\"");

            SetVariableValue(SpeechToTextVariable.Text, finalResult);
            ChangeStateTo(SpeechToTextState.TextGenerated);
            TriggerEvent(SpeechToTextEvent.TextGenerated);
            reset();
        }
    }

    private void reset()
    {
        SetChatBoxTyping(false);
        recogniser?.Reset();
    }

    private class Recognition
    {
        [JsonProperty("text")]
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
