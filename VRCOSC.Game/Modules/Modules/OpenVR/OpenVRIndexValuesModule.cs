// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.Game.Modules.Modules.OpenVR;

public class OpenVRIndexValuesModule : Module
{
    public override string Title => "OpenVR Controller Values";
    public override string Description => "Gets controller values to send into VRChat";
    public override string Author => "VolcanicArts";
    public override ModuleType ModuleType => ModuleType.General;
    protected override int DeltaUpdate => vrc_osc_delta_update;

    protected override void CreateAttributes()
    {
        CreateParameter<bool>(OpenVRIndexValuesParameter.LeftA, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/A", "Whether the left a button is currently touched");
        CreateParameter<bool>(OpenVRIndexValuesParameter.LeftB, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/B", "Whether the left b button is currently touched");
        CreateParameter<bool>(OpenVRIndexValuesParameter.LeftPad, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Pad", "Whether the left pad is currently touched");
        CreateParameter<bool>(OpenVRIndexValuesParameter.LeftStick, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Stick", "Whether the left stick is currently touched");
        CreateParameter<float>(OpenVRIndexValuesParameter.LeftIndex, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Finger/Index", "The touch value of your left index finger");
        CreateParameter<float>(OpenVRIndexValuesParameter.LeftMiddle, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Finger/Middle", "The touch value of your left middle finger");
        CreateParameter<float>(OpenVRIndexValuesParameter.LeftRing, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Finger/Ring", "The touch value of your left ring finger");
        CreateParameter<float>(OpenVRIndexValuesParameter.LeftPinky, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Finger/Pinky", "The touch value of your left pinky finger");

        CreateParameter<bool>(OpenVRIndexValuesParameter.RightA, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/A", "Whether the right a button is currently touched");
        CreateParameter<bool>(OpenVRIndexValuesParameter.RightB, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/B", "Whether the right b button is currently touched");
        CreateParameter<bool>(OpenVRIndexValuesParameter.RightPad, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Pad", "Whether the right pad is currently touched");
        CreateParameter<bool>(OpenVRIndexValuesParameter.RightStick, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Stick", "Whether the right stick is currently touched");
        CreateParameter<float>(OpenVRIndexValuesParameter.RightIndex, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Finger/Index", "The touch value of your right index finger");
        CreateParameter<float>(OpenVRIndexValuesParameter.RightMiddle, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Finger/Middle", "The touch value of your right middle finger");
        CreateParameter<float>(OpenVRIndexValuesParameter.RightRing, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Finger/Ring", "The touch value of your right ring finger");
        CreateParameter<float>(OpenVRIndexValuesParameter.RightPinky, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Finger/Pinky", "The touch value of your right pinky finger");
    }

    protected override Task OnUpdate()
    {
        if (OpenVrInterface.IsLeftControllerPresent())
        {
            var leftController = OpenVrInterface.GetLeftControllerData();
            SendParameter(OpenVRIndexValuesParameter.LeftA, leftController.ATouched);
            SendParameter(OpenVRIndexValuesParameter.LeftB, leftController.BTouched);
            SendParameter(OpenVRIndexValuesParameter.LeftPad, leftController.PadTouched);
            SendParameter(OpenVRIndexValuesParameter.LeftStick, leftController.StickTouched);
            SendParameter(OpenVRIndexValuesParameter.LeftIndex, leftController.IndexFinger);
            SendParameter(OpenVRIndexValuesParameter.LeftMiddle, leftController.MiddleFinger);
            SendParameter(OpenVRIndexValuesParameter.LeftRing, leftController.RingFinger);
            SendParameter(OpenVRIndexValuesParameter.LeftPinky, leftController.PinkyFinger);
        }

        if (OpenVrInterface.IsRightControllerPresent())
        {
            var rightController = OpenVrInterface.GetRightControllerData();
            SendParameter(OpenVRIndexValuesParameter.RightA, rightController.ATouched);
            SendParameter(OpenVRIndexValuesParameter.RightB, rightController.BTouched);
            SendParameter(OpenVRIndexValuesParameter.RightPad, rightController.PadTouched);
            SendParameter(OpenVRIndexValuesParameter.RightStick, rightController.StickTouched);
            SendParameter(OpenVRIndexValuesParameter.RightIndex, rightController.IndexFinger);
            SendParameter(OpenVRIndexValuesParameter.RightMiddle, rightController.MiddleFinger);
            SendParameter(OpenVRIndexValuesParameter.RightRing, rightController.RingFinger);
            SendParameter(OpenVRIndexValuesParameter.RightPinky, rightController.PinkyFinger);
        }

        return Task.CompletedTask;
    }

    private enum OpenVRIndexValuesParameter
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
