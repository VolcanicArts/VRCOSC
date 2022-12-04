// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.Game.Modules.Modules.OpenVR;

public class OpenVRModule : Module
{
    private const int max_tracker_count = 8;

    public override string Title => "OpenVR";
    public override string Description => "Gets statistics from your OpenVR (SteamVR) session";
    public override string Author => "VolcanicArts";
    public override ModuleType ModuleType => ModuleType.General;
    protected override int DeltaUpdate => 5000;

    protected override void CreateAttributes()
    {
        CreateParameter<bool>(OpenVrParameter.HMD_Present, ParameterMode.Write, "VRCOSC/OpenVR/HMD/Present", "Whether your HMD is connected");
        CreateParameter<float>(OpenVrParameter.HMD_Battery, ParameterMode.Write, "VRCOSC/OpenVR/HMD/Battery", "The battery percentage normalised of your headset");
        CreateParameter<bool>(OpenVrParameter.HMD_Charging, ParameterMode.Write, "VRCOSC/OpenVR/HMD/Charging", "The charge state of your headset");

        CreateParameter<bool>(OpenVrParameter.LeftController_Present, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Present", "Whether your left controller is connected");
        CreateParameter<float>(OpenVrParameter.LeftController_Battery, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Battery", "The battery percentage normalised of your left controller");
        CreateParameter<bool>(OpenVrParameter.LeftController_Charging, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Charging", "The charge state of your left controller");
        CreateParameter<bool>(OpenVrParameter.RightController_Present, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Present", "Whether your right controller is connected");
        CreateParameter<float>(OpenVrParameter.RightController_Battery, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Battery", "The battery percentage normalised of your right controller");
        CreateParameter<bool>(OpenVrParameter.RightController_Charging, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Charging", "The charge state of your right controller");

        for (int i = 0; i < max_tracker_count; i++)
        {
            CreateParameter<bool>(OpenVrParameter.Tracker1_Present + i, ParameterMode.Write, $"VRCOSC/OpenVR/Trackers/{i + 1}/Present", $"Whether tracker {i + 1} is connected");
            CreateParameter<float>(OpenVrParameter.Tracker1_Battery + i, ParameterMode.Write, $"VRCOSC/OpenVR/Trackers/{i + 1}/Battery", $"The battery percentage normalised (0-1) of tracker {i + 1}");
            CreateParameter<bool>(OpenVrParameter.Tracker1_Charging + i, ParameterMode.Write, $"VRCOSC/OpenVR/Trackers/{i + 1}/Charging", $"Whether tracker {i + 1} is currently charging");
        }
    }

    protected override Task OnUpdate()
    {
        if (!OpenVrInterface.HasSession) return Task.CompletedTask;

        handleHmd();
        handleControllers();
        handleTrackers();

        return Task.CompletedTask;
    }

    private void handleHmd()
    {
        SendParameter(OpenVrParameter.HMD_Present, OpenVrInterface.IsHmdPresent());

        if (OpenVrInterface.IsHmdPresent())
        {
            if (OpenVrInterface.CanHmdProvideBatteryData())
            {
                SendParameter(OpenVrParameter.HMD_Battery, OpenVrInterface.GetHmdBatteryPercentage());
                SendParameter(OpenVrParameter.HMD_Charging, OpenVrInterface.IsHmdCharging());
            }
        }
    }

    private void handleControllers()
    {
        SendParameter(OpenVrParameter.LeftController_Present, OpenVrInterface.IsLeftControllerPresent());

        if (OpenVrInterface.IsLeftControllerPresent())
        {
            SendParameter(OpenVrParameter.LeftController_Battery, OpenVrInterface.GetLeftControllerBatteryPercentage());
            SendParameter(OpenVrParameter.LeftController_Charging, OpenVrInterface.IsLeftControllerCharging());
        }

        SendParameter(OpenVrParameter.RightController_Present, OpenVrInterface.IsRightControllerPresent());

        if (OpenVrInterface.IsRightControllerPresent())
        {
            SendParameter(OpenVrParameter.RightController_Battery, OpenVrInterface.GetRightControllerBatteryPercentage());
            SendParameter(OpenVrParameter.RightController_Charging, OpenVrInterface.IsRightControllerCharging());
        }
    }

    private void handleTrackers()
    {
        for (int i = 0; i < max_tracker_count; i++)
        {
            SendParameter(OpenVrParameter.Tracker1_Present + i, OpenVrInterface.IsTrackerPresent(i));

            if (OpenVrInterface.IsTrackerPresent(i))
            {
                SendParameter(OpenVrParameter.Tracker1_Battery + i, OpenVrInterface.GetTrackerBatteryPercentage(i));
                SendParameter(OpenVrParameter.Tracker1_Charging + i, OpenVrInterface.IsTrackerCharging(i));
            }
        }
    }

    private enum OpenVrParameter
    {
        HMD_Present,
        LeftController_Present,
        RightController_Present,
        Tracker1_Present,
        Tracker2_Present,
        Tracker3_Present,
        Tracker4_Present,
        Tracker5_Present,
        Tracker6_Present,
        Tracker7_Present,
        Tracker8_Present,
        HMD_Battery,
        LeftController_Battery,
        RightController_Battery,
        Tracker1_Battery,
        Tracker2_Battery,
        Tracker3_Battery,
        Tracker4_Battery,
        Tracker5_Battery,
        Tracker6_Battery,
        Tracker7_Battery,
        Tracker8_Battery,
        HMD_Charging,
        LeftController_Charging,
        RightController_Charging,
        Tracker1_Charging,
        Tracker2_Charging,
        Tracker3_Charging,
        Tracker4_Charging,
        Tracker5_Charging,
        Tracker6_Charging,
        Tracker7_Charging,
        Tracker8_Charging
    }
}
