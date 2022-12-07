// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.IO;
using System.Linq;
using System.Speech.Recognition;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Vosk;
using System;
using System.Threading;

namespace VRCOSC.Game.Modules.Modules.SpeechToText;

public sealed class SpeechToTextModule : Module
{
    public override string Title => "Speech To Text";
    public override string Description => "Speech to text using VOSK's local processing for VRChat's ChatBox";
    public override string Author => "VolcanicArts";
    public override ModuleType ModuleType => ModuleType.Accessibility;
    protected override int ChatBoxPriority => 4;

    private readonly SpeechRecognitionEngine speechRecognitionEngine = new();
    private VoskRecognizer recognizer = null!;
    private readonly MemoryStream initialStream = new();
    private readonly MemoryStream finalStream = new();

    public SpeechToTextModule()
    {
        speechRecognitionEngine.SetInputToDefaultAudioDevice();
        speechRecognitionEngine.LoadGrammar(new DictationGrammar());

        Vosk.Vosk.SetLogLevel(-1);
    }

    protected override void CreateAttributes()
    {
        base.CreateAttributes();
        CreateSetting(SpeechToTextSetting.ModelLocation, "Model Location", "The folder location of the speech model you'd like to use.\nFor standard English, download 'vosk-model-small-en-us-0.15'", string.Empty, "Download a model",
            () => OpenUrlExternally("https://alphacephei.com/vosk/models"));
        CreateSetting(SpeechToTextSetting.DisplayPeriod, "Display Period", "How long should a valid recognition be shown for? (Milliseconds)", 10000);
        CreateSetting(SpeechToTextSetting.FollowMute, "Follow Mute", "Should speech to text only be enabled if you're muted in game?", false);
    }

    protected override async Task OnStart(CancellationToken cancellationToken)
    {
        await base.OnStart(cancellationToken);

        if (!Directory.Exists(GetSetting<string>(SpeechToTextSetting.ModelLocation)))
        {
            Log("Please enter a valid model folder path");
            return;
        }

        speechRecognitionEngine.SpeechHypothesized += onTalkingDetected;
        speechRecognitionEngine.SpeechRecognized += onTalkingFinished;
        speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);

        Log("Loading model...");

        var model = new Model(GetSetting<string>(SpeechToTextSetting.ModelLocation));
        recognizer = new VoskRecognizer(model, 16000);
        recognizer.SetMaxAlternatives(0);
        recognizer.SetWords(true);

        Log("Model loaded!");

        SetChatBoxTyping(false);
    }

    protected override async Task OnStop()
    {
        await base.OnStop();

        speechRecognitionEngine.RecognizeAsyncStop();
        speechRecognitionEngine.SpeechHypothesized -= onTalkingDetected;
        speechRecognitionEngine.SpeechRecognized -= onTalkingFinished;

        recognizer.Dispose();

        SetChatBoxTyping(false);
    }

    private void onTalkingDetected(object? sender, SpeechHypothesizedEventArgs e)
    {
        if (GetSetting<bool>(SpeechToTextSetting.FollowMute) && !(Player.IsMuted ?? false)) return;

        SetChatBoxTyping(true);
    }

    private void onTalkingFinished(object? sender, SpeechRecognizedEventArgs e)
    {
        if (GetSetting<bool>(SpeechToTextSetting.FollowMute) && !(Player.IsMuted ?? false)) return;
        if (e.Result.Audio is null) return;

        initialStream.SetLength(0);
        finalStream.SetLength(0);

        // 2nd stream used as GetBuffer() must be called
        e.Result.Audio.WriteToWaveStream(initialStream);
        finalStream.Write(initialStream.GetBuffer());

        var buffer = new byte[4096];
        int bytesRead;

        while ((bytesRead = finalStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            recognizer.AcceptWaveform(buffer, bytesRead);
        }

        var finalResult = JsonConvert.DeserializeObject<Recognition>(recognizer.FinalResult())?.Text ?? string.Empty;

        SetChatBoxTyping(false);
        recognizer.Reset();

        if (string.IsNullOrEmpty(finalResult)) return;

        finalResult = string.Concat(finalResult.First().ToString().ToUpper(), finalResult.AsSpan(1));

        Log($"Recognised: {finalResult}");
        // TODO: Find a way to allow SpeechToText to override the whole ChatBox system
        //SetChatBoxText(finalResult, GetSetting<int>(SpeechToTextSetting.DisplayPeriod));
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
