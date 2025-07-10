// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.OpenVR.Device;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.Nodes.Types.SteamVR;

[Node("HMD", "SteamVR/Devices")]
public sealed class SteamVRHMDSourceNode : UpdateNode<HMD?>
{
    public ValueOutput<HMD?> Device = new();

    protected override Task Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OpenVRManager.GetHMD(), c);
        return Task.CompletedTask;
    }

    protected override HMD? GetValue(PulseContext c) => AppManager.GetInstance().OpenVRManager.GetHMD();
}

[Node("Left Controller", "SteamVR/Devices")]
public sealed class SteamVRLeftControllerSourceNode : UpdateNode<Controller?>
{
    public ValueOutput<Controller?> Device = new();

    protected override Task Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OpenVRManager.GetLeftController(), c);
        return Task.CompletedTask;
    }

    protected override Controller? GetValue(PulseContext c) => AppManager.GetInstance().OpenVRManager.GetLeftController();
}

[Node("Right Controller", "SteamVR/Devices")]
public sealed class SteamVRRightControllerSourceNode : UpdateNode<Controller?>
{
    public ValueOutput<Controller?> Device = new();

    protected override Task Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OpenVRManager.GetRightController(), c);
        return Task.CompletedTask;
    }

    protected override Controller? GetValue(PulseContext c) => AppManager.GetInstance().OpenVRManager.GetRightController();
}

[Node("Chest Tracker", "SteamVR/Devices")]
public sealed class SteamVRChestSourceNode : UpdateNode<TrackedDevice?>
{
    public ValueOutput<TrackedDevice?> Device = new();

    protected override Task Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OpenVRManager.GetTrackedDevice(DeviceRole.Chest), c);
        return Task.CompletedTask;
    }

    protected override TrackedDevice? GetValue(PulseContext c) => AppManager.GetInstance().OpenVRManager.GetTrackedDevice(DeviceRole.Chest);
}

[Node("Waist Tracker", "SteamVR/Devices")]
public sealed class SteamVRWaistSourceNode : UpdateNode<TrackedDevice?>
{
    public ValueOutput<TrackedDevice?> Device = new();

    protected override Task Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OpenVRManager.GetTrackedDevice(DeviceRole.Waist), c);
        return Task.CompletedTask;
    }

    protected override TrackedDevice? GetValue(PulseContext c) => AppManager.GetInstance().OpenVRManager.GetTrackedDevice(DeviceRole.Waist);
}

[Node("Left Elbow Tracker", "SteamVR/Devices")]
public sealed class SteamVRLeftElbowSourceNode : UpdateNode<TrackedDevice?>
{
    public ValueOutput<TrackedDevice?> Device = new();

    protected override Task Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OpenVRManager.GetTrackedDevice(DeviceRole.LeftElbow), c);
        return Task.CompletedTask;
    }

    protected override TrackedDevice? GetValue(PulseContext c) => AppManager.GetInstance().OpenVRManager.GetTrackedDevice(DeviceRole.LeftElbow);
}

[Node("Right Elbow Tracker", "SteamVR/Devices")]
public sealed class SteamVRRightElbowSourceNode : UpdateNode<TrackedDevice?>
{
    public ValueOutput<TrackedDevice?> Device = new();

    protected override Task Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OpenVRManager.GetTrackedDevice(DeviceRole.RightElbow), c);
        return Task.CompletedTask;
    }

    protected override TrackedDevice? GetValue(PulseContext c) => AppManager.GetInstance().OpenVRManager.GetTrackedDevice(DeviceRole.RightElbow);
}

[Node("Left Knee Tracker", "SteamVR/Devices")]
public sealed class SteamVRLeftKneeSourceNode : UpdateNode<TrackedDevice?>
{
    public ValueOutput<TrackedDevice?> Device = new();

    protected override Task Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OpenVRManager.GetTrackedDevice(DeviceRole.LeftKnee), c);
        return Task.CompletedTask;
    }

    protected override TrackedDevice? GetValue(PulseContext c) => AppManager.GetInstance().OpenVRManager.GetTrackedDevice(DeviceRole.LeftKnee);
}

[Node("Right Knee Tracker", "SteamVR/Devices")]
public sealed class SteamVRRightKneeSourceNode : UpdateNode<TrackedDevice?>
{
    public ValueOutput<TrackedDevice?> Device = new();

    protected override Task Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OpenVRManager.GetTrackedDevice(DeviceRole.RightKnee), c);
        return Task.CompletedTask;
    }

    protected override TrackedDevice? GetValue(PulseContext c) => AppManager.GetInstance().OpenVRManager.GetTrackedDevice(DeviceRole.RightKnee);
}

[Node("Left Foot Tracker", "SteamVR/Devices")]
public sealed class SteamVRLeftFootSourceNode : UpdateNode<TrackedDevice?>
{
    public ValueOutput<TrackedDevice?> Device = new();

    protected override Task Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OpenVRManager.GetTrackedDevice(DeviceRole.LeftFoot), c);
        return Task.CompletedTask;
    }

    protected override TrackedDevice? GetValue(PulseContext c) => AppManager.GetInstance().OpenVRManager.GetTrackedDevice(DeviceRole.LeftFoot);
}

[Node("Right Foot Tracker", "SteamVR/Devices")]
public sealed class SteamVRRightFootSourceNode : UpdateNode<TrackedDevice?>
{
    public ValueOutput<TrackedDevice?> Device = new();

    protected override Task Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OpenVRManager.GetTrackedDevice(DeviceRole.RightFoot), c);
        return Task.CompletedTask;
    }

    protected override TrackedDevice? GetValue(PulseContext c) => AppManager.GetInstance().OpenVRManager.GetTrackedDevice(DeviceRole.RightFoot);
}

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

    protected override TrackedDevice? GetValue(PulseContext c) => AppManager.GetInstance().OpenVRManager.GetTrackedDevice(Text);
}