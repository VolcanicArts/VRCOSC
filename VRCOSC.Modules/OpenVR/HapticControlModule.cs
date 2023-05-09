// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules;

namespace VRCOSC.Modules.OpenVR;

public class HapticControlModule : Module
{
    public override string Title => "Haptic Control";
    public override string Description => "Lets you set haptic parameters and trigger them for OpenVR controllers";
    public override string Author => "VolcanicArts";
    public override ModuleType Type => ModuleType.OpenVR;

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
