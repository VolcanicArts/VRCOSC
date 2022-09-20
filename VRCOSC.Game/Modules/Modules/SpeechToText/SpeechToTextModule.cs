using System.IO;
using System.Speech.Recognition;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Speech.v1;
using Google.Cloud.Speech.V1;

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
        CreateSetting(SpeechToTextSetting.AccessToken, "Access Token", "Your access token to access the STT API", string.Empty, "Obtain Access Token", authenticate);
    }

    protected override void OnStart()
    {
        speechRecognitionEngine.SpeechHypothesized += speechHypothesising;
        speechRecognitionEngine.SpeechRecognized += speechRecognising;
        speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);

        var speechBuilder = new SpeechClientBuilder
        {
            GoogleCredential = GoogleCredential.FromAccessToken(GetSetting<string>(SpeechToTextSetting.AccessToken))
        };

        speechClient = speechBuilder.Build();

        recognitionConfig = new RecognitionConfig
        {
            LanguageCode = LanguageCodes.English.UnitedStates
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

        using var memoryStream = new MemoryStream();
        e.Result.Audio.WriteToWaveStream(memoryStream);
        var audio = RecognitionAudio.FromBytes(memoryStream.GetBuffer());
        var response = speechClient.Recognize(recognitionConfig, audio);
        var text = response.Results[0].Alternatives[0].Transcript;

        Terminal.Log($"Recognised: {text}");
        SetChatBoxTyping(false);
        SetChatBoxText(text, true);
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
        AccessToken
    }
}
