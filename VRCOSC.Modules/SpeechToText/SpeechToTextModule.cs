// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.ChatBox;
using VRCOSC.Game.Providers.SpeechToText;

namespace VRCOSC.Modules.SpeechToText;

[ModuleTitle("Speech To Text")]
[ModuleDescription("Speech to text using VOSK's local processing for VRChat's ChatBox")]
[ModuleAuthor("VolcanicArts", "https://github.com/VolcanicArts", "https://avatars.githubusercontent.com/u/29819296?v=4")]
[ModuleGroup(ModuleType.Accessibility)]
public class SpeechToTextModule : ChatBoxModule
{
    private readonly SpeechToTextProvider speechToTextProvider;

    private bool listening;
    private bool playerMuted;

    public SpeechToTextModule()
    {
        speechToTextProvider = new SpeechToTextProvider();
        speechToTextProvider.OnLog += Log;
        speechToTextProvider.OnBeforeAnalysis += onBeforeAnalysis;
        speechToTextProvider.OnPartialResult += onPartialResult;
        speechToTextProvider.OnFinalResult += onFinalResult;
    }

    protected override void CreateAttributes()
    {
        CreateSetting(SpeechToTextSetting.ModelLocation, "Model Location", "The folder location of the speech model you'd like to use\nRecommended default: vosk-model-small-en-us-0.15", string.Empty, "Download a model", () => OpenUrlExternally("https://alphacephei.com/vosk/models"));
        CreateSetting(SpeechToTextSetting.FollowMute, "Follow Mute", "Only run recognition when you're muted", false);
        CreateSetting(SpeechToTextSetting.Confidence, "Confidence", "How confident should VOSK be to push a result to the ChatBox? (%)", 75, 0, 100);

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
        speechToTextProvider.Initialise(GetSetting<string>(SpeechToTextSetting.ModelLocation));
        listening = true;
        resetState();
        SendParameter(SpeechToTextParameter.Listen, listening);
    }

    protected override void OnPlayerUpdate()
    {
        var isPlayerMuted = Player.IsMuted.GetValueOrDefault();
        if (playerMuted == isPlayerMuted) return;

        playerMuted = isPlayerMuted;
        if (GetSetting<bool>(SpeechToTextSetting.FollowMute)) resetState();
    }

    protected override void OnModuleStop()
    {
        speechToTextProvider.Teardown();
    }

    protected override void OnModuleParameterReceived(AvatarParameter parameter)
    {
        switch (parameter.Lookup)
        {
            case SpeechToTextParameter.Reset when parameter.ValueAs<bool>():
                resetState();
                break;

            case SpeechToTextParameter.Listen:
                listening = parameter.ValueAs<bool>();
                break;
        }
    }

    private void onBeforeAnalysis()
    {
        speechToTextProvider.AnalysisEnabled = listening && (!GetSetting<bool>(SpeechToTextSetting.FollowMute) || playerMuted);
        speechToTextProvider.RequiredConfidence = GetSetting<int>(SpeechToTextSetting.Confidence) / 100f;
    }

    private void onPartialResult(string text)
    {
        ChangeStateTo(SpeechToTextState.TextGenerating);
        SetVariableValue(SpeechToTextVariable.Text, formatTextForChatBox(text));
        SetChatBoxTyping(true);
    }

    private void onFinalResult(bool success, string text)
    {
        if (success)
        {
            SetVariableValue(SpeechToTextVariable.Text, formatTextForChatBox(text));
            ChangeStateTo(SpeechToTextState.TextGenerated);
            TriggerEvent(SpeechToTextEvent.TextGenerated);
        }
        else
        {
            resetState();
        }
    }

    private void resetState()
    {
        SetChatBoxTyping(false);
        ChangeStateTo(SpeechToTextState.Idle);
        SetVariableValue(SpeechToTextVariable.Text, string.Empty);
    }

    private static string formatTextForChatBox(string text) => text.Length > 1 ? text[..1].ToUpper(CultureInfo.CurrentCulture) + text[1..] : text.ToUpper(CultureInfo.CurrentCulture);

    private enum SpeechToTextSetting
    {
        ModelLocation,
        FollowMute,
        Confidence
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
