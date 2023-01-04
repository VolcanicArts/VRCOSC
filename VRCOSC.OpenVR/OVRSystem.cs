// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Valve.VR;
using VRCOSC.OpenVR.Device;

namespace VRCOSC.OpenVR;

public class OVRSystem
{
    public const int MAX_TRACKER_COUNT = 8;

    public HMD HMD { get; private set; } = null!;
    public Controller LeftController { get; private set; } = null!;
    public Controller RightController { get; private set; } = null!;
    public List<Tracker> Trackers = new();
    public float FPS => 1 / OVRHelper.GetFrameTimeMilli();

    public void Init()
    {
        HMD = new HMD();
        LeftController = new Controller();
        RightController = new Controller();

        Trackers.Clear();

        for (int i = 0; i < MAX_TRACKER_COUNT; i++)
        {
            Trackers.Add(new Tracker());
        }
    }

    public void Update()
    {
        auditDevices();
        updateDevices();
    }

    private void auditDevices()
    {
        HMD.BindTo(getHmdIndex());
        LeftController.BindTo(getLeftControllerIndex());
        RightController.BindTo(getRightControllerIndex());

        var trackerIds = getTrackerIndexes().ToList();

        for (int i = 0; i < MAX_TRACKER_COUNT; i++)
        {
            Trackers[i].BindTo(trackerIds[i]);
        }
    }

    private void updateDevices()
    {
        HMD.Update();
        LeftController.Update();
        RightController.Update();
        Trackers.ForEach(tracker => tracker.Update());
    }

    private static uint getHmdIndex() => OVRHelper.GetIndexForTrackedDeviceClass(ETrackedDeviceClass.HMD);
    private static uint getLeftControllerIndex() => OVRHelper.GetControllerIdFromHint(@"left");
    private static uint getRightControllerIndex() => OVRHelper.GetControllerIdFromHint(@"right");
    private static IEnumerable<uint> getTrackerIndexes() => OVRHelper.GetIndexesForTrackedDeviceClass(ETrackedDeviceClass.GenericTracker);
}
