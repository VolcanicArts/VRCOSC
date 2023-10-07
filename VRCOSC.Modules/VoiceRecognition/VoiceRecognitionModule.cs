// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Avatar;
using VRCOSC.Game.Providers.SpeechToText;

namespace VRCOSC.Modules.VoiceRecognition;

[ModuleTitle("Voice Recognition")]
[ModuleDescription("Set parameters when words or phrases are recognised")]
[ModuleAuthor("VolcanicArts", "https://github.com/VolcanicArts", "https://avatars.githubusercontent.com/u/29819296?v=4")]
[ModuleGroup(ModuleType.General)]
public class VoiceRecognitionModule : AvatarModule
{
    private SpeechToTextProvider? speechToTextProvider;
    private bool enabled;

    protected override void CreateAttributes()
    {
        CreateSetting(VoiceRecognitionSetting.SpeechModelLocation, "Speech Model Location", "The folder location of the speech model you'd like to use\nRecommended default: vosk-model-small-en-us-0.15", string.Empty, "Download a model", () => OpenUrlExternally("https://alphacephei.com/vosk/models"));

        CreateSetting(VoiceRecognitionSetting.SpeechConfidence, "Speech Confidence", "How confident should VOSK be that it's recognised a phrase to set the parameter? (%)", 75, 0, 100);

        CreateSetting(VoiceRecognitionSetting.PhraseList, new VoiceRecognitionPhraseInstanceListAttribute
        {
            Name = "Phrase List",
            Description = "The list of words or phrases and what parameters to set when they're recognised\nYou are allowed to add the same phrase multiple times to affect multiple parameters\nNote that for boolean parameters use 'true' and 'false'",
            Default = new List<VoiceRecognitionPhraseInstance>()
        });

        CreateParameter<bool>(VoiceRecognitionParameter.Enable, ParameterMode.ReadWrite, "VRCOSC/VoiceRecognition/Enable", "Enable", "Enables the recognition when true");
    }

    protected override void OnModuleStart()
    {
        speechToTextProvider = new SpeechToTextProvider();
        speechToTextProvider.OnLog += Log;
        speechToTextProvider.OnFinalResult += onNewSentenceSpoken;
        speechToTextProvider.RequiredConfidence = GetSetting<int>(VoiceRecognitionSetting.SpeechConfidence) / 100f;
        speechToTextProvider.Initialise(GetSetting<string>(VoiceRecognitionSetting.SpeechModelLocation));

        enabled = true;
        SendParameter(VoiceRecognitionParameter.Enable, enabled);
    }

    protected override void OnModuleStop()
    {
        speechToTextProvider?.Teardown();
        speechToTextProvider = null;
    }

    [ModuleUpdate(ModuleUpdateMode.Custom, false, 5000)]
    private void onModuleUpdate()
    {
        speechToTextProvider?.Update();
    }

    protected override void OnRegisteredParameterReceived(AvatarParameter avatarParameter)
    {
        switch (avatarParameter.Lookup)
        {
            case VoiceRecognitionParameter.Enable:
                enabled = avatarParameter.ValueAs<bool>();
                break;
        }
    }

    private async void onNewSentenceSpoken(bool success, string sentence)
    {
        if (!success) return;

        var phraseInstances = GetSettingList<VoiceRecognitionPhraseInstance>(VoiceRecognitionSetting.PhraseList).Where(wordInstance => sentence.Contains(wordInstance.Phrase.Value, StringComparison.InvariantCultureIgnoreCase)).ToList();
        if (!phraseInstances.Any()) return;

        Log($"Found phrase '{phraseInstances[0].Phrase.Value}'");

        foreach (var phraseInstance in phraseInstances)
        {
            var parameterType = await FindParameterType(phraseInstance.ParameterName.Value);

            if (parameterType is null)
            {
                Log($"Could not find parameter '{phraseInstance.ParameterName.Value}'");
                return;
            }

            object value;

            try
            {
                switch (parameterType)
                {
                    case TypeCode.Boolean:
                        value = bool.Parse(phraseInstance.Value.Value);
                        break;

                    case TypeCode.Int32:
                        value = int.Parse(phraseInstance.Value.Value);
                        break;

                    case TypeCode.Single:
                        value = float.Parse(phraseInstance.Value.Value);
                        break;

                    default:
                        Log($"Unexpected value type of {parameterType}");
                        return;
                }
            }
            catch (Exception)
            {
                Log($"Could not convert value '{phraseInstance.Value.Value}' for parameter '{phraseInstance.ParameterName.Value}'");
                return;
            }

            SendParameter(phraseInstance.ParameterName.Value, value);
        }
    }

    private enum VoiceRecognitionSetting
    {
        SpeechModelLocation,
        SpeechConfidence,
        PhraseList
    }

    private enum VoiceRecognitionParameter
    {
        Enable
    }
}
