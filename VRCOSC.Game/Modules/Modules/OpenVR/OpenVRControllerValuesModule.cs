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
        CreateParameter<bool>(OpenVRControllerValuesParameter.LeftA, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/A", "Whether the left a button is currently touched");
        CreateParameter<bool>(OpenVRControllerValuesParameter.LeftB, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/B", "Whether the left b button is currently touched");
        CreateParameter<bool>(OpenVRControllerValuesParameter.LeftPad, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Pad", "Whether the left pad is currently touched");
        CreateParameter<bool>(OpenVRControllerValuesParameter.LeftStick, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Stick", "Whether the left stick is currently touched");
        CreateParameter<float>(OpenVRControllerValuesParameter.LeftIndex, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Finger/Index", "The touch value of your left index finger");
        CreateParameter<float>(OpenVRControllerValuesParameter.LeftMiddle, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Finger/Middle", "The touch value of your left middle finger");
        CreateParameter<float>(OpenVRControllerValuesParameter.LeftRing, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Finger/Ring", "The touch value of your left ring finger");
        CreateParameter<float>(OpenVRControllerValuesParameter.LeftPinky, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Finger/Pinky", "The touch value of your left pinky finger");

        CreateParameter<bool>(OpenVRControllerValuesParameter.RightA, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/A", "Whether the right a button is currently touched");
        CreateParameter<bool>(OpenVRControllerValuesParameter.RightB, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/B", "Whether the right b button is currently touched");
        CreateParameter<bool>(OpenVRControllerValuesParameter.RightPad, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Pad", "Whether the right pad is currently touched");
        CreateParameter<bool>(OpenVRControllerValuesParameter.RightStick, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Stick", "Whether the right stick is currently touched");
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
            SendParameter(OpenVRControllerValuesParameter.LeftA, leftController.ATouched);
            SendParameter(OpenVRControllerValuesParameter.LeftB, leftController.BTouched);
            SendParameter(OpenVRControllerValuesParameter.LeftPad, leftController.PadTouched);
            SendParameter(OpenVRControllerValuesParameter.LeftStick, leftController.StickTouched);
            SendParameter(OpenVRControllerValuesParameter.LeftIndex, leftController.IndexFinger);
            SendParameter(OpenVRControllerValuesParameter.LeftMiddle, leftController.MiddleFinger);
            SendParameter(OpenVRControllerValuesParameter.LeftRing, leftController.RingFinger);
            SendParameter(OpenVRControllerValuesParameter.LeftPinky, leftController.PinkyFinger);
        }

        if (OpenVrInterface.IsRightControllerPresent())
        {
            var rightController = OpenVrInterface.GetRightControllerData();
            SendParameter(OpenVRControllerValuesParameter.RightA, rightController.ATouched);
            SendParameter(OpenVRControllerValuesParameter.RightB, rightController.BTouched);
            SendParameter(OpenVRControllerValuesParameter.RightPad, rightController.PadTouched);
            SendParameter(OpenVRControllerValuesParameter.RightStick, rightController.StickTouched);
            SendParameter(OpenVRControllerValuesParameter.RightIndex, rightController.IndexFinger);
            SendParameter(OpenVRControllerValuesParameter.RightMiddle, rightController.MiddleFinger);
            SendParameter(OpenVRControllerValuesParameter.RightRing, rightController.RingFinger);
            SendParameter(OpenVRControllerValuesParameter.RightPinky, rightController.PinkyFinger);
        }

        return Task.CompletedTask;
    }

    private enum OpenVRControllerValuesParameter
    {
        LeftA,
        LeftB,
        LeftPad,
        LeftStick,
        LeftIndex,
        LeftMiddle,
        LeftRing,
        LeftPinky,
        RightA,
        RightB,
        RightPad,
        RightStick,
        RightIndex,
        RightMiddle,
        RightRing,
        RightPinky
    }
}
