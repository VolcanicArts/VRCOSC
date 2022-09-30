using System;
using System.IO;
using System.Linq;
using System.Speech.Recognition;
using System.Text;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Speech.v1;
using Google.Cloud.Speech.V1;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace VRCOSC.Game.Modules.Modules.SpeechToText;

public sealed class SpeechToTextModule : Module
{
    public override string Title => "Speech To Text";
    public override string Description => "Speech to text using Google's API for VRChat's ChatBox";
    public override string Author => "VolcanicArts";
    public override ModuleType ModuleType => ModuleType.Accessibility;

    private readonly SpeechRecognitionEngine speechRecognitionEngine = new();
    private SpeechClient speechClient;
    private RecognitionConfig recognitionConfig;

    public SpeechToTextModule()
    {
        speechRecognitionEngine.SetInputToDefaultAudioDevice();
        speechRecognitionEngine.LoadGrammar(new DictationGrammar());
    }

    protected override void CreateAttributes()
    {
        CreateSetting(SpeechToTextSetting.AccessToken, "Access Token", "Your access token to access the Google Speech-To-Text API", string.Empty, "Obtain Access Token", authenticate);
        CreateSetting(SpeechToTextSetting.FilterProfanity, "Filter Profanity", "Whether profanity should be replaced with ****", false);
        CreateSetting(SpeechToTextSetting.LanguageCode, "Language Code", "What language should be detected", LanguageCodes.English.UnitedStates);
    }

    protected override void OnStart()
    {
        speechRecognitionEngine.SpeechHypothesized += speechHypothesising;
        speechRecognitionEngine.SpeechRecognized += speechRecognising;
        speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);

        speechClient = new SpeechClientBuilder
        {
            GoogleCredential = GoogleCredential.FromAccessToken(GetSetting<string>(SpeechToTextSetting.AccessToken))
        }.Build();

        recognitionConfig = new RecognitionConfig
        {
            LanguageCode = GetSetting<string>(SpeechToTextSetting.LanguageCode),
            ProfanityFilter = GetSetting<bool>(SpeechToTextSetting.FilterProfanity),
            MaxAlternatives = 0
        };

        SetChatBoxTyping(false);
        SetChatBoxText("SpeechToText Activated", true);
    }

    protected override void OnStop()
    {
        speechRecognitionEngine.RecognizeAsyncStop();
        speechRecognitionEngine.SpeechHypothesized -= speechHypothesising;
        speechRecognitionEngine.SpeechRecognized -= speechRecognising;

        SetChatBoxTyping(false);
        SetChatBoxText("SpeechToText Deactivated", true);
    }

    private void speechHypothesising(object? sender, SpeechHypothesizedEventArgs e)
    {
        SetChatBoxTyping(true);
    }

    private void speechRecognising(object? sender, SpeechRecognizedEventArgs e)
    {
        if (e.Result.Audio is null) return;
        if (string.IsNullOrEmpty(e.Result.Text)) return;

        using var memoryStream = new MemoryStream();
        e.Result.Audio.WriteToWaveStream(memoryStream);
        var audio = RecognitionAudio.FromBytes(memoryStream.GetBuffer());
        var response = speechClient.Recognize(recognitionConfig, audio);

        try
        {
            var textBuilder = new StringBuilder();
            response.Results.ForEach(result => textBuilder.Append(result.Alternatives.First().Transcript + ". "));

            var text = textBuilder.ToString().Trim();
            if (string.IsNullOrEmpty(text)) return;

            Log($"Recognised: {text}");
            SetChatBoxTyping(false);
            SetChatBoxText(text, true);
        }
        catch (InvalidOperationException) { }
    }

    private void authenticate()
    {
        var credentials = GoogleWebAuthorizationBroker.AuthorizeAsync(
            new ClientSecrets
            {
                ClientId = VRCOSCSecrets.GOOGLE_CLIENT_ID,
                ClientSecret = VRCOSCSecrets.GOOGLE_CLIENT_SECRET
            },
            new[] { SpeechService.Scope.CloudPlatform },
            "user",
            CancellationToken.None).Result;

        SetSetting(SpeechToTextSetting.AccessToken, credentials.Token.AccessToken);
    }

    private enum SpeechToTextSetting
    {
        AccessToken,
        FilterProfanity,
        LanguageCode
    }
}
