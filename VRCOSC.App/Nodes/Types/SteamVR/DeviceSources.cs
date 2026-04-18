// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using VRCOSC.App.OpenVR.Device;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.Nodes.Types.SteamVR;

public abstract class SteamVRDeviceSourceNode<T>(Func<T> func) : ValueSourceNode<T>(func, "Device") where T : TrackedDevice?;

[Node("HMD", "SteamVR/Devices")]
public sealed class SteamVRHMDSourceNode() : SteamVRDeviceSourceNode<HMD?>(() => AppManager.GetInstance().OpenVRManager.GetHMD());

[Node("Left Controller", "SteamVR/Devices")]
public sealed class SteamVRLeftControllerSourceNode() : SteamVRDeviceSourceNode<Controller?>(() => AppManager.GetInstance().OpenVRManager.GetLeftController());

[Node("Right Controller", "SteamVR/Devices")]
public sealed class SteamVRRightControllerSourceNode() : SteamVRDeviceSourceNode<Controller?>(() => AppManager.GetInstance().OpenVRManager.GetRightController());

[Node("Chest Tracker", "SteamVR/Devices")]
public sealed class SteamVRChestSourceNode() : SteamVRDeviceSourceNode<TrackedDevice?>(() => AppManager.GetInstance().OpenVRManager.GetTrackedDevice(DeviceRole.Chest));

[Node("Waist Tracker", "SteamVR/Devices")]
public sealed class SteamVRWaistSourceNode() : SteamVRDeviceSourceNode<TrackedDevice?>(() => AppManager.GetInstance().OpenVRManager.GetTrackedDevice(DeviceRole.Waist));

[Node("Left Elbow Tracker", "SteamVR/Devices")]
public sealed class SteamVRLeftElbowSourceNode() : SteamVRDeviceSourceNode<TrackedDevice?>(() => AppManager.GetInstance().OpenVRManager.GetTrackedDevice(DeviceRole.LeftElbow));

[Node("Right Elbow Tracker", "SteamVR/Devices")]
public sealed class SteamVRRightElbowSourceNode() : SteamVRDeviceSourceNode<TrackedDevice?>(() => AppManager.GetInstance().OpenVRManager.GetTrackedDevice(DeviceRole.RightElbow));

[Node("Left Knee Tracker", "SteamVR/Devices")]
public sealed class SteamVRLeftKneeSourceNode() : SteamVRDeviceSourceNode<TrackedDevice?>(() => AppManager.GetInstance().OpenVRManager.GetTrackedDevice(DeviceRole.LeftKnee));

[Node("Right Knee Tracker", "SteamVR/Devices")]
public sealed class SteamVRRightKneeSourceNode() : SteamVRDeviceSourceNode<TrackedDevice?>(() => AppManager.GetInstance().OpenVRManager.GetTrackedDevice(DeviceRole.RightKnee));

[Node("Left Foot Tracker", "SteamVR/Devices")]
public sealed class SteamVRLeftFootSourceNode() : SteamVRDeviceSourceNode<TrackedDevice?>(() => AppManager.GetInstance().OpenVRManager.GetTrackedDevice(DeviceRole.LeftFoot));

[Node("Right Foot Tracker", "SteamVR/Devices")]
public sealed class SteamVRRightFootSourceNode() : SteamVRDeviceSourceNode<TrackedDevice?>(() => AppManager.GetInstance().OpenVRManager.GetTrackedDevice(DeviceRole.RightFoot));

[Node("Tracked Device", "SteamVR/Devices")]
public sealed class SteamVRTrackedDeviceSourceNode : UpdateNode<TrackedDevice?>, IHasTextProperty
{
    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    public ValueOutput<TrackedDevice?> Device = new();

    protected override Task Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OpenVRManager.GetTrackedDevice(Text), c);
        return Task.CompletedTask;
    }

    protected override Task<TrackedDevice?> GetValue(PulseContext c) => Task.FromResult(AppManager.GetInstance().OpenVRManager.GetTrackedDevice(Text));
}