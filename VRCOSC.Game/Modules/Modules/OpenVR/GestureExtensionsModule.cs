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

    private const float upper_threshold = 0.5f;
    private const float lower_threshold = 0.5f;

    protected override void CreateAttributes()
    {
        CreateParameter<int>(GestureExtensionsParameter.GestureLeft, ParameterMode.Write, "VRCOSC/Gestures/Left", "Custom left hand gesture value");
        CreateParameter<int>(GestureExtensionsParameter.GestureRight, ParameterMode.Write, "VRCOSC/Gestures/Right", "Custom right hand gesture value");
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

    private static GestureNames getControllerGesture(ControllerData controllerData)
    {
        if (isGestureDoubleGun(controllerData)) return GestureNames.DoubleGun;
        if (isGestureMiddleFinger(controllerData)) return GestureNames.MiddleFinger;
        if (isGesturePinkyFinger(controllerData)) return GestureNames.PinkyFinger;

        return GestureNames.None;
    }

    private static bool isGestureDoubleGun(ControllerData controllerData) => controllerData is { IndexFinger: < lower_threshold, MiddleFinger: < lower_threshold, RingFinger: > upper_threshold, PinkyFinger: > upper_threshold, ThumbDown: false };
    private static bool isGestureMiddleFinger(ControllerData controllerData) => controllerData is { IndexFinger: > upper_threshold, MiddleFinger: < lower_threshold, RingFinger: > upper_threshold, PinkyFinger: > upper_threshold };
    private static bool isGesturePinkyFinger(ControllerData controllerData) => controllerData is { IndexFinger: > upper_threshold, MiddleFinger: > upper_threshold, RingFinger: > upper_threshold, PinkyFinger: < lower_threshold };

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
