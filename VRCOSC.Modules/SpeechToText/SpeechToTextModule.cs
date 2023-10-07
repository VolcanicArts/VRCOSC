// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Avatar;
using VRCOSC.Game.Providers.SpeechToText;

namespace VRCOSC.Modules.SpeechToText;

[ModuleTitle("Speech To Text")]
[ModuleDescription("Speech to text using VOSK's local processing for VRChat's ChatBox")]
[ModuleAuthor("VolcanicArts", "https://github.com/VolcanicArts", "https://avatars.githubusercontent.com/u/29819296?v=4")]
[ModuleGroup(ModuleType.Accessibility)]
public class SpeechToTextModule : ChatBoxModule
{
    private SpeechToTextProvider? speechToTextProvider;

    private bool listening;
    private bool playerMuted;
    private int selectedModel;

    protected override void CreateAttributes()
    {
        CreateSetting(SpeechToTextSetting.ModelLocations, new SpeechToTextModelInstanceListAttribute
        {
            Name = "Model Locations",
            Description = "The folder locations of the speech models you'd like to use\nRecommended default: vosk-model-small-en-us-0.15\nChanging the Model parameter will switch models at runtime",
            Default = new List<SpeechToTextModelInstance>()
        });

        CreateSetting(SpeechToTextSetting.FollowMute, "Follow Mute", "Only run recognition when you're muted", false);
        CreateSetting(SpeechToTextSetting.Confidence, "Confidence", "How confident should VOSK be to push a result to the ChatBox? (%)", 75, 0, 100);

        CreateParameter<bool>(SpeechToTextParameter.Reset, ParameterMode.Read, "VRCOSC/SpeechToText/Reset", "Reset", "Manually reset the state to idle to remove the generated text from the ChatBox");
        CreateParameter<bool>(SpeechToTextParameter.Listen, ParameterMode.ReadWrite, "VRCOSC/SpeechToText/Listen", "Listen", "Whether Speech To Text is currently listening");
        CreateParameter<int>(SpeechToTextParameter.Model, ParameterMode.ReadWrite, "VRCOSC/SpeechToText/Model", "Selected Model", "The (0th based) index of the model you'd like to use. This allows you to switch models in real time");

        CreateVariable(SpeechToTextVariable.Text, "Text", "text");

        CreateState(SpeechToTextState.Idle, "Idle", string.Empty);
        CreateState(SpeechToTextState.TextGenerating, "Text Generating", $"{GetVariableFormat(SpeechToTextVariable.Text)}");
        CreateState(SpeechToTextState.TextGenerated, "Text Generated", $"{GetVariableFormat(SpeechToTextVariable.Text)}");

        CreateEvent(SpeechToTextEvent.TextGenerated, "Text Generated", $"{GetVariableFormat(SpeechToTextVariable.Text)}", 20);
    }

    protected override void OnModuleStart()
    {
        initProvider();

        listening = true;
        selectedModel = 0;
        resetState();
        SendParameter(SpeechToTextParameter.Listen, listening);
        SendParameter(SpeechToTextParameter.Model, selectedModel);
    }

    private void initProvider()
    {
        speechToTextProvider = new SpeechToTextProvider();
        speechToTextProvider.OnLog += Log;
        speechToTextProvider.OnBeforeAnalysis += onBeforeAnalysis;
        speechToTextProvider.OnPartialResult += onPartialResult;
        speechToTextProvider.OnFinalResult += onFinalResult;

        if (!GetSettingList<SpeechToTextModelInstance>(SpeechToTextSetting.ModelLocations).Any())
        {
            Log("Please add at least 1 model in the speech to text settings");
            return;
        }

        speechToTextProvider.Initialise(GetSettingList<SpeechToTextModelInstance>(SpeechToTextSetting.ModelLocations)[selectedModel].Path.Value);
    }

    [ModuleUpdate(ModuleUpdateMode.Custom, false, 5000)]
    private void onModuleUpdate()
    {
        speechToTextProvider?.Update();
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
        speechToTextProvider?.Teardown();
        speechToTextProvider = null;
    }

    protected override async void OnRegisteredParameterReceived(AvatarParameter parameter)
    {
        switch (parameter.Lookup)
        {
            case SpeechToTextParameter.Reset when parameter.ValueAs<bool>():
                resetState();
                break;

            case SpeechToTextParameter.Listen:
                listening = parameter.ValueAs<bool>();
                break;

            case SpeechToTextParameter.Model:
                var modelIndex = parameter.ValueAs<int>();

                if (selectedModel != modelIndex)
                {
                    Log($"Model change requested. Changing to model at index {modelIndex}");
                    selectedModel = modelIndex;
                    speechToTextProvider?.Teardown();
                    speechToTextProvider = null;
                    await Task.Delay(1000);
                    initProvider();
                }

                break;
        }
    }

    private void onBeforeAnalysis()
    {
        speechToTextProvider!.AnalysisEnabled = listening && (!GetSetting<bool>(SpeechToTextSetting.FollowMute) || playerMuted);
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
        ModelLocations,
        FollowMute,
        Confidence
    }

    private enum SpeechToTextParameter
    {
        Reset,
        Listen,
        Model
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
