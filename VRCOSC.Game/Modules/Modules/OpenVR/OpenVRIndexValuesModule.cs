// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.Game.Modules.Modules.OpenVR;

public class OpenVRIndexValuesModule : Module
{
    public override string Title => "OpenVR Index Values";
    public override string Description => "Gets Index controller values to send into VRChat";
    public override string Author => "VolcanicArts";
    public override ModuleType ModuleType => ModuleType.General;
    protected override int DeltaUpdate => vrc_osc_delta_update;

    protected override void CreateAttributes()
    {
        CreateParameter<bool>(OpenVRIndexValuesParameter.LeftA, ParameterMode.Write, "VRCOSC/OpenVR/Index/Left/A", "Whether the left a button is currently touched");
        CreateParameter<bool>(OpenVRIndexValuesParameter.LeftB, ParameterMode.Write, "VRCOSC/OpenVR/Index/Left/B", "Whether the left b button is currently touched");
        CreateParameter<bool>(OpenVRIndexValuesParameter.LeftPad, ParameterMode.Write, "VRCOSC/OpenVR/Index/Left/Pad", "Whether the left pad is currently touched");
        CreateParameter<bool>(OpenVRIndexValuesParameter.LeftStick, ParameterMode.Write, "VRCOSC/OpenVR/Index/Left/Stick", "Whether the left stick is currently touched");
        CreateParameter<float>(OpenVRIndexValuesParameter.LeftIndex, ParameterMode.Write, "VRCOSC/OpenVR/Index/Left/Index", "The touch value of your left index finger");
        CreateParameter<float>(OpenVRIndexValuesParameter.LeftMiddle, ParameterMode.Write, "VRCOSC/OpenVR/Index/Left/Middle", "The touch value of your left middle finger");
        CreateParameter<float>(OpenVRIndexValuesParameter.LeftRing, ParameterMode.Write, "VRCOSC/OpenVR/Index/Left/Ring", "The touch value of your left ring finger");
        CreateParameter<float>(OpenVRIndexValuesParameter.LeftPinky, ParameterMode.Write, "VRCOSC/OpenVR/Index/Left/Pinky", "The touch value of your left pinky finger");

        CreateParameter<bool>(OpenVRIndexValuesParameter.RightA, ParameterMode.Write, "VRCOSC/OpenVR/Index/Right/A", "Whether the right a button is currently touched");
        CreateParameter<bool>(OpenVRIndexValuesParameter.RightB, ParameterMode.Write, "VRCOSC/OpenVR/Index/Right/B", "Whether the right b button is currently touched");
        CreateParameter<bool>(OpenVRIndexValuesParameter.RightPad, ParameterMode.Write, "VRCOSC/OpenVR/Index/Right/Pad", "Whether the right pad is currently touched");
        CreateParameter<bool>(OpenVRIndexValuesParameter.RightStick, ParameterMode.Write, "VRCOSC/OpenVR/Index/Right/Stick", "Whether the right stick is currently touched");
        CreateParameter<float>(OpenVRIndexValuesParameter.RightIndex, ParameterMode.Write, "VRCOSC/OpenVR/Index/Right/Index", "The touch value of your right index finger");
        CreateParameter<float>(OpenVRIndexValuesParameter.RightMiddle, ParameterMode.Write, "VRCOSC/OpenVR/Index/Right/Middle", "The touch value of your right middle finger");
        CreateParameter<float>(OpenVRIndexValuesParameter.RightRing, ParameterMode.Write, "VRCOSC/OpenVR/Index/Right/Ring", "The touch value of your right ring finger");
        CreateParameter<float>(OpenVRIndexValuesParameter.RightPinky, ParameterMode.Write, "VRCOSC/OpenVR/Index/Right/Pinky", "The touch value of your right pinky finger");
    }

    protected override Task OnUpdate()
    {
        var leftController = OpenVrInterface.GetLeftIndexControllerData();
        SendParameter(OpenVRIndexValuesParameter.LeftA, leftController.ATouched);
        SendParameter(OpenVRIndexValuesParameter.LeftB, leftController.BTouched);
        SendParameter(OpenVRIndexValuesParameter.LeftPad, leftController.PadTouched);
        SendParameter(OpenVRIndexValuesParameter.LeftStick, leftController.StickTouched);
        SendParameter(OpenVRIndexValuesParameter.LeftIndex, leftController.IndexFinger);
        SendParameter(OpenVRIndexValuesParameter.LeftMiddle, leftController.MiddleFinger);
        SendParameter(OpenVRIndexValuesParameter.LeftRing, leftController.RingFinger);
        SendParameter(OpenVRIndexValuesParameter.LeftPinky, leftController.PinkyFinger);

        var rightController = OpenVrInterface.GetRightIndexControllerData();
        SendParameter(OpenVRIndexValuesParameter.RightA, rightController.ATouched);
        SendParameter(OpenVRIndexValuesParameter.RightB, rightController.BTouched);
        SendParameter(OpenVRIndexValuesParameter.RightPad, rightController.PadTouched);
        SendParameter(OpenVRIndexValuesParameter.RightStick, rightController.StickTouched);
        SendParameter(OpenVRIndexValuesParameter.RightIndex, rightController.IndexFinger);
        SendParameter(OpenVRIndexValuesParameter.RightMiddle, rightController.MiddleFinger);
        SendParameter(OpenVRIndexValuesParameter.RightRing, rightController.RingFinger);
        SendParameter(OpenVRIndexValuesParameter.RightPinky, rightController.PinkyFinger);

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
