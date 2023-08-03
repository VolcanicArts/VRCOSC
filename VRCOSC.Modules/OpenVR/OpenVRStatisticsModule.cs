// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Avatar;
using VRCOSC.Game.OpenVR;

namespace VRCOSC.Modules.OpenVR;

[ModuleTitle("OpenVR Statistics")]
[ModuleDescription("Gets statistics from your OpenVR (SteamVR) session")]
[ModuleAuthor("VolcanicArts", "https://github.com/VolcanicArts", "https://avatars.githubusercontent.com/u/29819296?v=4")]
[ModuleGroup(ModuleType.OpenVR)]
[ModuleInfo("The tracker order in Unity is the order you must turn your trackers on IRL")]
public class OpenVRStatisticsModule : ChatBoxModule
{
    protected override void CreateAttributes()
    {
        CreateParameter<float>(OpenVrParameter.FPS, ParameterMode.Write, "VRCOSC/OpenVR/FPS", "FPS", "The current FPS normalised to 240 FPS");

        CreateParameter<bool>(OpenVrParameter.HMD_Connected, ParameterMode.Write, "VRCOSC/OpenVR/HMD/Connected", "HMD Connected", "Whether your headset is connected");
        CreateParameter<float>(OpenVrParameter.HMD_Battery, ParameterMode.Write, "VRCOSC/OpenVR/HMD/Battery", "HMD Battery", "The battery percentage normalised of your headset");
        CreateParameter<bool>(OpenVrParameter.HMD_Charging, ParameterMode.Write, "VRCOSC/OpenVR/HMD/Charging", "HMD Charging", "The charge state of your headset");

        CreateParameter<bool>(OpenVrParameter.LeftController_Connected, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Connected", "Left Controller Connected", "Whether your left controller is connected");
        CreateParameter<float>(OpenVrParameter.LeftController_Battery, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Battery", "Left Controller Battery", "The battery percentage normalised of your left controller");
        CreateParameter<bool>(OpenVrParameter.LeftController_Charging, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Charging", "Left Controller Charging", "The charge state of your left controller");
        CreateParameter<bool>(OpenVrParameter.RightController_Connected, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Connected", "Right Controller Connected", "Whether your right controller is connected");
        CreateParameter<float>(OpenVrParameter.RightController_Battery, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Battery", "Right Controller Battery", "The battery percentage normalised of your right controller");
        CreateParameter<bool>(OpenVrParameter.RightController_Charging, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Charging", "Right Controller Charging", "The charge state of your right controller");

        for (int i = 0; i < OVRSystem.MAX_TRACKER_COUNT; i++)
        {
            CreateParameter<bool>(OpenVrParameter.Tracker1_Connected + i, ParameterMode.Write, $"VRCOSC/OpenVR/Trackers/{i + 1}/Connected", $"Tracker {i + 1} Connected", $"Whether tracker {i + 1} is connected");
            CreateParameter<float>(OpenVrParameter.Tracker1_Battery + i, ParameterMode.Write, $"VRCOSC/OpenVR/Trackers/{i + 1}/Battery", $"Tracker {i + 1} Battery", $"The battery percentage normalised (0-1) of tracker {i + 1}");
            CreateParameter<bool>(OpenVrParameter.Tracker1_Charging + i, ParameterMode.Write, $"VRCOSC/OpenVR/Trackers/{i + 1}/Charging", $"Tracker {i + 1} Charging", $"Whether tracker {i + 1} is currently charging");
        }

        CreateVariable(OpenVrVariable.FPS, @"FPS", @"fps");
        CreateVariable(OpenVrVariable.HMDBattery, @"HMD Battery (%)", @"hmdbattery");
        CreateVariable(OpenVrVariable.LeftControllerBattery, @"Left Controller Battery (%)", @"leftcontrollerbattery");
        CreateVariable(OpenVrVariable.RightControllerBattery, @"Right Controller Battery (%)", @"rightcontrollerbattery");
        CreateVariable(OpenVrVariable.AverageTrackerBattery, @"Average Tracker Battery (%)", @"averagetrackerbattery");

        CreateState(OpenVrState.Default, "Default", $@"HMD: {GetVariableFormat(OpenVrVariable.HMDBattery)}/vLC: {GetVariableFormat(OpenVrVariable.LeftControllerBattery)}/vRC: {GetVariableFormat(OpenVrVariable.RightControllerBattery)}/vTrackers: {GetVariableFormat(OpenVrVariable.AverageTrackerBattery)}");
    }

    protected override void OnModuleStart()
    {
        ChangeStateTo(OpenVrState.Default);
    }

    [ModuleUpdate(ModuleUpdateMode.Custom, true, 5000)]
    private void updateVariablesAndParameters()
    {
        if (OVRClient.HasInitialised)
        {
            SendParameter(OpenVrParameter.FPS, OVRClient.System.FPS / 240.0f);
            updateHmd();
            updateLeftController();
            updateRightController();
            updateTrackers();

            var activeTrackers = OVRClient.Trackers.Where(tracker => tracker.IsConnected).ToList();

            var trackerBatteryAverage = 0f;

            if (activeTrackers.Any())
            {
                trackerBatteryAverage = activeTrackers.Sum(tracker => tracker.BatteryPercentage) / activeTrackers.Count;
            }

            SetVariableValue(OpenVrVariable.FPS, OVRClient.System.FPS.ToString("##0"));
            SetVariableValue(OpenVrVariable.HMDBattery, ((int)(OVRClient.HMD.BatteryPercentage * 100)).ToString("##0"));
            SetVariableValue(OpenVrVariable.LeftControllerBattery, ((int)(OVRClient.LeftController.BatteryPercentage * 100)).ToString("##0"));
            SetVariableValue(OpenVrVariable.RightControllerBattery, ((int)(OVRClient.RightController.BatteryPercentage * 100)).ToString("##0"));
            SetVariableValue(OpenVrVariable.AverageTrackerBattery, ((int)(trackerBatteryAverage * 100)).ToString("##0"));
        }
        else
        {
            SetVariableValue(OpenVrVariable.FPS, "0");
            SetVariableValue(OpenVrVariable.HMDBattery, "0");
            SetVariableValue(OpenVrVariable.LeftControllerBattery, "0");
            SetVariableValue(OpenVrVariable.RightControllerBattery, "0");
            SetVariableValue(OpenVrVariable.AverageTrackerBattery, "0");

            SendParameter(OpenVrParameter.HMD_Connected, false);
            SendParameter(OpenVrParameter.HMD_Battery, 0);
            SendParameter(OpenVrParameter.HMD_Charging, false);

            SendParameter(OpenVrParameter.LeftController_Connected, false);
            SendParameter(OpenVrParameter.LeftController_Battery, 0);
            SendParameter(OpenVrParameter.LeftController_Charging, false);

            SendParameter(OpenVrParameter.RightController_Connected, false);
            SendParameter(OpenVrParameter.RightController_Battery, 0);
            SendParameter(OpenVrParameter.RightController_Charging, false);

            for (int i = 0; i < OVRSystem.MAX_TRACKER_COUNT; i++)
            {
                SendParameter(OpenVrParameter.Tracker1_Connected + i, false);
                SendParameter(OpenVrParameter.Tracker1_Battery + i, 0);
                SendParameter(OpenVrParameter.Tracker1_Charging + i, false);
            }
        }
    }

    private void updateHmd()
    {
        SendParameter(OpenVrParameter.HMD_Connected, OVRClient.HMD.IsConnected);

        if (OVRClient.HMD.IsConnected && OVRClient.HMD.ProvidesBatteryStatus)
        {
            SendParameter(OpenVrParameter.HMD_Battery, OVRClient.HMD.BatteryPercentage);
            SendParameter(OpenVrParameter.HMD_Charging, OVRClient.HMD.IsCharging);
        }
    }

    private void updateLeftController()
    {
        SendParameter(OpenVrParameter.LeftController_Connected, OVRClient.LeftController.IsConnected);

        if (OVRClient.LeftController.IsConnected && OVRClient.LeftController.ProvidesBatteryStatus)
        {
            SendParameter(OpenVrParameter.LeftController_Battery, OVRClient.LeftController.BatteryPercentage);
            SendParameter(OpenVrParameter.LeftController_Charging, OVRClient.LeftController.IsCharging);
        }
        else
        {
            SendParameter(OpenVrParameter.LeftController_Battery, 0);
            SendParameter(OpenVrParameter.LeftController_Charging, false);
        }
    }

    private void updateRightController()
    {
        SendParameter(OpenVrParameter.RightController_Connected, OVRClient.RightController.IsConnected);

        if (OVRClient.RightController.IsConnected && OVRClient.RightController.ProvidesBatteryStatus)
        {
            SendParameter(OpenVrParameter.RightController_Battery, OVRClient.RightController.BatteryPercentage);
            SendParameter(OpenVrParameter.RightController_Charging, OVRClient.RightController.IsCharging);
        }
        else
        {
            SendParameter(OpenVrParameter.RightController_Battery, 0);
            SendParameter(OpenVrParameter.RightController_Charging, false);
        }
    }

    private void updateTrackers()
    {
        var trackers = OVRClient.Trackers.ToList();

        for (int i = 0; i < OVRSystem.MAX_TRACKER_COUNT; i++)
        {
            var tracker = trackers[i];

            SendParameter(OpenVrParameter.Tracker1_Connected + i, tracker.IsConnected);

            if (tracker.IsConnected && tracker.ProvidesBatteryStatus)
            {
                SendParameter(OpenVrParameter.Tracker1_Battery + i, tracker.BatteryPercentage);
                SendParameter(OpenVrParameter.Tracker1_Charging + i, tracker.IsCharging);
            }
            else
            {
                SendParameter(OpenVrParameter.Tracker1_Battery + i, 0);
                SendParameter(OpenVrParameter.Tracker1_Charging + i, false);
            }
        }
    }

    private enum OpenVrParameter
    {
        FPS,
        HMD_Connected,
        LeftController_Connected,
        RightController_Connected,
        Tracker1_Connected,
        Tracker2_Connected,
        Tracker3_Connected,
        Tracker4_Connected,
        Tracker5_Connected,
        Tracker6_Connected,
        Tracker7_Connected,
        Tracker8_Connected,
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

    private enum OpenVrState
    {
        Default
    }

    private enum OpenVrVariable
    {
        FPS,
        HMDBattery,
        LeftControllerBattery,
        RightControllerBattery,
        AverageTrackerBattery
    }
}
