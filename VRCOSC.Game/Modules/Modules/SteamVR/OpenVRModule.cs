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
        CreateOutgoingParameter(OpenVROutgoingParameter.HMD_Battery, "HMD Battery", "The battery percentage normalised of your headset", "/avatar/parameters/VRCOSC/OpenVR/HMD/Battery");

        CreateOutgoingParameter(OpenVROutgoingParameter.LeftController_Battery, "Left Controller Battery", "The battery percentage normalised of your left controller", "/avatar/parameters/VRCOSC/OpenVR/LeftController/Battery");
        CreateOutgoingParameter(OpenVROutgoingParameter.RightController_Battery, "Right Controller Battery", "The battery percentage normalised of your right controller", "/avatar/parameters/VRCOSC/OpenVR/RightController/Battery");

        CreateOutgoingParameter(OpenVROutgoingParameter.Tracker1_Battery, "Tracker 1 Battery", "The battery percentage normalised of tracker 1", "/avatar/parameters/VRCOSC/OpenVR/Trackers/1/Battery");
        CreateOutgoingParameter(OpenVROutgoingParameter.Tracker2_Battery, "Tracker 2 Battery", "The battery percentage normalised of tracker 2", "/avatar/parameters/VRCOSC/OpenVR/Trackers/2/Battery");
        CreateOutgoingParameter(OpenVROutgoingParameter.Tracker3_Battery, "Tracker 3 Battery", "The battery percentage normalised of tracker 3", "/avatar/parameters/VRCOSC/OpenVR/Trackers/3/Battery");
        CreateOutgoingParameter(OpenVROutgoingParameter.Tracker4_Battery, "Tracker 4 Battery", "The battery percentage normalised of tracker 4", "/avatar/parameters/VRCOSC/OpenVR/Trackers/4/Battery");
        CreateOutgoingParameter(OpenVROutgoingParameter.Tracker5_Battery, "Tracker 5 Battery", "The battery percentage normalised of tracker 5", "/avatar/parameters/VRCOSC/OpenVR/Trackers/5/Battery");
        CreateOutgoingParameter(OpenVROutgoingParameter.Tracker6_Battery, "Tracker 6 Battery", "The battery percentage normalised of tracker 6", "/avatar/parameters/VRCOSC/OpenVR/Trackers/6/Battery");
        CreateOutgoingParameter(OpenVROutgoingParameter.Tracker7_Battery, "Tracker 7 Battery", "The battery percentage normalised of tracker 7", "/avatar/parameters/VRCOSC/OpenVR/Trackers/7/Battery");
        CreateOutgoingParameter(OpenVROutgoingParameter.Tracker8_Battery, "Tracker 8 Battery", "The battery percentage normalised of tracker 8", "/avatar/parameters/VRCOSC/OpenVR/Trackers/8/Battery");
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
