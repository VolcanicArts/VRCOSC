// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.OpenVR.Device;
using VRCOSC.App.Utils;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.Nodes.Types.SteamVR;

[Node("Is Dashboard Visible", "SteamVR")]
[NodeCollapsed]
public sealed class SteamVRIsDashboardVisibleNode() : SimpleValueSourceNode<bool>(() => AppManager.GetInstance().OpenVRManager.IsDashboardVisible, "Is Visible");

[Node("Is User Present", "SteamVR")]
[NodeCollapsed]
public sealed class SteamVRIsUserPresentNode() : SimpleValueSourceNode<bool>(() => AppManager.GetInstance().OpenVRManager.IsUserPresent, "Is Present");

[Node("VR FPS", "SteamVR")]
[NodeCollapsed]
public sealed class SteamVRFPSNode() : SimpleValueSourceNode<float>(() => AppManager.GetInstance().OpenVRManager.FPS, "FPS");

[Node("Device Info", "SteamVR")]
public sealed class SteamVRDeviceInfoNode() : ValueConsumeNode<TrackedDevice?>("Device"), IContinuousNode
{
    public int UpdateOffset => 0;

    public ValueOutput<bool> IsConnected = new();
    public ValueOutput<bool> IsCharging = new();
    public ValueOutput<float> Battery = new();

    protected override void ConsumeValue(TrackedDevice? device, PulseContext c)
    {
        if (device is null) return;

        IsConnected.Write(device.IsConnected, c);
        IsCharging.Write(device.IsCharging, c);
        Battery.Write(device.BatteryPercentage, c);
    }
}

[Node("Device Transform", "SteamVR")]
public sealed class SteamVRDeviceTransformSourceNode() : SimpleValueTransformNode<TrackedDevice?, Transform>(d => d?.Transform ?? Transform.Identity, "Device", "Transform"), IContinuousNode
{
    public int UpdateOffset => 0;
}