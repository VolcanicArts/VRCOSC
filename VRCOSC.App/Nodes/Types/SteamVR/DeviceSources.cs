// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.OVR;
using VRCOSC.App.SDK.OVR.Device;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.Nodes.Types.SteamVR;

[Node("HMD", "SteamVR/Devices")]
public sealed class SteamVRHMDSourceNode : UpdateNode<HMD?>
{
    public ValueOutput<HMD?> Device = new();

    protected override void Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OVRClient.GetHMD(), c);
    }

    protected override HMD? GetValue(PulseContext c) => AppManager.GetInstance().OVRClient.GetHMD();
}

[Node("Left Controller", "SteamVR/Devices")]
public sealed class SteamVRLeftControllerSourceNode : UpdateNode<Controller?>
{
    public ValueOutput<Controller?> Device = new();

    protected override void Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OVRClient.GetLeftController(), c);
    }

    protected override Controller? GetValue(PulseContext c) => AppManager.GetInstance().OVRClient.GetLeftController();
}

[Node("Right Controller", "SteamVR/Devices")]
public sealed class SteamVRRightControllerSourceNode : UpdateNode<Controller?>
{
    public ValueOutput<Controller?> Device = new();

    protected override void Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OVRClient.GetRightController(), c);
    }

    protected override Controller? GetValue(PulseContext c) => AppManager.GetInstance().OVRClient.GetRightController();
}

[Node("Chest Tracker", "SteamVR/Devices")]
public sealed class SteamVRChestSourceNode : UpdateNode<TrackedDevice?>
{
    public ValueOutput<TrackedDevice?> Device = new();

    protected override void Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OVRClient.GetTrackedDevice(DeviceRole.Chest), c);
    }

    protected override TrackedDevice? GetValue(PulseContext c) => AppManager.GetInstance().OVRClient.GetTrackedDevice(DeviceRole.Chest);
}

[Node("Waist Tracker", "SteamVR/Devices")]
public sealed class SteamVRWaistSourceNode : UpdateNode<TrackedDevice?>
{
    public ValueOutput<TrackedDevice?> Device = new();

    protected override void Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OVRClient.GetTrackedDevice(DeviceRole.Waist), c);
    }

    protected override TrackedDevice? GetValue(PulseContext c) => AppManager.GetInstance().OVRClient.GetTrackedDevice(DeviceRole.Waist);
}

[Node("Left Elbow Tracker", "SteamVR/Devices")]
public sealed class SteamVRLeftElbowSourceNode : UpdateNode<TrackedDevice?>
{
    public ValueOutput<TrackedDevice?> Device = new();

    protected override void Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OVRClient.GetTrackedDevice(DeviceRole.LeftElbow), c);
    }

    protected override TrackedDevice? GetValue(PulseContext c) => AppManager.GetInstance().OVRClient.GetTrackedDevice(DeviceRole.LeftElbow);
}

[Node("Right Elbow Tracker", "SteamVR/Devices")]
public sealed class SteamVRRightElbowSourceNode : UpdateNode<TrackedDevice?>
{
    public ValueOutput<TrackedDevice?> Device = new();

    protected override void Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OVRClient.GetTrackedDevice(DeviceRole.RightElbow), c);
    }

    protected override TrackedDevice? GetValue(PulseContext c) => AppManager.GetInstance().OVRClient.GetTrackedDevice(DeviceRole.RightElbow);
}

[Node("Left Knee Tracker", "SteamVR/Devices")]
public sealed class SteamVRLeftKneeSourceNode : UpdateNode<TrackedDevice?>
{
    public ValueOutput<TrackedDevice?> Device = new();

    protected override void Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OVRClient.GetTrackedDevice(DeviceRole.LeftKnee), c);
    }

    protected override TrackedDevice? GetValue(PulseContext c) => AppManager.GetInstance().OVRClient.GetTrackedDevice(DeviceRole.LeftKnee);
}

[Node("Right Knee Tracker", "SteamVR/Devices")]
public sealed class SteamVRRightKneeSourceNode : UpdateNode<TrackedDevice?>
{
    public ValueOutput<TrackedDevice?> Device = new();

    protected override void Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OVRClient.GetTrackedDevice(DeviceRole.RightKnee), c);
    }

    protected override TrackedDevice? GetValue(PulseContext c) => AppManager.GetInstance().OVRClient.GetTrackedDevice(DeviceRole.RightKnee);
}

[Node("Left Foot Tracker", "SteamVR/Devices")]
public sealed class SteamVRLeftFootSourceNode : UpdateNode<TrackedDevice?>
{
    public ValueOutput<TrackedDevice?> Device = new();

    protected override void Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OVRClient.GetTrackedDevice(DeviceRole.LeftFoot), c);
    }

    protected override TrackedDevice? GetValue(PulseContext c) => AppManager.GetInstance().OVRClient.GetTrackedDevice(DeviceRole.LeftFoot);
}

[Node("Right Foot Tracker", "SteamVR/Devices")]
public sealed class SteamVRRightFootSourceNode : UpdateNode<TrackedDevice?>
{
    public ValueOutput<TrackedDevice?> Device = new();

    protected override void Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OVRClient.GetTrackedDevice(DeviceRole.RightFoot), c);
    }

    protected override TrackedDevice? GetValue(PulseContext c) => AppManager.GetInstance().OVRClient.GetTrackedDevice(DeviceRole.RightFoot);
}

[Node("Tracked Device", "SteamVR/Devices")]
public sealed class SteamVRTrackedDeviceSourceNode : UpdateNode<TrackedDevice?>, IHasTextProperty
{
    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    public ValueOutput<TrackedDevice?> Device = new();

    protected override void Process(PulseContext c)
    {
        var device = AppManager.GetInstance().OVRClient.GetTrackedDevice(Text);
        Device.Write(device, c);
    }

    protected override TrackedDevice? GetValue(PulseContext c) => AppManager.GetInstance().OVRClient.GetTrackedDevice(Text);
}