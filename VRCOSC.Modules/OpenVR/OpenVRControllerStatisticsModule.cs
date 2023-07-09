// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules;

namespace VRCOSC.Modules.OpenVR;

[ModuleTitle("OpenVR Controller Statistics")]
[ModuleDescription("Gets controller statistics from your OpenVR (SteamVR) session")]
[ModuleAuthor("VolcanicArts", "https://github.com/VolcanicArts", "https://avatars.githubusercontent.com/u/29819296?v=4")]
[ModuleGroup(ModuleType.OpenVR)]
public class OpenVRControllerStatisticsModule : Module
{
    protected override void CreateAttributes()
    {
        CreateParameter<bool>(OpenVRControllerStatisticsParameter.LeftATouch, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/A/Touch", "Left Controller A Touch", "Whether the left a button is currently touched");
        CreateParameter<bool>(OpenVRControllerStatisticsParameter.LeftBTouch, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/B/Touch", "Left Controller B Touch", "Whether the left b button is currently touched");
        CreateParameter<bool>(OpenVRControllerStatisticsParameter.LeftPadTouch, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Pad/Touch", "Left Controller Pad Touch", "Whether the left pad is currently touched");
        CreateParameter<bool>(OpenVRControllerStatisticsParameter.LeftStickTouch, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Stick/Touch", "Left Controller Stick Touch", "Whether the left stick is currently touched");
        CreateParameter<float>(OpenVRControllerStatisticsParameter.LeftIndex, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Finger/Index", "Left Index", "The touch value of your left index finger");
        CreateParameter<float>(OpenVRControllerStatisticsParameter.LeftMiddle, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Finger/Middle", "Left Middle", "The touch value of your left middle finger");
        CreateParameter<float>(OpenVRControllerStatisticsParameter.LeftRing, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Finger/Ring", "Left Ring", "The touch value of your left ring finger");
        CreateParameter<float>(OpenVRControllerStatisticsParameter.LeftPinky, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Finger/Pinky", "Left Pinky", "The touch value of your left pinky finger");

        CreateParameter<bool>(OpenVRControllerStatisticsParameter.RightATouch, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/A/Touch", "Right Controller A Touch", "Whether the right a button is currently touched");
        CreateParameter<bool>(OpenVRControllerStatisticsParameter.RightBTouch, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/B/Touch", "Right Controller B Touch", "Whether the right b button is currently touched");
        CreateParameter<bool>(OpenVRControllerStatisticsParameter.RightPadTouch, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Pad/Touch", "Right Controller Pad Touch", "Whether the right pad is currently touched");
        CreateParameter<bool>(OpenVRControllerStatisticsParameter.RightStickTouch, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Stick/Touch", "Right Controller Stick Touch", "Whether the right stick is currently touched");
        CreateParameter<float>(OpenVRControllerStatisticsParameter.RightIndex, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Finger/Index", "Right Index", "The touch value of your right index finger");
        CreateParameter<float>(OpenVRControllerStatisticsParameter.RightMiddle, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Finger/Middle", "Right Middle", "The touch value of your right middle finger");
        CreateParameter<float>(OpenVRControllerStatisticsParameter.RightRing, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Finger/Ring", "Right Ring", "The touch value of your right ring finger");
        CreateParameter<float>(OpenVRControllerStatisticsParameter.RightPinky, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Finger/Pinky", "Right Pinky", "The touch value of your right pinky finger");
    }

    [ModuleUpdate(ModuleUpdateMode.Custom)]
    private void updateParameters()
    {
        if (!OVRClient.HasInitialised) return;

        if (OVRClient.LeftController.IsConnected)
        {
            var input = OVRClient.LeftController.Input;
            SendParameter(OpenVRControllerStatisticsParameter.LeftATouch, input.A.Touched);
            SendParameter(OpenVRControllerStatisticsParameter.LeftBTouch, input.B.Touched);
            SendParameter(OpenVRControllerStatisticsParameter.LeftPadTouch, input.PadTouched);
            SendParameter(OpenVRControllerStatisticsParameter.LeftStickTouch, input.StickTouched);
            SendParameter(OpenVRControllerStatisticsParameter.LeftIndex, input.IndexFinger);
            SendParameter(OpenVRControllerStatisticsParameter.LeftMiddle, input.MiddleFinger);
            SendParameter(OpenVRControllerStatisticsParameter.LeftRing, input.RingFinger);
            SendParameter(OpenVRControllerStatisticsParameter.LeftPinky, input.PinkyFinger);
        }

        if (OVRClient.RightController.IsConnected)
        {
            var input = OVRClient.RightController.Input;
            SendParameter(OpenVRControllerStatisticsParameter.RightATouch, input.A.Touched);
            SendParameter(OpenVRControllerStatisticsParameter.RightBTouch, input.B.Touched);
            SendParameter(OpenVRControllerStatisticsParameter.RightPadTouch, input.PadTouched);
            SendParameter(OpenVRControllerStatisticsParameter.RightStickTouch, input.StickTouched);
            SendParameter(OpenVRControllerStatisticsParameter.RightIndex, input.IndexFinger);
            SendParameter(OpenVRControllerStatisticsParameter.RightMiddle, input.MiddleFinger);
            SendParameter(OpenVRControllerStatisticsParameter.RightRing, input.RingFinger);
            SendParameter(OpenVRControllerStatisticsParameter.RightPinky, input.PinkyFinger);
        }
    }

    private enum OpenVRControllerStatisticsParameter
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
