// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using Valve.VR;
using VRCOSC.App.SDK.OVR.Device;

namespace VRCOSC.App.SDK.OVR;

public class OVRSystem
{
    public const int MAX_TRACKER_COUNT = 8;

    internal HMD HMD { get; private set; } = null!;
    internal Controller LeftController { get; private set; } = null!;
    internal Controller RightController { get; private set; } = null!;
    internal readonly List<Tracker> Trackers = new();
    public float FPS { get; private set; }

    internal void Init()
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

    internal void Update()
    {
        FPS = 1000.0f / OVRHelper.GetFrameTimeMilli();

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

    // Remove standable trackers as they're virtual
    private static IEnumerable<uint> getTrackerIndexes() => OVRHelper.GetIndexesForTrackedDeviceClass(ETrackedDeviceClass.GenericTracker).Where(index => !OVRHelper.GetStringTrackedDeviceProperty(index, ETrackedDeviceProperty.Prop_RenderModelName_String).Contains("stndbl", StringComparison.InvariantCultureIgnoreCase));
}
