// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using Valve.VR;
using VRCOSC.Game.Modules.Util;

namespace VRCOSC.Game.Modules.Modules.SteamVR;

public class OpenVRModule : Module
{
    public override string Title => "OpenVR";
    public override string Description => "Gets stats from your OpenVR (SteamVR) session";
    public override string Author => "VolcanicArts";
    public override ModuleType ModuleType => ModuleType.General;
    protected override int DeltaUpdate => 5000;

    protected override void CreateAttributes()
    {
        CreateOutgoingParameter<float>(OpenVROutgoingParameter.HMD_Battery, "The battery percentage normalised of your headset", "VRCOSC/OpenVR/HMD/Battery");

        CreateOutgoingParameter<float>(OpenVROutgoingParameter.LeftController_Battery, "The battery percentage normalised of your left controller", "VRCOSC/OpenVR/LeftController/Battery");
        CreateOutgoingParameter<float>(OpenVROutgoingParameter.RightController_Battery, "The battery percentage normalised of your right controller", "VRCOSC/OpenVR/RightController/Battery");

        CreateOutgoingParameter<float>(OpenVROutgoingParameter.Tracker1_Battery, "The battery percentage normalised of tracker 1", "VRCOSC/OpenVR/Trackers/1/Battery");
        CreateOutgoingParameter<float>(OpenVROutgoingParameter.Tracker2_Battery, "The battery percentage normalised of tracker 2", "VRCOSC/OpenVR/Trackers/2/Battery");
        CreateOutgoingParameter<float>(OpenVROutgoingParameter.Tracker3_Battery, "The battery percentage normalised of tracker 3", "VRCOSC/OpenVR/Trackers/3/Battery");
        CreateOutgoingParameter<float>(OpenVROutgoingParameter.Tracker4_Battery, "The battery percentage normalised of tracker 4", "VRCOSC/OpenVR/Trackers/4/Battery");
        CreateOutgoingParameter<float>(OpenVROutgoingParameter.Tracker5_Battery, "The battery percentage normalised of tracker 5", "VRCOSC/OpenVR/Trackers/5/Battery");
        CreateOutgoingParameter<float>(OpenVROutgoingParameter.Tracker6_Battery, "The battery percentage normalised of tracker 6", "VRCOSC/OpenVR/Trackers/6/Battery");
        CreateOutgoingParameter<float>(OpenVROutgoingParameter.Tracker7_Battery, "The battery percentage normalised of tracker 7", "VRCOSC/OpenVR/Trackers/7/Battery");
        CreateOutgoingParameter<float>(OpenVROutgoingParameter.Tracker8_Battery, "The battery percentage normalised of tracker 8", "VRCOSC/OpenVR/Trackers/8/Battery");
    }

    protected override void OnStart()
    {
        OpenVrInterface.Init();
    }

    protected override void OnUpdate()
    {
        var battery = OpenVrInterface.GetHMDBatteryPercentage();
        if (battery is not null) SendParameter(OpenVROutgoingParameter.HMD_Battery, (float)battery);

        var batteryLeft = OpenVrInterface.GetLeftControllerBatteryPercentage();
        var batteryRight = OpenVrInterface.GetRightControllerBatteryPercentage();

        if (batteryLeft is not null) SendParameter(OpenVROutgoingParameter.LeftController_Battery, (float)batteryLeft);
        if (batteryRight is not null) SendParameter(OpenVROutgoingParameter.RightController_Battery, (float)batteryRight);

        var trackerBatteries = OpenVrInterface.GetTrackersBatteryPercentages().ToList();

        for (int i = 0; i < trackerBatteries.Count; i++)
        {
            SendParameter(OpenVROutgoingParameter.Tracker1_Battery + i, trackerBatteries[i]);
        }
    }

    protected override void OnStop()
    {
        OpenVR.Shutdown();
    }

    private enum OpenVROutgoingParameter
    {
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
        Tracker8_Battery
    }
}
