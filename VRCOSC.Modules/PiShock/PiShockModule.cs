// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Graphics;
using VRCOSC.Game.Modules.SDK;
using VRCOSC.Game.Modules.SDK.Attributes.Settings;
using VRCOSC.Game.Modules.SDK.Attributes.Settings.Addons;
using VRCOSC.Game.Modules.SDK.Parameters;
using VRCOSC.Game.Modules.SDK.Providers.PiShock;

namespace VRCOSC.Modules.PiShock;

[ModuleTitle("PiShock")]
[ModuleDescription("Allows for controlling PiShock shockers from avatar parameters and voice control using speech to text")]
[ModuleType(ModuleType.NSFW)]
[ModulePrefab("VRCOSC-PiShock", "https://github.com/VolcanicArts/VRCOSC/releases/download/latest/VRCOSC-PiShock.unitypackage")]
public class PiShockModule : Module
{
    private PiShockProvider? piShockProvider;

    private float duration;
    private float intensity;

    private (DateTimeOffset, int)? shock;
    private (DateTimeOffset, int)? vibrate;
    private (DateTimeOffset, int)? beep;
    private bool shockExecuted;
    private bool vibrateExecuted;
    private bool beepExecuted;

    private int convertedDuration => (int)Math.Round(Map(duration, 0, 1, 1, GetSettingValue<int>(PiShockSetting.MaxDuration)));
    private int convertedIntensity => (int)Math.Round(Map(intensity, 0, 1, 1, GetSettingValue<int>(PiShockSetting.MaxIntensity)));

    protected override void OnLoad()
    {
        CreateTextBox(PiShockSetting.Username, "Username", "Your PiShock username", string.Empty);
        CreateTextBox(PiShockSetting.APIKey, "API Key", "Your PiShock API key", string.Empty);

        CreateTextBox(PiShockSetting.Delay, "Button Delay", "The amount of time in milliseconds the shock, vibrate, and beep parameters need to be true to execute the action\nThis is helpful for if you accidentally press buttons on your action menu", 0);
        CreateSlider(PiShockSetting.MaxDuration, "Max Duration", "The maximum value the duration can be in seconds\nThis is the upper limit of 100% duration and is local only", 15, 1, 15);
        CreateSlider(PiShockSetting.MaxIntensity, "Max Intensity", "The maximum value the intensity can be in percent\nThis is the upper limit of 100% intensity and is local only", 100, 1, 100);

        CreateCustomSetting(PiShockSetting.Shockers, new ShockerListModuleSetting(
            new ModuleSettingMetadata("Shockers", "Each instance represents a single shocker using a sharecode\nThe name is used as a readable reference and can be anything you like", typeof(DrawableShockerListModuleSetting)),
            new[]
            {
                new ShockerInstance
                {
                    Name = { Value = "Test" }
                }
            }));

        CreateToggle(PiShockSetting.EnableVoiceControl, "Enable Voice Control", "Enables voice control using speech to text and the phrase list", false);

        CreateGroup("Credentials", PiShockSetting.Username, PiShockSetting.APIKey);
        CreateGroup("Tweaks", PiShockSetting.Delay);
        CreateGroup("Shockers", PiShockSetting.Shockers);
        CreateGroup("Voice Control", PiShockSetting.EnableVoiceControl);

        RegisterParameter<float>(PiShockParameter.Duration, "VRCOSC/PiShock/Duration", ParameterMode.ReadWrite, "Duration", "The duration of the action as a percentage mapped between 1-15");
        RegisterParameter<float>(PiShockParameter.Intensity, "VRCOSC/PiShock/Intensity", ParameterMode.ReadWrite, "Intensity", "The intensity of the action as a percentage mapped between 1-100");
        RegisterParameter<bool>(PiShockParameter.Shock, "VRCOSC/PiShock/Shock/*", ParameterMode.Read, "Shock Group", "For use when you want to execute a shock on a specific group instead of selecting a group");
        RegisterParameter<bool>(PiShockParameter.Vibrate, "VRCOSC/PiShock/Vibrate/*", ParameterMode.Read, "Vibrate Group", "For use when you want to execute a vibration on a specific group instead of selecting a group");
        RegisterParameter<bool>(PiShockParameter.Beep, "VRCOSC/PiShock/Beep/*", ParameterMode.Read, "Beep Group", "For use when you want to execute a beep on a specific group instead of selecting a group");
        RegisterParameter<bool>(PiShockParameter.Success, "VRCOSC/PiShock/Success", ParameterMode.Write, "Success", "If the execution was successful, this will become true for 1 second to act as a notification");
    }

    protected override void OnPostLoad()
    {
        GetSetting(PiShockSetting.APIKey)!
            .AddAddon(new ButtonModuleSettingAddon("Generate API Key", Colours.BLUE0, () => OpenUrlExternally("https://pishock.com/#/account")));

        // GetSetting(PiShockSetting.PhraseList)!.IsEnabled = () => GetSettingValue<bool>(PiShockSetting.EnableVoiceControl);
    }

    protected override Task<bool> OnModuleStart()
    {
        duration = 0f;
        intensity = 0f;
        shock = null;
        vibrate = null;
        beep = null;
        shockExecuted = false;
        vibrateExecuted = false;
        beepExecuted = false;

        piShockProvider = new PiShockProvider();

        sendParameters();

        return Task.FromResult(true);
    }

    protected override void OnAvatarChange()
    {
        sendParameters();
    }

    [ModuleUpdate(ModuleUpdateMode.Custom)]
    private void checkForExecutions()
    {
        var delay = TimeSpan.FromMilliseconds(GetSettingValue<int>(PiShockSetting.Delay));

        if (shock is not null && shock.Value.Item1 + delay <= DateTimeOffset.Now && !shockExecuted)
        {
            executePiShockMode(PiShockMode.Shock, shock.Value.Item2);
            shockExecuted = true;
        }

        if (shock is null) shockExecuted = false;

        if (vibrate is not null && vibrate.Value.Item1 + delay <= DateTimeOffset.Now && !vibrateExecuted)
        {
            executePiShockMode(PiShockMode.Vibrate, vibrate.Value.Item2);
            vibrateExecuted = true;
        }

        if (vibrate is null) vibrateExecuted = false;

        if (beep is not null && beep.Value.Item1 + delay <= DateTimeOffset.Now && !beepExecuted)
        {
            executePiShockMode(PiShockMode.Beep, beep.Value.Item2);
            beepExecuted = true;
        }

        if (beep is null) beepExecuted = false;
    }

    private async void executePiShockMode(PiShockMode mode, int group)
    {
        // var groupData = GetSettingValue<List<PiShockGroupInstance>>(PiShockSetting.Groups).ElementAtOrDefault(group);
        //
        // if (groupData is null)
        // {
        //     Log($"No group with ID {group}");
        //     return;
        // }
        //
        // var shockerKeys = groupData.Names.Value.Split(',').Where(key => !string.IsNullOrEmpty(key)).Select(key => key.Trim());
        //
        // foreach (var shockerKey in shockerKeys)
        // {
        //     var shockerInstance = getShockerInstanceFromKey(shockerKey);
        //     if (shockerInstance is null) continue;
        //
        //     await sendPiShockData(mode, shockerInstance);
        // }
    }

    private async Task sendPiShockData(PiShockMode mode, ShockerInstance instance)
    {
        var response = await piShockProvider!.Execute(GetSettingValue<string>(PiShockSetting.Username)!, GetSettingValue<string>(PiShockSetting.APIKey)!, instance.Sharecode.Value, mode, convertedDuration, convertedIntensity);

        Log(response.Success ? $"Executing {mode} on {instance.Name.Value} with duration {response.FinalDuration}s and intensity {response.FinalIntensity}%" : response.Message);

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

    private ShockerInstance? getShockerInstanceFromKey(string key)
    {
        var instance = GetSettingValue<List<ShockerInstance>>(PiShockSetting.Shockers).SingleOrDefault(shockerInstance => shockerInstance.Name.Value == key);

        if (instance is not null) return instance;

        Log($"No shocker with key '{key}'");
        return null;
    }

    private void sendParameters()
    {
        SendParameter(PiShockParameter.Duration, duration);
        SendParameter(PiShockParameter.Intensity, intensity);
    }

    protected override void OnRegisteredParameterReceived(RegisteredParameter parameter)
    {
        switch (parameter.Lookup)
        {
            case PiShockParameter.Duration:
                duration = Math.Clamp(parameter.GetValue<float>(), 0f, 1f);
                break;

            case PiShockParameter.Intensity:
                intensity = Math.Clamp(parameter.GetValue<float>(), 0f, 1f);
                break;

            case PiShockParameter.Shock:
                if (!parameter.IsWildcardType<int>(0)) return;

                var shockGroup = parameter.WildcardAs<int>(0);
                shock = parameter.GetValue<bool>() ? (DateTimeOffset.Now, shockGroup) : null;
                break;

            case PiShockParameter.Vibrate:
                if (!parameter.IsWildcardType<int>(0)) return;

                var vibrateGroup = parameter.WildcardAs<int>(0);
                vibrate = parameter.GetValue<bool>() ? (DateTimeOffset.Now, vibrateGroup) : null;
                break;

            case PiShockParameter.Beep:
                if (!parameter.IsWildcardType<int>(0)) return;

                var beepGroup = parameter.WildcardAs<int>(0);
                beep = parameter.GetValue<bool>() ? (DateTimeOffset.Now, beepGroup) : null;
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
        Duration,
        Intensity,
        Success,
        Shock,
        Vibrate,
        Beep
    }
}
