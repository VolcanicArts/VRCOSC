using System.Speech.Recognition;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;

namespace VRCOSC.Game.Modules.Modules.SpeechToText;

public class SpeechToTextModule : Module
{
    public override string Title => "Speech To Text";
    public override string Description => "Speech to text for VRChat's ChatBox";
    public override string Author => "VolcanicArts";
    public override ColourInfo Colour => Colour4.LightBlue;
    public override ModuleType ModuleType => ModuleType.General;

    private readonly SpeechRecognitionEngine speechRecognitionEngine = new();

    public SpeechToTextModule()
    {
        speechRecognitionEngine.SetInputToDefaultAudioDevice();
        speechRecognitionEngine.LoadGrammar(new DictationGrammar());
        speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
    }

    protected override void OnStart()
    {
        speechRecognitionEngine.SpeechHypothesized += speechHypothesising;
        speechRecognitionEngine.SpeechRecognized += speechRecognising;
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

    protected override void OnStop()
    {
        speechRecognitionEngine.SpeechHypothesized -= speechHypothesising;
        speechRecognitionEngine.SpeechRecognized -= speechRecognising;
    }
}
