// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.IO;
using System.Linq;
using System.Speech.Recognition;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Vosk;
using System;

namespace VRCOSC.Game.Modules.Modules.SpeechToText;

public sealed class SpeechToTextModule : Module
{
    public override string Title => "Speech To Text";
    public override string Description => "Speech to text using VOSK's local processing for VRChat's ChatBox";
    public override string Author => "VolcanicArts";
    public override ModuleType ModuleType => ModuleType.Accessibility;

    private readonly SpeechRecognitionEngine speechRecognitionEngine = new();
    private VoskRecognizer recognizer = null!;

    public SpeechToTextModule()
    {
        speechRecognitionEngine.SetInputToDefaultAudioDevice();
        speechRecognitionEngine.LoadGrammar(new DictationGrammar());

        Vosk.Vosk.SetLogLevel(-1);
    }

    protected override void CreateAttributes()
    {
        CreateSetting(SpeechToTextSetting.ModelLocation, "Model Location", "The folder location of the speech model you'd like to use", string.Empty, "Download a model", () => OpenUrlExternally("https://alphacephei.com/vosk/models"));
        CreateSetting(SpeechToTextSetting.DisplayPeriod, "Display Period", "How long should a valid recognition be shown for?", 10000);
        CreateSetting(SpeechToTextSetting.FollowMute, "Follow Mute", "Should speech to text only be enabled if you're muted in game?", false);
    }

    protected override void OnStart()
    {
        if (!Directory.Exists(GetSetting<string>(SpeechToTextSetting.ModelLocation)))
        {
            Log("Please enter a valid model folder path");
            return;
        }

        speechRecognitionEngine.SpeechHypothesized += onTalkingDetected;
        speechRecognitionEngine.SpeechRecognized += onTalkingFinished;
        speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);

        Task.Run(() =>
        {
            var model = new Model(GetSetting<string>(SpeechToTextSetting.ModelLocation));
            recognizer = new VoskRecognizer(model, 16000);
            recognizer.SetMaxAlternatives(0);
            recognizer.SetWords(true);
        });

        ChatBox.SetTyping(false);
    }

    protected override void OnStop()
    {
        speechRecognitionEngine.RecognizeAsyncStop();
        speechRecognitionEngine.SpeechHypothesized -= onTalkingDetected;
        speechRecognitionEngine.SpeechRecognized -= onTalkingFinished;

        recognizer.Dispose();

        ChatBox.SetTyping(false);
    }

    private void onTalkingDetected(object? sender, SpeechHypothesizedEventArgs e)
    {
        if (GetSetting<bool>(SpeechToTextSetting.FollowMute) && !(Player.IsMuted ?? false)) return;

        ChatBox.SetTyping(true);
    }

    private void onTalkingFinished(object? sender, SpeechRecognizedEventArgs e)
    {
        if (GetSetting<bool>(SpeechToTextSetting.FollowMute) && !(Player.IsMuted ?? false)) return;
        if (e.Result.Audio is null) return;
        if (string.IsNullOrEmpty(e.Result.Text)) return;

        using var memoryStream = new MemoryStream();
        e.Result.Audio.WriteToWaveStream(memoryStream);

        var buffer = new byte[4096];
        int bytesRead;

        // using a 2nd memory stream as GetBuffer() must be called
        using var wavStream = new MemoryStream(memoryStream.GetBuffer());

        while ((bytesRead = wavStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            recognizer.AcceptWaveform(buffer, bytesRead);
        }

        var finalResult = JsonConvert.DeserializeObject<Recognition>(recognizer.FinalResult())?.Text ?? string.Empty;
        if (string.IsNullOrEmpty(finalResult)) return;

        finalResult = string.Concat(finalResult.First().ToString().ToUpper(), finalResult.AsSpan(1));

        Log($"Recognised: {finalResult}");
        ChatBox.SetTyping(false);
        ChatBox.SetText(finalResult, true, 2, GetSetting<int>(SpeechToTextSetting.DisplayPeriod));

        recognizer.Reset();
    }

    private class Recognition
    {
        [JsonProperty("text")]
        public string Text = null!;
    }

    private enum SpeechToTextSetting
    {
        ModelLocation,
        DisplayPeriod,
        FollowMute
    }
}
