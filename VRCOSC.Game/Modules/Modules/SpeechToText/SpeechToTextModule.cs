using System.Speech.Recognition;

namespace VRCOSC.Game.Modules.Modules.SpeechToText;

public class SpeechToTextModule : Module
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
        Terminal.Log($"Recognised: {e.Result.Text}");
        SetChatBoxTyping(false);
        SetChatBoxText(e.Result.Text, true);
    }
}
