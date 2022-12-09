// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading;
using System.Threading.Tasks;
using VRCOSC.Game.Modules.Util;

namespace VRCOSC.Game.Modules.Modules.OpenVR;

public class GestureExtensionsModule : Module
{
    public override string Title => "Gesture Extensions";
    public override string Description => "Detect a range of custom gestures from Index controllers";
    public override string Author => "VolcanicArts";
    public override ModuleType ModuleType => ModuleType.OpenVR;
    protected override int DeltaUpdate => vrc_osc_delta_update;

    private float lowerThreshold;
    private float upperThreshold;

    protected override void CreateAttributes()
    {
        CreateSetting(GestureExtensionsSetting.LowerThreshold, "Lower Threshold", "How far down a finger should be until it's not considered up", 0.5f, 0, 1);
        CreateSetting(GestureExtensionsSetting.UpperThreshold, "Upper Threshold", "How far down a finger should be before it's considered down", 0.5f, 0, 1);

        CreateParameter<int>(GestureExtensionsParameter.GestureLeft, ParameterMode.Write, "VRCOSC/Gestures/Left", "Custom left hand gesture value");
        CreateParameter<int>(GestureExtensionsParameter.GestureRight, ParameterMode.Write, "VRCOSC/Gestures/Right", "Custom right hand gesture value");
    }

    protected override Task OnStart(CancellationToken cancellationToken)
    {
        lowerThreshold = GetSetting<float>(GestureExtensionsSetting.LowerThreshold);
        upperThreshold = GetSetting<float>(GestureExtensionsSetting.UpperThreshold);

        return Task.CompletedTask;
    }

    protected override Task OnUpdate()
    {
        if (!OpenVrInterface.HasInitialised) return Task.CompletedTask;

        if (OpenVrInterface.IsLeftControllerConnected()) SendParameter(GestureExtensionsParameter.GestureLeft, (int)getLeftControllerGesture());
        if (OpenVrInterface.IsRightControllerConnected()) SendParameter(GestureExtensionsParameter.GestureRight, (int)getRightControllerGesture());

        return Task.CompletedTask;
    }

    private GestureNames getLeftControllerGesture() => getControllerGesture(OpenVrInterface.LeftController);
    private GestureNames getRightControllerGesture() => getControllerGesture(OpenVrInterface.RightController);

    private GestureNames getControllerGesture(ControllerData controllerData)
    {
        if (isGestureDoubleGun(controllerData)) return GestureNames.DoubleGun;
        if (isGestureMiddleFinger(controllerData)) return GestureNames.MiddleFinger;
        if (isGesturePinkyFinger(controllerData)) return GestureNames.PinkyFinger;

        return GestureNames.None;
    }

    private bool isGestureDoubleGun(ControllerData controllerData)
    {
        return controllerData.IndexFinger < lowerThreshold
               && controllerData.MiddleFinger < lowerThreshold
               && controllerData.RingFinger > upperThreshold
               && controllerData.PinkyFinger > upperThreshold
               && !controllerData.ThumbDown;
    }

    private bool isGestureMiddleFinger(ControllerData controllerData)
    {
        return controllerData.IndexFinger > upperThreshold
               && controllerData.MiddleFinger < lowerThreshold
               && controllerData.RingFinger > upperThreshold
               && controllerData.PinkyFinger > upperThreshold;
    }

    private bool isGesturePinkyFinger(ControllerData controllerData)
    {
        return controllerData.IndexFinger > upperThreshold
               && controllerData.MiddleFinger > upperThreshold
               && controllerData.RingFinger > upperThreshold
               && controllerData.PinkyFinger < lowerThreshold;
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
