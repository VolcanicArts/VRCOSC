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
    public override string Description => "Speech to text for VRChat's ChatBox";
    public override string Author => "VolcanicArts";
    public override ModuleType ModuleType => ModuleType.Accessibility;

    private readonly SpeechRecognitionEngine speechRecognitionEngine = new();

    public SpeechToTextModule()
    {
        speechRecognitionEngine.SetInputToDefaultAudioDevice();
        speechRecognitionEngine.LoadGrammar(new DictationGrammar());
    }

    protected override void CreateAttributes()
    {
        CreateSetting(SpeechToTextSetting.AccessToken, "Access Token", "Your access token to access the STT API", string.Empty, "Obtain Access Token", async () =>
        {
            SetSetting(SpeechToTextSetting.AccessToken, string.Empty);
            authenticate();
        });
    }

    protected override void OnStart()
    {
        speechRecognitionEngine.SpeechHypothesized += speechHypothesising;
        speechRecognitionEngine.SpeechRecognized += speechRecognising;
        speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);

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

        var speechBuilder = new SpeechClientBuilder
        {
            GoogleCredential = GoogleCredential.FromAccessToken(GetSetting<string>(SpeechToTextSetting.AccessToken))
        };

        var speech = speechBuilder.Build();

        var config = new RecognitionConfig
        {
            LanguageCode = LanguageCodes.English.UnitedStates
        };

        const string path = @"./tempAudio.wav";

        using (Stream outputStream = new FileStream(path, FileMode.Create))
        {
            RecognizedAudio nameAudio = e.Result.Audio;
            nameAudio.WriteToWaveStream(outputStream);
            outputStream.Close();
        }

        var audio = RecognitionAudio.FromFile(@"./tempAudio.wav");

        var response = speech.Recognize(config, audio);

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
