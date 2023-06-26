// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Attributes;
using VRCOSC.Game.Providers.PiShock;

namespace VRCOSC.Modules.PiShock;

public class PiShockModule : Module
{
    public override string Title => "PiShock";
    public override string Description => "Allows for controlling PiShock from avatar parameters";
    public override string Author => "VolcanicArts";
    public override ModuleType Type => ModuleType.NSFW;

    private int profile;
    private float duration;
    private float intensity;
    private PiShockProvider? piShockProvider;

    private int convertedDuration => (int)Math.Round(Map(duration, 0, 1, 1, 15));
    private int convertedIntensity => (int)Math.Round(Map(intensity, 0, 1, 1, 100));

    protected override void CreateAttributes()
    {
        CreateSetting(PiShockSetting.Profiles, "Profiles", "A list of profiles that allows for selection using the Profile parameter\nProfile is 0 by default (1st profile entry) for people that only want local control", new List<MutableKeyValuePair>(), "Username", "ShareCode");

        CreateParameter<int>(PiShockParameter.Profile, ParameterMode.ReadWrite, "VRCOSC/PiShock/Profile", "Profile", "The profile to select for the actions");
        CreateParameter<float>(PiShockParameter.Duration, ParameterMode.ReadWrite, "VRCOSC/PiShock/Duration", "Duration", "The duration of the action as a percentage mapped between 1-15");
        CreateParameter<float>(PiShockParameter.Intensity, ParameterMode.ReadWrite, "VRCOSC/PiShock/Intensity", "Intensity", "The intensity of the action as a percentage mapped between 1-100");
        CreateParameter<bool>(PiShockParameter.Shock, ParameterMode.Read, "VRCOSC/PiShock/Shock", "Shock", "Executes a shock using the defined parameters");
        CreateParameter<bool>(PiShockParameter.Vibrate, ParameterMode.Read, "VRCOSC/PiShock/Vibrate", "Vibrate", "Executes a vibration using the defined parameters");
        CreateParameter<bool>(PiShockParameter.Beep, ParameterMode.Read, "VRCOSC/PiShock/Beep", "Beep", "Executes a beep using the defined parameters");
    }

    protected override void OnModuleStart()
    {
        piShockProvider ??= new PiShockProvider(OfficialModuleSecrets.GetSecret(OfficialModuleSecretsKeys.PiShock));

        profile = 0;
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
        SendParameter(PiShockParameter.Profile, profile);
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
            case PiShockParameter.Profile:
                profile = value;
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

    private void executePiShockMode(PiShockMode mode)
    {
        if (piShockProvider is null) return;

        var profileData = GetSettingList<MutableKeyValuePair>(PiShockSetting.Profiles).ElementAtOrDefault(profile);

        if (profileData is null)
        {
            Log($"No profile with ID {profile}");
            return;
        }

        piShockProvider.Username = profileData.Key.Value;
        piShockProvider.ShareCode = profileData.Value.Value;
        piShockProvider.Execute(mode, convertedDuration, convertedIntensity);
    }

    private enum PiShockSetting
    {
        Profiles
    }

    private enum PiShockParameter
    {
        Profile,
        Duration,
        Intensity,
        Shock,
        Vibrate,
        Beep
    }
}
