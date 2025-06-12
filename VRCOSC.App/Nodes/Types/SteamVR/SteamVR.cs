// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Numerics;
using VRCOSC.App.SDK.OVR.Device;
using VRCOSC.App.Utils;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.Nodes.Types.SteamVR;

[Node("Is Dashboard Visible", "SteamVR")]
public sealed class SteamVRIsDashboardVisibleNode : UpdateNode<bool>
{
    public ValueOutput<bool> IsVisible = new("Is Visible");

    protected override void Process(PulseContext c)
    {
        IsVisible.Write(AppManager.GetInstance().OVRClient.IsDashboardVisible(), c);
    }

    protected override bool GetValue(PulseContext c) => AppManager.GetInstance().OVRClient.IsDashboardVisible();
}

[Node("Is User Present", "SteamVR")]
public sealed class SteamVRIsUserPresentNode : UpdateNode<bool>
{
    public ValueOutput<bool> IsPresent = new();

    protected override void Process(PulseContext c)
    {
        IsPresent.Write(AppManager.GetInstance().OVRClient.IsUserPresent(), c);
    }

    protected override bool GetValue(PulseContext c) => AppManager.GetInstance().OVRClient.IsUserPresent();
}

[Node("FPS", "SteamVR")]
public sealed class SteamVRFPSNode : UpdateNode<float>
{
    public ValueOutput<float> FPS = new();

    protected override void Process(PulseContext c)
    {
        FPS.Write(AppManager.GetInstance().OVRClient.FPS, c);
    }

    protected override float GetValue(PulseContext c) => AppManager.GetInstance().OVRClient.FPS;
}

[Node("Tracked Device Info", "SteamVR")]
public sealed class SteamVRTrackedDeviceInfoNode : UpdateNode<int>
{
    public ValueInput<TrackedDevice> Device = new();
    public ValueOutput<bool> IsConnected = new("Is Connected");
    public ValueOutput<bool> IsCharging = new("Is Charging");
    public ValueOutput<float> Battery = new();

    protected override void Process(PulseContext c)
    {
        var device = Device.Read(c);
        IsConnected.Write(device.IsConnected, c);
        IsCharging.Write(device.IsCharging, c);
        Battery.Write(device.BatteryPercentage, c);
    }

    protected override int GetValue(PulseContext c)
    {
        var device = Device.Read(c);
        return HashCode.Combine(device.IsConnected, device.IsCharging, device.BatteryPercentage);
    }
}

[Node("Device Transform", "SteamVR")]
public sealed class SteamVRDeviceTransformSourceNode : UpdateNode<Transform>
{
    public ValueInput<TrackedDevice> Device = new();
    public ValueOutput<Vector3> Pos = new();
    public ValueOutput<Quaternion> Rot = new();

    protected override void Process(PulseContext c)
    {
        var transform = Device.Read(c).Transform;
        Pos.Write(transform.Position, c);
        Rot.Write(transform.Rotation, c);
    }

    protected override Transform GetValue(PulseContext c) => Device.Read(c).Transform;
}