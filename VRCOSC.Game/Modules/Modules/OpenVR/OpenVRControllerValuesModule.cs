// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.Game.Modules.Modules.OpenVR;

public class OpenVRControllerValuesModule : Module
{
    public override string Title => "OpenVR Controller Values";
    public override string Description => "Gets controller values to send into VRChat";
    public override string Author => "VolcanicArts";
    public override ModuleType ModuleType => ModuleType.General;
    protected override int DeltaUpdate => vrc_osc_delta_update;

    protected override void CreateAttributes()
    {
        CreateParameter<bool>(OpenVRControllerValuesParameter.LeftATouch, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/A/Touch", "Whether the left a button is currently touched");
        CreateParameter<bool>(OpenVRControllerValuesParameter.LeftBTouch, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/B/Touch", "Whether the left b button is currently touched");
        CreateParameter<bool>(OpenVRControllerValuesParameter.LeftPadTouch, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Pad/Touch", "Whether the left pad is currently touched");
        CreateParameter<bool>(OpenVRControllerValuesParameter.LeftStickTouch, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Stick/Touch", "Whether the left stick is currently touched");
        CreateParameter<float>(OpenVRControllerValuesParameter.LeftIndex, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Finger/Index", "The touch value of your left index finger");
        CreateParameter<float>(OpenVRControllerValuesParameter.LeftMiddle, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Finger/Middle", "The touch value of your left middle finger");
        CreateParameter<float>(OpenVRControllerValuesParameter.LeftRing, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Finger/Ring", "The touch value of your left ring finger");
        CreateParameter<float>(OpenVRControllerValuesParameter.LeftPinky, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Finger/Pinky", "The touch value of your left pinky finger");

        CreateParameter<bool>(OpenVRControllerValuesParameter.RightATouch, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/A/Touch", "Whether the right a button is currently touched");
        CreateParameter<bool>(OpenVRControllerValuesParameter.RightBTouch, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/B/Touch", "Whether the right b button is currently touched");
        CreateParameter<bool>(OpenVRControllerValuesParameter.RightPadTouch, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Pad/Touch", "Whether the right pad is currently touched");
        CreateParameter<bool>(OpenVRControllerValuesParameter.RightStickTouch, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Stick/Touch", "Whether the right stick is currently touched");
        CreateParameter<float>(OpenVRControllerValuesParameter.RightIndex, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Finger/Index", "The touch value of your right index finger");
        CreateParameter<float>(OpenVRControllerValuesParameter.RightMiddle, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Finger/Middle", "The touch value of your right middle finger");
        CreateParameter<float>(OpenVRControllerValuesParameter.RightRing, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Finger/Ring", "The touch value of your right ring finger");
        CreateParameter<float>(OpenVRControllerValuesParameter.RightPinky, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Finger/Pinky", "The touch value of your right pinky finger");
    }

    protected override Task OnUpdate()
    {
        if (OpenVrInterface.IsLeftControllerPresent())
        {
            var leftController = OpenVrInterface.GetLeftControllerData();
            SendParameter(OpenVRControllerValuesParameter.LeftATouch, leftController.ATouched);
            SendParameter(OpenVRControllerValuesParameter.LeftBTouch, leftController.BTouched);
            SendParameter(OpenVRControllerValuesParameter.LeftPadTouch, leftController.PadTouched);
            SendParameter(OpenVRControllerValuesParameter.LeftStickTouch, leftController.StickTouched);
            SendParameter(OpenVRControllerValuesParameter.LeftIndex, leftController.IndexFinger);
            SendParameter(OpenVRControllerValuesParameter.LeftMiddle, leftController.MiddleFinger);
            SendParameter(OpenVRControllerValuesParameter.LeftRing, leftController.RingFinger);
            SendParameter(OpenVRControllerValuesParameter.LeftPinky, leftController.PinkyFinger);
        }

        if (OpenVrInterface.IsRightControllerPresent())
        {
            var rightController = OpenVrInterface.GetRightControllerData();
            SendParameter(OpenVRControllerValuesParameter.RightATouch, rightController.ATouched);
            SendParameter(OpenVRControllerValuesParameter.RightBTouch, rightController.BTouched);
            SendParameter(OpenVRControllerValuesParameter.RightPadTouch, rightController.PadTouched);
            SendParameter(OpenVRControllerValuesParameter.RightStickTouch, rightController.StickTouched);
            SendParameter(OpenVRControllerValuesParameter.RightIndex, rightController.IndexFinger);
            SendParameter(OpenVRControllerValuesParameter.RightMiddle, rightController.MiddleFinger);
            SendParameter(OpenVRControllerValuesParameter.RightRing, rightController.RingFinger);
            SendParameter(OpenVRControllerValuesParameter.RightPinky, rightController.PinkyFinger);
        }

        return Task.CompletedTask;
    }

    private enum OpenVRControllerValuesParameter
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
