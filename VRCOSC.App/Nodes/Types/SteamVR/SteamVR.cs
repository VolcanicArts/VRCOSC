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

[Node("Controller Trigger", "SteamVR")]
public sealed class SteamVRControllerTriggerNode : Node, IUpdateNode
{
    public ValueInput<Controller> Controller = new();

    public ValueOutput<float> Pull = new();
    public ValueOutput<bool> Touch = new();
    public ValueOutput<bool> Click = new();

    protected override void Process(PulseContext c)
    {
        var controller = Controller.Read(c);
        if (controller is null) return;

        Pull.Write(controller.Input.Trigger.Pull, c);
        Touch.Write(controller.Input.Trigger.Touch, c);
        Click.Write(controller.Input.Trigger.Click, c);
    }

    public bool OnUpdate(PulseContext c) => true;
}

[Node("Controller Stick", "SteamVR")]
public sealed class SteamVRControllerStickNode : Node, IUpdateNode
{
    public ValueInput<Controller> Controller = new();

    public ValueOutput<Vector2> Position = new();
    public ValueOutput<bool> Touch = new();
    public ValueOutput<bool> Click = new();

    protected override void Process(PulseContext c)
    {
        var controller = Controller.Read(c);
        if (controller is null) return;

        Position.Write(controller.Input.Stick.Position, c);
        Touch.Write(controller.Input.Stick.Touch, c);
        Click.Write(controller.Input.Stick.Click, c);
    }

    public bool OnUpdate(PulseContext c) => true;
}

[Node("Controller Primary", "SteamVR")]
public sealed class SteamVRControllerPrimaryNode : Node, IUpdateNode
{
    public ValueInput<Controller> Controller = new();

    public ValueOutput<bool> Touch = new();
    public ValueOutput<bool> Click = new();

    protected override void Process(PulseContext c)
    {
        var controller = Controller.Read(c);
        if (controller is null) return;

        Touch.Write(controller.Input.Primary.Touch, c);
        Click.Write(controller.Input.Primary.Click, c);
    }

    public bool OnUpdate(PulseContext c) => true;
}

[Node("Controller Secondary", "SteamVR")]
public sealed class SteamVRControllerSecondaryNode : Node, IUpdateNode
{
    public ValueInput<Controller> Controller = new();

    public ValueOutput<bool> Touch = new();
    public ValueOutput<bool> Click = new();

    protected override void Process(PulseContext c)
    {
        var controller = Controller.Read(c);
        if (controller is null) return;

        Touch.Write(controller.Input.Secondary.Touch, c);
        Click.Write(controller.Input.Secondary.Click, c);
    }

    public bool OnUpdate(PulseContext c) => true;
}

[Node("Controller Grip", "SteamVR")]
public sealed class SteamVRControllerGripNode : Node, IUpdateNode
{
    public ValueInput<Controller> Controller = new();

    public ValueOutput<float> Pull = new();
    public ValueOutput<bool> Click = new();

    protected override void Process(PulseContext c)
    {
        var controller = Controller.Read(c);
        if (controller is null) return;

        Pull.Write(controller.Input.Grip.Pull, c);
        Click.Write(controller.Input.Grip.Click, c);
    }

    public bool OnUpdate(PulseContext c) => true;
}

[Node("Controller Pad", "SteamVR")]
public sealed class SteamVRControllerPadNode : Node, IUpdateNode
{
    public ValueInput<Controller> Controller = new();

    public ValueOutput<Vector2> Position = new();
    public ValueOutput<bool> Touch = new();
    public ValueOutput<bool> Click = new();

    protected override void Process(PulseContext c)
    {
        var controller = Controller.Read(c);
        if (controller is null) return;

        Position.Write(controller.Input.Pad.Position, c);
        Touch.Write(controller.Input.Pad.Touch, c);
        Click.Write(controller.Input.Pad.Click, c);
    }

    public bool OnUpdate(PulseContext c) => true;
}

[Node("Controller Skeleton", "SteamVR")]
public sealed class SteamVRControllerSkeletonNode : Node, IUpdateNode
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

        Index.Write(controller.Input.Skeleton.Index, c);
        Middle.Write(controller.Input.Skeleton.Middle, c);
        Ring.Write(controller.Input.Skeleton.Ring, c);
        Pinky.Write(controller.Input.Skeleton.Pinky, c);
    }

    public bool OnUpdate(PulseContext c) => true;
}