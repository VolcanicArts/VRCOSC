// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.Nodes.Types.Base;
using VRCOSC.App.SDK.OVR.Device;

namespace VRCOSC.App.Nodes.Types.SteamVR;

[Node("Is Dashboard Visible", "SteamVR")]
public sealed class SteamVRIsDashboardVisibleNode : SourceNode<bool>
{
    public ValueOutput<bool> IsVisible = new();

    protected override void Process(PulseContext c)
    {
        IsVisible.Write(AppManager.GetInstance().OVRClient.IsDashboardVisible(), c);
    }

    protected override bool GetValue(PulseContext c) => AppManager.GetInstance().OVRClient.IsDashboardVisible();
}

[Node("Is User Present", "SteamVR")]
public sealed class SteamVRIsUserPresentNode : SourceNode<bool>
{
    public ValueOutput<bool> IsPresent = new();

    protected override void Process(PulseContext c)
    {
        IsPresent.Write(AppManager.GetInstance().OVRClient.IsUserPresent(), c);
    }

    protected override bool GetValue(PulseContext c) => AppManager.GetInstance().OVRClient.IsUserPresent();
}

[Node("FPS", "SteamVR")]
public sealed class SteamVRFPSNode : SourceNode<float>
{
    public ValueOutput<float> FPS = new();

    protected override void Process(PulseContext c)
    {
        FPS.Write(AppManager.GetInstance().OVRClient.FPS, c);
    }

    protected override float GetValue(PulseContext c) => AppManager.GetInstance().OVRClient.FPS;
}

[Node("Tracked Device Info", "SteamVR")]
public sealed class SteamVRTrackedDeviceInfoNode : SourceNode<int>
{
    public ValueInput<TrackedDevice> Device = new();
    public ValueOutput<bool> IsConnected = new();
    public ValueOutput<bool> IsCharging = new();
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

[Node("HMD Source", "SteamVR")]
public sealed class SteamVRHMDSourceNode : SourceNode<string>
{
    public ValueOutput<HMD> HMD = new();

    protected override void Process(PulseContext c)
    {
        HMD.Write(AppManager.GetInstance().OVRClient.GetHMD(), c);
    }

    protected override string GetValue(PulseContext c) => AppManager.GetInstance().OVRClient.GetHMD().SerialNumber;
}