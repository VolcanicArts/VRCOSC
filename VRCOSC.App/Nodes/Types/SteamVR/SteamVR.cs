// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.SDK.OVR.Device;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.Nodes.Types.SteamVR;

[Node("Is Dashboard Visible", "SteamVR")]
public sealed class SteamVRIsDashboardVisibleNode : Node, IUpdateNode
{
    public ValueOutput<bool> IsVisible = new("Is Visible");

    protected override void Process(PulseContext c)
    {
        IsVisible.Write(AppManager.GetInstance().OVRClient.IsDashboardVisible(), c);
    }

    public bool OnUpdate(PulseContext c) => true;
}

[Node("Is User Present", "SteamVR")]
public sealed class SteamVRIsUserPresentNode : Node, IUpdateNode
{
    public ValueOutput<bool> IsPresent = new();

    protected override void Process(PulseContext c)
    {
        IsPresent.Write(AppManager.GetInstance().OVRClient.IsUserPresent(), c);
    }

    public bool OnUpdate(PulseContext c) => true;
}

[Node("FPS", "SteamVR")]
public sealed class SteamVRFPSNode : Node, IUpdateNode
{
    public ValueOutput<float> FPS = new();

    protected override void Process(PulseContext c)
    {
        FPS.Write(AppManager.GetInstance().OVRClient.FPS, c);
    }

    public bool OnUpdate(PulseContext c) => true;
}

[Node("Tracked Device Info", "SteamVR")]
public sealed class SteamVRTrackedDeviceInfoNode : Node, IUpdateNode
{
    public ValueInput<TrackedDevice> Device = new();
    public ValueOutput<bool> IsConnected = new("Is Connected");
    public ValueOutput<bool> IsCharging = new("Is Charging");
    public ValueOutput<float> Battery = new();

    protected override void Process(PulseContext c)
    {
        var device = Device.Read(c);
        if (device is null) return;

        IsConnected.Write(device.IsConnected, c);
        IsCharging.Write(device.IsCharging, c);
        Battery.Write(device.BatteryPercentage, c);
    }

    public bool OnUpdate(PulseContext c) => true;
}

[Node("Device Transform", "SteamVR")]
public sealed class SteamVRDeviceTransformSourceNode : Node, IUpdateNode
{
    public ValueInput<TrackedDevice> Device = new();
    public ValueOutput<Vector3> Pos = new();
    public ValueOutput<Quaternion> Rot = new();

    protected override void Process(PulseContext c)
    {
        var device = Device.Read(c);
        if (device is null) return;

        Pos.Write(device.Transform.Position, c);
        Rot.Write(device.Transform.Rotation, c);
    }

    public bool OnUpdate(PulseContext c) => true;
}

[Node("Button State", "SteamVR")]
public sealed class SteamVRControllerButtonStateNode : Node, IUpdateNode
{
    public ValueInput<Controller> Controller = new();

    public ValueOutput<bool> A = new();
    public ValueOutput<bool> B = new();

    protected override void Process(PulseContext c)
    {
        var controller = Controller.Read(c);
        if (controller is null) return;

        A.Write(controller.Input.A.Touched, c);
        B.Write(controller.Input.B.Touched, c);
    }

    public bool OnUpdate(PulseContext c) => true;
}

[Node("Index Fingers", "SteamVR")]
public sealed class SteamVRIndexFingersNode : Node, IUpdateNode
{
    public ValueInput<Controller> Controller = new();

    public ValueOutput<float> Index = new();
    public ValueOutput<float> Middle = new();
    public ValueOutput<float> Ring = new();
    public ValueOutput<float> Pinky = new();

    protected override void Process(PulseContext c)
    {
        var controller = Controller.Read(c);
        if (controller is null) return;

        Index.Write(controller.Input.IndexFinger, c);
        Middle.Write(controller.Input.MiddleFinger, c);
        Ring.Write(controller.Input.RingFinger, c);
        Pinky.Write(controller.Input.PinkyFinger, c);
    }

    public bool OnUpdate(PulseContext c) => true;
}