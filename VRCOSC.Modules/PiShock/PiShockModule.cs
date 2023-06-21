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

    private float duration;
    private float intensity;

    private PiShockProvider? piShockProvider;

    protected override void CreateAttributes()
    {
        CreateSetting(PiShockSetting.Username, "Username", "Your PiShock username", string.Empty);
        CreateSetting(PiShockSetting.ShareCode, "Share Code", "A generated PiShock share code", string.Empty);

        CreateParameter<float>(PiShockParameter.Duration, ParameterMode.Read, "VRCOSC/PiShock/Duration", "Duration", "The duration of the action as a percentage mapped between 1-15");
        CreateParameter<float>(PiShockParameter.Intensity, ParameterMode.Read, "VRCOSC/PiShock/Intensity", "Intensity", "The intensity of the action as a percentage mapped between 1-100");
        CreateParameter<bool>(PiShockParameter.TriggerShock, ParameterMode.Read, "VRCOSC/PiShock/TriggerShock", "Trigger Shock", "Triggers a shock using the defined parameters");
        CreateParameter<bool>(PiShockParameter.TriggerVibrate, ParameterMode.Read, "VRCOSC/PiShock/TriggerVibrate", "Trigger Vibrate", "Triggers a vibration using the defined parameters");
        CreateParameter<bool>(PiShockParameter.TriggerBeep, ParameterMode.Read, "VRCOSC/PiShock/TriggerBeep", "Trigger Beep", "Triggers a beep using the defined parameters");
    }

    protected override void OnModuleStart()
    {
        duration = 0f;
        intensity = 0f;

        piShockProvider = new PiShockProvider(OfficialModuleSecrets.GetSecret(OfficialModuleSecretsKeys.PiShock));
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

    protected override void OnBoolParameterReceived(Enum key, bool value)
    {
        switch (key)
        {
            case PiShockParameter.TriggerShock:
                triggerPiShockMode(PiShockMode.Shock);
                break;

            case PiShockParameter.TriggerVibrate:
                triggerPiShockMode(PiShockMode.Vibrate);
                break;

            case PiShockParameter.TriggerBeep:
                triggerPiShockMode(PiShockMode.Beep);
                break;
        }
    }

    private void triggerPiShockMode(PiShockMode mode)
    {
        if (piShockProvider is null) return;

        piShockProvider.Username = GetSetting<string>(PiShockSetting.Username);
        piShockProvider.ShareCode = GetSetting<string>(PiShockSetting.ShareCode);
        piShockProvider.Execute(mode, (int)Math.Round(Map(duration, 0, 1, 1, 15)), (int)Math.Round(Map(intensity, 0, 1, 1, 100)));
    }

    private enum PiShockSetting
    {
        Username,
        ShareCode
    }

    private enum PiShockParameter
    {
        Duration,
        Intensity,
        TriggerShock,
        TriggerVibrate,
        TriggerBeep
    }
}
