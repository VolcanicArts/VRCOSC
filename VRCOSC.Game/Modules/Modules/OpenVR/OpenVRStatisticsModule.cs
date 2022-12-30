// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VRCOSC.OpenVR;

namespace VRCOSC.Game.Modules.Modules.OpenVR;

public partial class OpenVRStatisticsModule : ChatBoxModule
{
    public override string Title => "OpenVR Statistics";
    public override string Description => "Gets statistics from your OpenVR (SteamVR) session";
    public override string Author => "VolcanicArts";
    public override ModuleType Type => ModuleType.OpenVR;
    protected override int DeltaUpdate => 5000;
    protected override int ChatBoxPriority => 1;
    protected override bool DefaultChatBoxDisplay => false;
    protected override IEnumerable<string> ChatBoxFormatValues => new[] { "$fps$", "$hmdbattery$", "$leftcontrollerbattery$", "$rightcontrollerbattery$" };
    protected override string DefaultChatBoxFormat => "FPS: $fps$ | HMD: $hmdbattery$ | LC: $leftcontrollerbattery$ | RC: $rightcontrollerbattery$";

    protected override void CreateAttributes()
    {
        base.CreateAttributes();

        CreateParameter<float>(OpenVrParameter.FPS, ParameterMode.Write, "VRCOSC/OpenVR/FPS", "The current FPS normalised to 240 FPS");

        CreateParameter<bool>(OpenVrParameter.HMD_Connected, ParameterMode.Write, "VRCOSC/OpenVR/HMD/Connected", "Whether your HMD is connected");
        CreateParameter<float>(OpenVrParameter.HMD_Battery, ParameterMode.Write, "VRCOSC/OpenVR/HMD/Battery", "The battery percentage normalised of your headset");
        CreateParameter<bool>(OpenVrParameter.HMD_Charging, ParameterMode.Write, "VRCOSC/OpenVR/HMD/Charging", "The charge state of your headset");

        CreateParameter<bool>(OpenVrParameter.LeftController_Connected, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Connected", "Whether your left controller is connected");
        CreateParameter<float>(OpenVrParameter.LeftController_Battery, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Battery", "The battery percentage normalised of your left controller");
        CreateParameter<bool>(OpenVrParameter.LeftController_Charging, ParameterMode.Write, "VRCOSC/OpenVR/LeftController/Charging", "The charge state of your left controller");
        CreateParameter<bool>(OpenVrParameter.RightController_Connected, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Connected", "Whether your right controller is connected");
        CreateParameter<float>(OpenVrParameter.RightController_Battery, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Battery", "The battery percentage normalised of your right controller");
        CreateParameter<bool>(OpenVrParameter.RightController_Charging, ParameterMode.Write, "VRCOSC/OpenVR/RightController/Charging", "The charge state of your right controller");

        for (int i = 0; i < OVRSystem.MAX_TRACKER_COUNT; i++)
        {
            CreateParameter<bool>(OpenVrParameter.Tracker1_Connected + i, ParameterMode.Write, $"VRCOSC/OpenVR/Trackers/{i + 1}/Connected", $"Whether tracker {i + 1} is connected");
            CreateParameter<float>(OpenVrParameter.Tracker1_Battery + i, ParameterMode.Write, $"VRCOSC/OpenVR/Trackers/{i + 1}/Battery", $"The battery percentage normalised (0-1) of tracker {i + 1}");
            CreateParameter<bool>(OpenVrParameter.Tracker1_Charging + i, ParameterMode.Write, $"VRCOSC/OpenVR/Trackers/{i + 1}/Charging", $"Whether tracker {i + 1} is currently charging");
        }
    }

    protected override string? GetChatBoxText()
    {
        if (!OVRClient.HasInitialised) return null;

        return GetSetting<string>(ChatBoxSetting.ChatBoxFormat)
               .Replace("$fps$", OVRClient.System.FPS.ToString("00", CultureInfo.InvariantCulture))
               .Replace("$hmdbattery$", ((int)(OVRClient.HMD.BatteryPercentage * 100)).ToString(CultureInfo.InvariantCulture))
               .Replace("$leftcontrollerbattery$", ((int)(OVRClient.LeftController.BatteryPercentage * 100)).ToString(CultureInfo.InvariantCulture))
               .Replace("$rightcontrollerbattery$", ((int)(OVRClient.RightController.BatteryPercentage * 100)).ToString(CultureInfo.InvariantCulture));
    }

    protected override void OnModuleUpdate()
    {
        if (!OVRClient.HasInitialised) return;

        SendParameter(OpenVrParameter.FPS, OVRClient.System.FPS / 240.0f);
        handleHmd();
        handleControllers();
        handleTrackers();
    }

    private void handleHmd()
    {
        SendParameter(OpenVrParameter.HMD_Connected, OVRClient.HMD.IsConnected);

        if (OVRClient.HMD.IsConnected && OVRClient.HMD.CanProvideBatteryInfo)
        {
            SendParameter(OpenVrParameter.HMD_Battery, OVRClient.HMD.BatteryPercentage);
            SendParameter(OpenVrParameter.HMD_Charging, OVRClient.HMD.IsCharging);
        }
    }

    private void handleControllers()
    {
        SendParameter(OpenVrParameter.LeftController_Connected, OVRClient.LeftController.IsConnected);

        if (OVRClient.LeftController.IsConnected && OVRClient.LeftController.CanProvideBatteryInfo)
        {
            SendParameter(OpenVrParameter.LeftController_Battery, OVRClient.LeftController.BatteryPercentage);
            SendParameter(OpenVrParameter.LeftController_Charging, OVRClient.LeftController.IsCharging);
        }

        SendParameter(OpenVrParameter.RightController_Connected, OVRClient.RightController.IsConnected);

        if (OVRClient.RightController.IsConnected && OVRClient.RightController.CanProvideBatteryInfo)
        {
            SendParameter(OpenVrParameter.RightController_Battery, OVRClient.RightController.BatteryPercentage);
            SendParameter(OpenVrParameter.RightController_Charging, OVRClient.RightController.IsCharging);
        }
    }

    private void handleTrackers()
    {
        var trackers = OVRClient.Trackers.ToList();

        for (int i = 0; i < OVRSystem.MAX_TRACKER_COUNT; i++)
        {
            var tracker = trackers[i];

            SendParameter(OpenVrParameter.Tracker1_Connected + i, tracker.IsConnected);

            if (tracker.IsConnected && tracker.CanProvideBatteryInfo)
            {
                SendParameter(OpenVrParameter.Tracker1_Battery + i, tracker.BatteryPercentage);
                SendParameter(OpenVrParameter.Tracker1_Charging + i, tracker.IsCharging);
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
}
