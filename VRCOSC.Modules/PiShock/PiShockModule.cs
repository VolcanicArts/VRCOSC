// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Avatar;
using VRCOSC.Game.Providers.PiShock;
using VRCOSC.Game.Providers.SpeechToText;

namespace VRCOSC.Modules.PiShock;

[ModuleTitle("PiShock")]
[ModuleDescription("Allows for controlling PiShock shockers from avatar parameters and voice control using speech to text")]
[ModuleAuthor("VolcanicArts", "https://github.com/VolcanicArts", "https://avatars.githubusercontent.com/u/29819296?v=4")]
[ModuleGroup(ModuleType.NSFW)]
[ModulePrefab("VRCOSC-PiShock", "https://github.com/VolcanicArts/VRCOSC/releases/download/latest/VRCOSC-PiShock.unitypackage")]
public class PiShockModule : AvatarModule
{
    private PiShockProvider? piShockProvider;
    private SpeechToTextProvider? speechToTextProvider;

    private int group;
    private float duration;
    private float intensity;

    private DateTimeOffset? shock;
    private DateTimeOffset? vibrate;
    private DateTimeOffset? beep;
    private bool shockExecuted;
    private bool vibrateExecuted;
    private bool beepExecuted;

    private int convertedDuration => (int)Math.Round(Map(duration, 0, 1, 1, GetSetting<int>(PiShockSetting.MaxDuration)));
    private int convertedIntensity => (int)Math.Round(Map(intensity, 0, 1, 1, GetSetting<int>(PiShockSetting.MaxIntensity)));

    protected override void CreateAttributes()
    {
        CreateSetting(PiShockSetting.Username, "Username", "Your PiShock username", string.Empty);
        CreateSetting(PiShockSetting.APIKey, "API Key", "Your PiShock API key", string.Empty, "Generate API Key", () => OpenUrlExternally("https://pishock.com/#/account"));

        CreateSetting(PiShockSetting.Delay, "Button Delay", "The amount of time in milliseconds the shock, vibrate, and beep parameters need to be true to execute the action\nThis is helpful for if you accidentally press buttons on your action menu", 0);
        CreateSetting(PiShockSetting.MaxDuration, "Max Duration", "The maximum value the duration can be in seconds\nThis is the upper limit of 100% duration and is local only", 15, 1, 15);
        CreateSetting(PiShockSetting.MaxIntensity, "Max Intensity", "The maximum value the intensity can be in percent\nThis is the upper limit of 100% intensity and is local only", 100, 1, 100);

        CreateSetting(PiShockSetting.Shockers, new PiShockShockerInstanceListAttribute
        {
            Name = "Shockers",
            Description = "Each instance represents a single shocker using a sharecode\nThe key is used as a reference to create groups of shockers",
            Default = new List<PiShockShockerInstance>()
        });

        CreateSetting(PiShockSetting.Groups, new PiShockGroupInstanceListAttribute
        {
            Name = "Groups",
            Description = "Each instance should contain one or more shocker keys separated by a comma\nA group can be chosen by setting the Group parameter to the left number",
            Default = new List<PiShockGroupInstance>()
        });

        CreateSetting(PiShockSetting.EnableVoiceControl, "Enable Voice Control", "Enables voice control using speech to text and a phrase list", false);

        CreateSetting(PiShockSetting.SpeechModelLocation, "Speech Model Location", "The folder location of the speech model you'd like to use\nRecommended default: vosk-model-small-en-us-0.15", string.Empty, "Download a model", () => OpenUrlExternally("https://alphacephei.com/vosk/models"), () => GetSetting<bool>(PiShockSetting.EnableVoiceControl));
        CreateSetting(PiShockSetting.SpeechConfidence, "Speech Confidence", "How confident should VOSK be that it's recognised a phrase to execute the action? (%)", 75, 0, 100, () => GetSetting<bool>(PiShockSetting.EnableVoiceControl));

        CreateSetting(PiShockSetting.PhraseList, new PiShockPhraseInstanceListAttribute
        {
            Name = "Phrase List",
            Description = "The list of words or phrases and what to do when they're said\nUse the shocker keys from Shockers to reference sharecodes",
            Default = new List<PiShockPhraseInstance>(),
            DependsOn = () => GetSetting<bool>(PiShockSetting.EnableVoiceControl)
        });

        CreateParameter<int>(PiShockParameter.Group, ParameterMode.ReadWrite, "VRCOSC/PiShock/Group", "Group", "The group to select for the actions");
        CreateParameter<float>(PiShockParameter.Duration, ParameterMode.ReadWrite, "VRCOSC/PiShock/Duration", "Duration", "The duration of the action as a percentage mapped between 1-15");
        CreateParameter<float>(PiShockParameter.Intensity, ParameterMode.ReadWrite, "VRCOSC/PiShock/Intensity", "Intensity", "The intensity of the action as a percentage mapped between 1-100");
        CreateParameter<bool>(PiShockParameter.Shock, ParameterMode.Read, "VRCOSC/PiShock/Shock", "Shock", "Executes a shock using the defined parameters");
        CreateParameter<bool>(PiShockParameter.Vibrate, ParameterMode.Read, "VRCOSC/PiShock/Vibrate", "Vibrate", "Executes a vibration using the defined parameters");
        CreateParameter<bool>(PiShockParameter.Beep, ParameterMode.Read, "VRCOSC/PiShock/Beep", "Beep", "Executes a beep using the defined parameters");
        CreateParameter<bool>(PiShockParameter.Success, ParameterMode.Write, "VRCOSC/PiShock/Success", "Success", "If the execution was successful, this will become true for 1 second to act as a notification");
    }

    protected override void OnModuleStart()
    {
        group = 0;
        duration = 0f;
        intensity = 0f;
        shock = null;
        vibrate = null;
        beep = null;
        shockExecuted = false;
        vibrateExecuted = false;
        beepExecuted = false;

        piShockProvider = new PiShockProvider();

        if (GetSetting<bool>(PiShockSetting.EnableVoiceControl))
        {
            speechToTextProvider = new SpeechToTextProvider();
            speechToTextProvider.OnLog += Log;
            speechToTextProvider.OnFinalResult += onNewSentenceSpoken;
            speechToTextProvider.Initialise(GetSetting<string>(PiShockSetting.SpeechModelLocation));
        }

        sendParameters();
    }

    protected override void OnModuleStop()
    {
        speechToTextProvider?.Teardown();
        speechToTextProvider = null;
    }

    protected override void OnAvatarChange()
    {
        sendParameters();
    }

    [ModuleUpdate(ModuleUpdateMode.Custom, true, 5000)]
    private void onModuleUpdate()
    {
        speechToTextProvider?.Update();
    }

    [ModuleUpdate(ModuleUpdateMode.Custom)]
    private void setSpeechToTextParameters()
    {
        if (speechToTextProvider is not null)
            speechToTextProvider.RequiredConfidence = GetSetting<int>(PiShockSetting.SpeechConfidence) / 100f;
    }

    [ModuleUpdate(ModuleUpdateMode.Custom)]
    private void checkForExecutions()
    {
        var delay = TimeSpan.FromMilliseconds(GetSetting<int>(PiShockSetting.Delay));

        if (shock is not null && shock + delay <= DateTimeOffset.Now && !shockExecuted)
        {
            executePiShockMode(PiShockMode.Shock);
            shockExecuted = true;
        }

        if (shock is null) shockExecuted = false;

        if (vibrate is not null && vibrate + delay <= DateTimeOffset.Now && !vibrateExecuted)
        {
            executePiShockMode(PiShockMode.Vibrate);
            vibrateExecuted = true;
        }

        if (vibrate is null) vibrateExecuted = false;

        if (beep is not null && beep + delay <= DateTimeOffset.Now && !beepExecuted)
        {
            executePiShockMode(PiShockMode.Beep);
            beepExecuted = true;
        }

        if (beep is null) beepExecuted = false;
    }

    private async void onNewSentenceSpoken(bool success, string sentence)
    {
        if (!success) return;

        foreach (var wordInstance in GetSettingList<PiShockPhraseInstance>(PiShockSetting.PhraseList).Where(wordInstance => sentence.Contains(wordInstance.Phrase.Value, StringComparison.InvariantCultureIgnoreCase)))
        {
            Log($"Found word: {wordInstance.Phrase.Value}");

            var shockerInstance = getShockerInstanceFromKey(wordInstance.ShockerKey.Value);
            if (shockerInstance is null) continue;

            var response = await piShockProvider!.Execute(GetSetting<string>(PiShockSetting.Username), GetSetting<string>(PiShockSetting.APIKey), shockerInstance.Sharecode.Value, wordInstance.Mode.Value, wordInstance.Duration.Value, wordInstance.Intensity.Value);

            Log(response.Success ? $"Executing {wordInstance.Mode.Value} on {wordInstance.ShockerKey.Value} with duration {response.FinalDuration}s and intensity {response.FinalIntensity}%" : response.Message);
        }
    }

    private async void executePiShockMode(PiShockMode mode)
    {
        var groupData = GetSettingList<PiShockGroupInstance>(PiShockSetting.Groups).ElementAtOrDefault(group);

        if (groupData is null)
        {
            Log($"No group with ID {group}");
            return;
        }

        var shockerKeys = groupData.Keys.Value.Split(',').Where(key => !string.IsNullOrEmpty(key)).Select(key => key.Trim());

        foreach (var shockerKey in shockerKeys)
        {
            var shockerInstance = getShockerInstanceFromKey(shockerKey);
            if (shockerInstance is null) continue;

            await sendPiShockData(mode, shockerInstance);
        }
    }

    private async Task sendPiShockData(PiShockMode mode, PiShockShockerInstance instance)
    {
        var response = await piShockProvider!.Execute(GetSetting<string>(PiShockSetting.Username), GetSetting<string>(PiShockSetting.APIKey), instance.Sharecode.Value, mode, convertedDuration, convertedIntensity);

        Log(response.Success ? $"Executing {mode} on {instance.Sharecode.Value} with duration {response.FinalDuration}s and intensity {response.FinalIntensity}%" : response.Message);

        if (response.Success)
        {
            _ = Task.Run(async () =>
            {
                SendParameter(PiShockParameter.Success, true);
                await Task.Delay(1000);
                SendParameter(PiShockParameter.Success, false);
            });
        }
    }

    private PiShockShockerInstance? getShockerInstanceFromKey(string key)
    {
        var instance = GetSettingList<PiShockShockerInstance>(PiShockSetting.Shockers).SingleOrDefault(shockerInstance => shockerInstance.Key.Value == key);

        if (instance is not null) return instance;

        Log($"No shocker with key {key}");
        return null;
    }

    private void sendParameters()
    {
        SendParameter(PiShockParameter.Group, group);
        SendParameter(PiShockParameter.Duration, duration);
        SendParameter(PiShockParameter.Intensity, intensity);
    }

    protected override void OnRegisteredParameterReceived(AvatarParameter parameter)
    {
        switch (parameter.Lookup)
        {
            case PiShockParameter.Shock:
                shock = parameter.ValueAs<bool>() ? DateTimeOffset.Now : null;
                break;

            case PiShockParameter.Vibrate:
                vibrate = parameter.ValueAs<bool>() ? DateTimeOffset.Now : null;
                break;

            case PiShockParameter.Beep:
                beep = parameter.ValueAs<bool>() ? DateTimeOffset.Now : null;
                break;

            case PiShockParameter.Group:
                group = parameter.ValueAs<int>();
                break;

            case PiShockParameter.Duration:
                duration = Math.Clamp(parameter.ValueAs<float>(), 0f, 1f);
                break;

            case PiShockParameter.Intensity:
                intensity = Math.Clamp(parameter.ValueAs<float>(), 0f, 1f);
                break;
        }
    }

    private enum PiShockSetting
    {
        Username,
        APIKey,
        MaxDuration,
        MaxIntensity,
        Shockers,
        Groups,
        Delay,
        EnableVoiceControl,
        SpeechModelLocation,
        SpeechConfidence,
        PhraseList
    }

    private enum PiShockParameter
    {
        Group,
        Duration,
        Intensity,
        Shock,
        Vibrate,
        Beep,
        Success
    }
}
