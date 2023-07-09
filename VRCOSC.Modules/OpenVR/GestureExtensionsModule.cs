// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules;
using VRCOSC.Game.OpenVR.Input;
using VRCOSC.Game.OSC.VRChat;

namespace VRCOSC.Modules.OpenVR;

[ModuleTitle("Gesture Extensions")]
[ModuleDescription("Detect a range of custom gestures from Index controllers")]
[ModuleAuthor("VolcanicArts", "https://github.com/VolcanicArts", "https://avatars.githubusercontent.com/u/29819296?v=4")]
[ModuleGroup(ModuleType.OpenVR)]
public class GestureExtensionsModule : Module
{
    protected override TimeSpan DeltaUpdate => VRChatOscConstants.UPDATE_TIME_SPAN;

    private float lowerThreshold;
    private float upperThreshold;

    protected override void CreateAttributes()
    {
        CreateSetting(GestureExtensionsSetting.LowerThreshold, "Lower Threshold", "How far down a finger should be until it's not considered up", 0.5f, 0, 1);
        CreateSetting(GestureExtensionsSetting.UpperThreshold, "Upper Threshold", "How far down a finger should be before it's considered down", 0.5f, 0, 1);

        CreateParameter<int>(GestureExtensionsParameter.GestureLeft, ParameterMode.Write, "VRCOSC/Gestures/Left", "Left Gestures", "Custom left hand gesture value");
        CreateParameter<int>(GestureExtensionsParameter.GestureRight, ParameterMode.Write, "VRCOSC/Gestures/Right", "Right Gestures", "Custom right hand gesture value");
    }

    protected override void OnModuleStart()
    {
        lowerThreshold = GetSetting<float>(GestureExtensionsSetting.LowerThreshold);
        upperThreshold = GetSetting<float>(GestureExtensionsSetting.UpperThreshold);
    }

    protected override void OnModuleUpdate()
    {
        if (!OVRClient.HasInitialised) return;

        if (OVRClient.LeftController.IsConnected) SendParameter(GestureExtensionsParameter.GestureLeft, (int)getControllerGesture(OVRClient.LeftController.Input));
        if (OVRClient.RightController.IsConnected) SendParameter(GestureExtensionsParameter.GestureRight, (int)getControllerGesture(OVRClient.RightController.Input));
    }

    private GestureNames getControllerGesture(InputStates input)
    {
        if (isGestureDoubleGun(input)) return GestureNames.DoubleGun;
        if (isGestureMiddleFinger(input)) return GestureNames.MiddleFinger;
        if (isGesturePinkyFinger(input)) return GestureNames.PinkyFinger;

        return GestureNames.None;
    }

    private bool isGestureDoubleGun(InputStates input)
    {
        return input.IndexFinger < lowerThreshold
               && input.MiddleFinger < lowerThreshold
               && input.RingFinger > upperThreshold
               && input.PinkyFinger > upperThreshold
               && input.ThumbUp;
    }

    private bool isGestureMiddleFinger(InputStates input)
    {
        return input.IndexFinger > upperThreshold
               && input.MiddleFinger < lowerThreshold
               && input.RingFinger > upperThreshold
               && input.PinkyFinger > upperThreshold;
    }

    private bool isGesturePinkyFinger(InputStates input)
    {
        return input.IndexFinger > upperThreshold
               && input.MiddleFinger > upperThreshold
               && input.RingFinger > upperThreshold
               && input.PinkyFinger < lowerThreshold;
    }

    private enum GestureExtensionsSetting
    {
        LowerThreshold,
        UpperThreshold
    }

    private enum GestureNames
    {
        None,
        DoubleGun,
        MiddleFinger,
        PinkyFinger
    }

    private enum GestureExtensionsParameter
    {
        GestureLeft,
        GestureRight
    }
}
