// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
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
        CreateParameter<bool>(OpenVRIndexValuesParameter.LeftTrigger, ParameterMode.Write, "VRCOSC/OpenVR/Index/Left/Trigger", "Whether the left trigger is currently touched");

        CreateParameter<bool>(OpenVRIndexValuesParameter.RightA, ParameterMode.Write, "VRCOSC/OpenVR/Index/Right/A", "Whether the right a button is currently touched");
        CreateParameter<bool>(OpenVRIndexValuesParameter.RightB, ParameterMode.Write, "VRCOSC/OpenVR/Index/Right/B", "Whether the right b button is currently touched");
        CreateParameter<bool>(OpenVRIndexValuesParameter.RightPad, ParameterMode.Write, "VRCOSC/OpenVR/Index/Right/Pad", "Whether the right pad is currently touched");
        CreateParameter<bool>(OpenVRIndexValuesParameter.RightStick, ParameterMode.Write, "VRCOSC/OpenVR/Index/Right/Stick", "Whether the right stick is currently touched");
        CreateParameter<bool>(OpenVRIndexValuesParameter.RightTrigger, ParameterMode.Write, "VRCOSC/OpenVR/Index/Right/Trigger", "Whether the right trigger is currently touched");
    }

    protected override Task OnUpdate()
    {
        SendParameter(OpenVRIndexValuesParameter.LeftA, OpenVrInterface.IsLeftAButtonTouched());
        SendParameter(OpenVRIndexValuesParameter.LeftB, OpenVrInterface.IsLeftBButtonTouched());
        SendParameter(OpenVRIndexValuesParameter.LeftPad, OpenVrInterface.IsLeftPadTouched());
        SendParameter(OpenVRIndexValuesParameter.LeftStick, OpenVrInterface.IsLeftStickTouched());
        SendParameter(OpenVRIndexValuesParameter.LeftTrigger, OpenVrInterface.IsLeftTriggerTouched());

        SendParameter(OpenVRIndexValuesParameter.RightA, OpenVrInterface.IsRightAButtonTouched());
        SendParameter(OpenVRIndexValuesParameter.RightB, OpenVrInterface.IsRightBButtonTouched());
        SendParameter(OpenVRIndexValuesParameter.RightPad, OpenVrInterface.IsRightPadTouched());
        SendParameter(OpenVRIndexValuesParameter.RightStick, OpenVrInterface.IsRightStickTouched());
        SendParameter(OpenVRIndexValuesParameter.RightTrigger, OpenVrInterface.IsRightTriggerTouched());

        return Task.CompletedTask;
    }

    private enum OpenVRIndexValuesParameter
    {
        LeftA,
        LeftB,
        LeftPad,
        LeftStick,
        LeftTrigger,
        RightA,
        RightB,
        RightPad,
        RightStick,
        RightTrigger
    }
}
