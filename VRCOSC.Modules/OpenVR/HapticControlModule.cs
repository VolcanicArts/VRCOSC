// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules;

namespace VRCOSC.Modules.OpenVR;

[ModuleTitle("Haptic Control")]
[ModuleDescription("Lets you set haptic parameters and trigger them for OpenVR controllers")]
[ModuleAuthor("VolcanicArts", "https://github.com/VolcanicArts", "https://avatars.githubusercontent.com/u/29819296?v=4")]
[ModuleGroup(ModuleType.OpenVR)]
[ModuleInfo("The duration, frequency, and amplitude parameters can be set from your animator")]
[ModuleInfo("If you're designing a prefab, ensure these parameters are set each time before you attempt a trigger in the case that the user has restarted this module")]
[ModuleInfo("Trigger parameters must be set back to false before attempting another trigger")]
public class HapticControlModule : Module
{
    private float duration;
    private float frequency;
    private float amplitude;

    protected override void CreateAttributes()
    {
        CreateParameter<float>(HapticControlParameter.Duration, ParameterMode.Read, "VRCOSC/Haptics/Duration", "Duration", "The duration of the haptic trigger in seconds");
        CreateParameter<float>(HapticControlParameter.Frequency, ParameterMode.Read, "VRCOSC/Haptics/Frequency", "Frequency", "The frequency of the haptic trigger");
        CreateParameter<float>(HapticControlParameter.Amplitude, ParameterMode.Read, "VRCOSC/Haptics/Amplitude", "Amplitude", "The amplitude of the haptic trigger");
        CreateParameter<bool>(HapticControlParameter.Trigger, ParameterMode.Read, "VRCOSC/Haptics/Trigger", "Trigger", "Becoming true causes a haptic trigger in both controllers");
        CreateParameter<bool>(HapticControlParameter.TriggerLeft, ParameterMode.Read, "VRCOSC/Haptics/TriggerLeft", "Trigger Left", "Becoming true causes a haptic trigger in the left controller");
        CreateParameter<bool>(HapticControlParameter.TriggerRight, ParameterMode.Read, "VRCOSC/Haptics/TriggerRight", "Trigger Right", "Becoming true causes a haptic trigger in the right controller");
    }

    protected override void OnModuleStart()
    {
        duration = 0;
        frequency = 0;
        amplitude = 0;
    }

    protected override void OnFloatParameterReceived(Enum key, float value)
    {
        switch (key)
        {
            case HapticControlParameter.Duration:
                duration = value;
                break;

            case HapticControlParameter.Frequency:
                value = Math.Clamp(value, 0, 1);
                frequency = value * 100f;
                break;

            case HapticControlParameter.Amplitude:
                value = Math.Clamp(value, 0, 1);
                amplitude = value;
                break;
        }
    }

    protected override void OnBoolParameterReceived(Enum key, bool value)
    {
        switch (key)
        {
            case HapticControlParameter.Trigger when value:
                triggerHaptic(true, true);
                break;

            case HapticControlParameter.TriggerLeft when value:
                triggerHaptic(true, false);
                break;

            case HapticControlParameter.TriggerRight when value:
                triggerHaptic(false, true);
                break;
        }
    }

    private async void triggerHaptic(bool left, bool right)
    {
        if (!OVRClient.HasInitialised) return;

        if (left) OVRClient.TriggerLeftControllerHaptic(duration, frequency, amplitude);
        await Task.Delay(10);
        if (right) OVRClient.TriggerRightControllerHaptic(duration, frequency, amplitude);
    }

    private enum HapticControlParameter
    {
        Duration,
        Frequency,
        Amplitude,
        TriggerLeft,
        TriggerRight,
        Trigger
    }
}
