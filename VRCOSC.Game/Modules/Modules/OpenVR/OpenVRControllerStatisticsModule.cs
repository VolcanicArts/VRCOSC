// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.Game.Modules.Modules.OpenVR;

public class OpenVRControllerStatisticsModule : Module
{
    public override string Title => "OpenVR Controller Statistics";
    public override string Description => "Gets controller statistics from your OpenVR (SteamVR) session";
    public override string Author => "VolcanicArts";
    public override ModuleType ModuleType => ModuleType.OpenVR;
    protected override int DeltaUpdate => Constants.OSC_UPDATE_DELTA;

    protected override void CreateAttributes()
    {
        CreateParameter<bool>(OpenVRControllerStatisticsParameter.LeftATouch, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/A/Touch", "Whether the left a button is currently touched");
        CreateParameter<bool>(OpenVRControllerStatisticsParameter.LeftBTouch, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/B/Touch", "Whether the left b button is currently touched");
        CreateParameter<bool>(OpenVRControllerStatisticsParameter.LeftPadTouch, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Pad/Touch", "Whether the left pad is currently touched");
        CreateParameter<bool>(OpenVRControllerStatisticsParameter.LeftStickTouch, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Stick/Touch", "Whether the left stick is currently touched");
        CreateParameter<float>(OpenVRControllerStatisticsParameter.LeftIndex, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Finger/Index", "The touch value of your left index finger");
        CreateParameter<float>(OpenVRControllerStatisticsParameter.LeftMiddle, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Finger/Middle", "The touch value of your left middle finger");
        CreateParameter<float>(OpenVRControllerStatisticsParameter.LeftRing, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Finger/Ring", "The touch value of your left ring finger");
        CreateParameter<float>(OpenVRControllerStatisticsParameter.LeftPinky, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Input/Finger/Pinky", "The touch value of your left pinky finger");

        CreateParameter<bool>(OpenVRControllerStatisticsParameter.RightATouch, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/A/Touch", "Whether the right a button is currently touched");
        CreateParameter<bool>(OpenVRControllerStatisticsParameter.RightBTouch, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/B/Touch", "Whether the right b button is currently touched");
        CreateParameter<bool>(OpenVRControllerStatisticsParameter.RightPadTouch, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Pad/Touch", "Whether the right pad is currently touched");
        CreateParameter<bool>(OpenVRControllerStatisticsParameter.RightStickTouch, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Stick/Touch", "Whether the right stick is currently touched");
        CreateParameter<float>(OpenVRControllerStatisticsParameter.RightIndex, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Finger/Index", "The touch value of your right index finger");
        CreateParameter<float>(OpenVRControllerStatisticsParameter.RightMiddle, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Finger/Middle", "The touch value of your right middle finger");
        CreateParameter<float>(OpenVRControllerStatisticsParameter.RightRing, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Finger/Ring", "The touch value of your right ring finger");
        CreateParameter<float>(OpenVRControllerStatisticsParameter.RightPinky, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Input/Finger/Pinky", "The touch value of your right pinky finger");
    }

    protected override Task OnUpdate()
    {
        if (!OpenVrInterface.HasInitialised) return Task.CompletedTask;

        if (OpenVrInterface.IsLeftControllerConnected())
        {
            SendParameter(OpenVRControllerStatisticsParameter.LeftATouch, OpenVrInterface.LeftController.ATouched);
            SendParameter(OpenVRControllerStatisticsParameter.LeftBTouch, OpenVrInterface.LeftController.BTouched);
            SendParameter(OpenVRControllerStatisticsParameter.LeftPadTouch, OpenVrInterface.LeftController.PadTouched);
            SendParameter(OpenVRControllerStatisticsParameter.LeftStickTouch, OpenVrInterface.LeftController.StickTouched);
            SendParameter(OpenVRControllerStatisticsParameter.LeftIndex, OpenVrInterface.LeftController.IndexFinger);
            SendParameter(OpenVRControllerStatisticsParameter.LeftMiddle, OpenVrInterface.LeftController.MiddleFinger);
            SendParameter(OpenVRControllerStatisticsParameter.LeftRing, OpenVrInterface.LeftController.RingFinger);
            SendParameter(OpenVRControllerStatisticsParameter.LeftPinky, OpenVrInterface.LeftController.PinkyFinger);
        }

        if (OpenVrInterface.IsRightControllerConnected())
        {
            SendParameter(OpenVRControllerStatisticsParameter.RightATouch, OpenVrInterface.RightController.ATouched);
            SendParameter(OpenVRControllerStatisticsParameter.RightBTouch, OpenVrInterface.RightController.BTouched);
            SendParameter(OpenVRControllerStatisticsParameter.RightPadTouch, OpenVrInterface.RightController.PadTouched);
            SendParameter(OpenVRControllerStatisticsParameter.RightStickTouch, OpenVrInterface.RightController.StickTouched);
            SendParameter(OpenVRControllerStatisticsParameter.RightIndex, OpenVrInterface.RightController.IndexFinger);
            SendParameter(OpenVRControllerStatisticsParameter.RightMiddle, OpenVrInterface.RightController.MiddleFinger);
            SendParameter(OpenVRControllerStatisticsParameter.RightRing, OpenVrInterface.RightController.RingFinger);
            SendParameter(OpenVRControllerStatisticsParameter.RightPinky, OpenVrInterface.RightController.PinkyFinger);
        }

        return Task.CompletedTask;
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
