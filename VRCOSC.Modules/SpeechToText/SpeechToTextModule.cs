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
    private VoskRecognizer recognizer = null!;
    private bool readyToAccept;

    public SpeechToTextModule()
    {
        speechRecognitionEngine.SetInputToDefaultAudioDevice();
        speechRecognitionEngine.LoadGrammar(new DictationGrammar());
        speechRecognitionEngine.SpeechHypothesized += speechHypothesised;
        speechRecognitionEngine.SpeechRecognized += speechRecognised;

        Vosk.Vosk.SetLogLevel(-1);
    }

    protected override void CreateAttributes()
    {
        CreateSetting(SpeechToTextSetting.ModelLocation, "Model Location", "The folder location of the speech model you'd like to use", string.Empty, "Download a model", () => OpenUrlExternally("https://alphacephei.com/vosk/models"));

        CreateParameter<bool>(SpeechToTextParameter.Reset, ParameterMode.Read, "VRCOSC/SpeechToText/Reset", "Reset", "Manually reset the state to idle to remove the generated text from the ChatBox");

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

        speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);

        Log("Model loading...");
        readyToAccept = false;

        Task.Run(() =>
        {
            var model = new Model(GetSetting<string>(SpeechToTextSetting.ModelLocation));
            recognizer = new VoskRecognizer(model, 16000);
            recognizer.SetMaxAlternatives(0);
            recognizer.SetWords(true);
            Log("Model loaded!");
            readyToAccept = true;
        });

        SetChatBoxTyping(false);
        SetVariableValue(SpeechToTextVariable.Text, string.Empty);
        ChangeStateTo(SpeechToTextState.Idle);
    }

    protected override void OnModuleStop()
    {
        SetChatBoxTyping(false);
        speechRecognitionEngine.RecognizeAsyncStop();
        recognizer.Dispose();
    }

    protected override void OnBoolParameterReceived(Enum key, bool value)
    {
        switch (key)
        {
            case SpeechToTextParameter.Reset:
                ChangeStateTo(SpeechToTextState.Idle);
                SetVariableValue(SpeechToTextVariable.Text, string.Empty);
                break;
        }
    }

    private void speechHypothesised(object? sender, SpeechHypothesizedEventArgs e)
    {
        SetChatBoxTyping(true);
        SetVariableValue(SpeechToTextVariable.Text, string.Empty);
        ChangeStateTo(SpeechToTextState.Idle);
    }

    private void speechRecognised(object? sender, SpeechRecognizedEventArgs e)
    {
        if (!readyToAccept) return;

        if (e.Result.Audio is null || string.IsNullOrEmpty(e.Result.Text))
        {
            ChangeStateTo(SpeechToTextState.Idle);
            SetChatBoxTyping(false);
            return;
        }

        using var memoryStream = new MemoryStream();
        e.Result.Audio.WriteToWaveStream(memoryStream);

        // using a 2nd memory stream as GetBuffer() must be called
        using var wavStream = new MemoryStream(memoryStream.GetBuffer());

        var buffer = new byte[1024 * 8];
        int bytesRead;

        while ((bytesRead = wavStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            recognizer.AcceptWaveform(buffer, bytesRead);
        }

        var finalResult = JsonConvert.DeserializeObject<Recognition>(recognizer.FinalResult())?.Text ?? string.Empty;

        if (string.IsNullOrEmpty(finalResult))
        {
            Log("Recognition failure");
            ChangeStateTo(SpeechToTextState.Idle);
        }
        else
        {
            finalResult = finalResult[..1].ToUpper(CultureInfo.CurrentCulture) + finalResult[1..];
            Log($"Recognised: \"{finalResult}\"");
            ChangeStateTo(SpeechToTextState.TextGenerated);
        }

        SetChatBoxTyping(false);
        SetVariableValue(SpeechToTextVariable.Text, finalResult);
        TriggerEvent(SpeechToTextEvent.TextGenerated);

        recognizer.Reset();
    }

    private class Recognition
    {
        [JsonProperty("text")]
        public string Text = null!;
    }

    private enum SpeechToTextSetting
    {
        ModelLocation
    }

    private enum SpeechToTextParameter
    {
        Reset
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
