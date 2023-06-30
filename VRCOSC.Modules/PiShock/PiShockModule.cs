// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules;
using VRCOSC.Game.Providers.PiShock;

namespace VRCOSC.Modules.PiShock;

public class PiShockModule : Module
{
    public override string Title => "PiShock";
    public override string Description => "Allows for controlling PiShock from avatar parameters";
    public override string Author => "VolcanicArts";
    public override ModuleType Type => ModuleType.NSFW;

    private int group;
    private float duration;
    private float intensity;
    private PiShockProvider? piShockProvider;

    private int convertedDuration => (int)Math.Round(Map(duration, 0, 1, 1, GetSetting<int>(PiShockSetting.MaxDuration)));
    private int convertedIntensity => (int)Math.Round(Map(intensity, 0, 1, 1, GetSetting<int>(PiShockSetting.MaxIntensity)));

    protected override void CreateAttributes()
    {
        CreateSetting(PiShockSetting.MaxDuration, "Max Duration", "The maximum value the duration can be in seconds\nThis is local only", 15, 1, 15);
        CreateSetting(PiShockSetting.MaxIntensity, "Max Intensity", "The maximum value the intensity can be in percent\nThis is local only", 100, 1, 100);

        CreateSetting(PiShockSetting.Shockers, new PiShockShockerInstanceListAttribute
        {
            Name = "Shockers",
            Description = "Each instance represents a single shocker using a username and a sharecode\nThe key is used as a reference to create groups of shockers",
            Default = new List<PiShockShockerInstance>()
        });

        CreateSetting(PiShockSetting.Groups, new PiShockGroupInstanceListAttribute
        {
            Name = "Groups",
            Description = "Each instance should contain one or more shocker keys separated by a comma\nA group can be chosen by setting the Group parameter to the left number",
            Default = new List<PiShockGroupInstance>()
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
        piShockProvider ??= new PiShockProvider(OfficialModuleSecrets.GetSecret(OfficialModuleSecretsKeys.PiShock));

        group = 0;
        duration = 0f;
        intensity = 0f;

        sendParameters();
    }

    protected override void OnAvatarChange()
    {
        sendParameters();
    }

    private void sendParameters()
    {
        SendParameter(PiShockParameter.Group, group);
        SendParameter(PiShockParameter.Duration, duration);
        SendParameter(PiShockParameter.Intensity, intensity);
    }

    protected override void OnBoolParameterReceived(Enum key, bool value)
    {
        switch (key)
        {
            case PiShockParameter.Shock when value:
                executePiShockMode(PiShockMode.Shock);
                break;

            case PiShockParameter.Vibrate when value:
                executePiShockMode(PiShockMode.Vibrate);
                break;

            case PiShockParameter.Beep when value:
                executePiShockMode(PiShockMode.Beep);
                break;
        }
    }

    protected override void OnIntParameterReceived(Enum key, int value)
    {
        switch (key)
        {
            case PiShockParameter.Group:
                group = value;
                break;
        }
    }

    protected override void OnFloatParameterReceived(Enum key, float value)
    {
        switch (key)
        {
            case PiShockParameter.Duration:
                duration = Math.Clamp(value, 0f, 1f);
                break;

            case PiShockParameter.Intensity:
                intensity = Math.Clamp(value, 0f, 1f);
                break;
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
            var shockerInstance = GetSettingList<PiShockShockerInstance>(PiShockSetting.Shockers).SingleOrDefault(instance => instance.Key.Value == shockerKey);

            if (shockerInstance is null)
            {
                Log($"No shocker with key {shockerKey}");
                continue;
            }

            await sendPiShockData(mode, shockerInstance.Username.Value, shockerInstance.Sharecode.Value);
        }
    }

    private async Task sendPiShockData(PiShockMode mode, string username, string sharecode)
    {
        if (piShockProvider is null)
        {
            Log("PiShock failed to initialise. Cannot execute parameters. Please restart the module");
            return;
        }

        Log($"Executing {mode} on {username} with duration {convertedDuration}s and intensity {convertedIntensity}%");
        var response = await piShockProvider.Execute(username, sharecode, mode, convertedDuration, convertedIntensity);
        Log(response.Message);

        if (response.Success)
        {
            SendParameter(PiShockParameter.Success, true);
            await Task.Delay(1000);
            SendParameter(PiShockParameter.Success, false);
        }
    }

    private enum PiShockSetting
    {
        MaxDuration,
        MaxIntensity,
        Shockers,
        Groups
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
