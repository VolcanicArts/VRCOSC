// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.Game.Modules.Modules.OpenVR;

public class OpenVRControllerModule : Module
{
    public override string Title => "OpenVR Controllers";
    public override string Description => "Gets controller values from your OpenVR (SteamVR) session";
    public override string Author => "VolcanicArts";
    public override ModuleType ModuleType => ModuleType.General;
    protected override int DeltaUpdate => vrc_osc_delta_update;

    protected override void CreateAttributes()
    {
        CreateParameter<bool>(OpenVRControllerParameter.LeftATouch, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/A/Touch", "Whether the left a button is currently touched");
        CreateParameter<bool>(OpenVRControllerParameter.LeftBTouch, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/B/Touch", "Whether the left b button is currently touched");
        CreateParameter<bool>(OpenVRControllerParameter.LeftPadTouch, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Pad/Touch", "Whether the left pad is currently touched");
        CreateParameter<bool>(OpenVRControllerParameter.LeftStickTouch, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Stick/Touch", "Whether the left stick is currently touched");
        CreateParameter<float>(OpenVRControllerParameter.LeftIndex, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Finger/Index", "The touch value of your left index finger");
        CreateParameter<float>(OpenVRControllerParameter.LeftMiddle, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Finger/Middle", "The touch value of your left middle finger");
        CreateParameter<float>(OpenVRControllerParameter.LeftRing, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Finger/Ring", "The touch value of your left ring finger");
        CreateParameter<float>(OpenVRControllerParameter.LeftPinky, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Finger/Pinky", "The touch value of your left pinky finger");

        CreateParameter<bool>(OpenVRControllerParameter.RightATouch, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/A/Touch", "Whether the right a button is currently touched");
        CreateParameter<bool>(OpenVRControllerParameter.RightBTouch, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/B/Touch", "Whether the right b button is currently touched");
        CreateParameter<bool>(OpenVRControllerParameter.RightPadTouch, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Pad/Touch", "Whether the right pad is currently touched");
        CreateParameter<bool>(OpenVRControllerParameter.RightStickTouch, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Stick/Touch", "Whether the right stick is currently touched");
        CreateParameter<float>(OpenVRControllerParameter.RightIndex, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Finger/Index", "The touch value of your right index finger");
        CreateParameter<float>(OpenVRControllerParameter.RightMiddle, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Finger/Middle", "The touch value of your right middle finger");
        CreateParameter<float>(OpenVRControllerParameter.RightRing, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Finger/Ring", "The touch value of your right ring finger");
        CreateParameter<float>(OpenVRControllerParameter.RightPinky, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Finger/Pinky", "The touch value of your right pinky finger");
    }

    protected override Task OnUpdate()
    {
        if (OpenVrInterface.IsLeftControllerConnected())
        {
            SendParameter(OpenVRControllerParameter.LeftATouch, OpenVrInterface.LeftController.ATouched);
            SendParameter(OpenVRControllerParameter.LeftBTouch, OpenVrInterface.LeftController.BTouched);
            SendParameter(OpenVRControllerParameter.LeftPadTouch, OpenVrInterface.LeftController.PadTouched);
            SendParameter(OpenVRControllerParameter.LeftStickTouch, OpenVrInterface.LeftController.StickTouched);
            SendParameter(OpenVRControllerParameter.LeftIndex, OpenVrInterface.LeftController.IndexFinger);
            SendParameter(OpenVRControllerParameter.LeftMiddle, OpenVrInterface.LeftController.MiddleFinger);
            SendParameter(OpenVRControllerParameter.LeftRing, OpenVrInterface.LeftController.RingFinger);
            SendParameter(OpenVRControllerParameter.LeftPinky, OpenVrInterface.LeftController.PinkyFinger);
        }

        if (OpenVrInterface.IsRightControllerConnected())
        {
            SendParameter(OpenVRControllerParameter.RightATouch, OpenVrInterface.RightController.ATouched);
            SendParameter(OpenVRControllerParameter.RightBTouch, OpenVrInterface.RightController.BTouched);
            SendParameter(OpenVRControllerParameter.RightPadTouch, OpenVrInterface.RightController.PadTouched);
            SendParameter(OpenVRControllerParameter.RightStickTouch, OpenVrInterface.RightController.StickTouched);
            SendParameter(OpenVRControllerParameter.RightIndex, OpenVrInterface.RightController.IndexFinger);
            SendParameter(OpenVRControllerParameter.RightMiddle, OpenVrInterface.RightController.MiddleFinger);
            SendParameter(OpenVRControllerParameter.RightRing, OpenVrInterface.RightController.RingFinger);
            SendParameter(OpenVRControllerParameter.RightPinky, OpenVrInterface.RightController.PinkyFinger);
        }

        return Task.CompletedTask;
    }

    private enum OpenVRControllerParameter
    {
        LeftATouch,
        LeftBTouch,
        LeftPadTouch,
        LeftStickTouch,
        LeftIndex,
        LeftMiddle,
        LeftRing,
        LeftPinky,
        RightATouch,
        RightBTouch,
        RightPadTouch,
        RightStickTouch,
        RightIndex,
        RightMiddle,
        RightRing,
        RightPinky
    }
}
