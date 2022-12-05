// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

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

    private const float upper_threshold = 0.7f;
    private const float lower_threshold = 0.3f;

    protected override void CreateAttributes()
    {
        CreateParameter<int>(GestureExtensionsParameter.GestureLeft, ParameterMode.Write, "VRCOSC/Gestures/Left", "Custom left hand gesture value");
        CreateParameter<int>(GestureExtensionsParameter.GestureRight, ParameterMode.Write, "VRCOSC/Gestures/Right", "Custom right hand gesture value");
    }

    protected override Task OnUpdate()
    {
        if (!OpenVrInterface.HasSession) return Task.CompletedTask;

        SendParameter(GestureExtensionsParameter.GestureLeft, (int)getLeftControllerGesture());
        SendParameter(GestureExtensionsParameter.GestureRight, (int)getRightControllerGesture());

        return Task.CompletedTask;
    }

    private GestureNames getLeftControllerGesture() => getControllerGesture(OpenVrInterface.LeftController);
    private GestureNames getRightControllerGesture() => getControllerGesture(OpenVrInterface.RightController);

    private static GestureNames getControllerGesture(ControllerData controllerData)
    {
        if (isGestureDoubleGun(controllerData)) return GestureNames.DoubleGun;
        if (isGestureMiddleFinger(controllerData)) return GestureNames.MiddleFinger;
        if (isGesturePinkyFinger(controllerData)) return GestureNames.PinkyFinger;

        return GestureNames.None;
    }

    private static bool isGestureDoubleGun(ControllerData controllerData)
    {
        return controllerData.IndexFinger < lower_threshold
               && controllerData.MiddleFinger < lower_threshold
               && controllerData.RingFinger > upper_threshold
               && controllerData.PinkyFinger > upper_threshold
               && !(controllerData.ATouched || controllerData.BTouched || controllerData.PadTouched || controllerData.StickTouched);
    }

    private static bool isGestureMiddleFinger(ControllerData controllerData)
    {
        return controllerData.IndexFinger > upper_threshold
               && controllerData.MiddleFinger < lower_threshold
               && controllerData.RingFinger > upper_threshold
               && controllerData.PinkyFinger > upper_threshold;
    }

    private static bool isGesturePinkyFinger(ControllerData controllerData)
    {
        return controllerData.IndexFinger > upper_threshold
               && controllerData.MiddleFinger > upper_threshold
               && controllerData.RingFinger > upper_threshold
               && controllerData.PinkyFinger < lower_threshold;
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
