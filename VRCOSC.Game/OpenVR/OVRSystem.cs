// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using Valve.VR;
using VRCOSC.Game.OpenVR.Device;

namespace VRCOSC.Game.OpenVR;

public class OVRSystem
{
    public const int MAX_TRACKER_COUNT = 8;

    public HMD HMD { get; private set; } = null!;
    public Controller LeftController { get; private set; } = null!;
    public Controller RightController { get; private set; } = null!;
    public List<Tracker> Trackers = new();
    public float FPS => 1000.0f / OVRHelper.GetFrameTimeMilli();
    public bool DashboardOpen => OVRHelper.IsDashboardVisible();

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

        for (int i = 0; i < Math.Min(MAX_TRACKER_COUNT, trackerIds.Count); i++)
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
    private static uint getLeftControllerIndex() => OVRHelper.GetLeftControllerId();
    private static uint getRightControllerIndex() => OVRHelper.GetRightControllerId();
    private static IEnumerable<uint> getTrackerIndexes() => OVRHelper.GetIndexesForTrackedDeviceClass(ETrackedDeviceClass.GenericTracker);
}
