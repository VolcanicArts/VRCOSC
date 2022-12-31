// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.OpenVR.Input;
using VRCOSC.OSC.VRChat;

namespace VRCOSC.Game.Modules.Modules.OpenVR;

public partial class GestureExtensionsModule : Module
{
    public override string Title => "Gesture Extensions";
    public override string Description => "Detect a range of custom gestures from Index controllers";
    public override string Author => "VolcanicArts";
    public override ModuleType Type => ModuleType.OpenVR;
    protected override int DeltaUpdate => VRChatOscConstants.UPDATE_DELTA;

    private float lowerThreshold;
    private float upperThreshold;

    protected override void CreateAttributes()
    {
        CreateSetting(GestureExtensionsSetting.LowerThreshold, "Lower Threshold", "How far down a finger should be until it's not considered up", 0.5f, 0, 1);
        CreateSetting(GestureExtensionsSetting.UpperThreshold, "Upper Threshold", "How far down a finger should be before it's considered down", 0.5f, 0, 1);

        CreateParameter<int>(GestureExtensionsParameter.GestureLeft, ParameterMode.Write, "VRCOSC/Gestures/Left", "Custom left hand gesture value");
        CreateParameter<int>(GestureExtensionsParameter.GestureRight, ParameterMode.Write, "VRCOSC/Gestures/Right", "Custom right hand gesture value");
    }

    protected override void OnModuleStart()
    {
        lowerThreshold = GetSetting<float>(GestureExtensionsSetting.LowerThreshold);
        upperThreshold = GetSetting<float>(GestureExtensionsSetting.UpperThreshold);
    }

    protected override void OnModuleUpdate()
    {
        if (!OVRClient.HasInitialised) return;

        if (OVRClient.LeftController?.IsConnected ?? false) SendParameter(GestureExtensionsParameter.GestureLeft, (int)getControllerGesture(OVRClient.LeftController.Input));
        if (OVRClient.RightController?.IsConnected ?? false) SendParameter(GestureExtensionsParameter.GestureRight, (int)getControllerGesture(OVRClient.RightController.Input));
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
