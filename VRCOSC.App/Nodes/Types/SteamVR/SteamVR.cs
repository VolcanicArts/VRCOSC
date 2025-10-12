// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;
using VRCOSC.App.OpenVR.Device;
using VRCOSC.App.Utils;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.Nodes.Types.SteamVR;

[Node("Is Dashboard Visible", "SteamVR")]
[NodeCollapsed]
public sealed class SteamVRIsDashboardVisibleNode : Node, IUpdateNode
{
    public ValueOutput<bool> IsVisible = new("Is Visible");

    protected override Task Process(PulseContext c)
    {
        IsVisible.Write(AppManager.GetInstance().OpenVRManager.IsDashboardVisible, c);
        return Task.CompletedTask;
    }

    public bool OnUpdate(PulseContext c) => true;
}

[Node("Is User Present", "SteamVR")]
[NodeCollapsed]
public sealed class SteamVRIsUserPresentNode : Node, IUpdateNode
{
    public ValueOutput<bool> IsPresent = new();

    protected override Task Process(PulseContext c)
    {
        IsPresent.Write(AppManager.GetInstance().OpenVRManager.IsUserPresent, c);
        return Task.CompletedTask;
    }

    public bool OnUpdate(PulseContext c) => true;
}

[Node("VR FPS", "SteamVR")]
[NodeCollapsed]
public sealed class SteamVRFPSNode : Node, IUpdateNode
{
    public ValueOutput<float> FPS = new();

    protected override Task Process(PulseContext c)
    {
        FPS.Write(AppManager.GetInstance().OpenVRManager.FPS, c);
        return Task.CompletedTask;
    }

    public bool OnUpdate(PulseContext c) => true;
}

[Node("Device Info", "SteamVR")]
public sealed class SteamVRDeviceInfoNode : Node, IUpdateNode
{
    public ValueInput<TrackedDevice> Device = new();
    public ValueOutput<bool> IsConnected = new("Is Connected");
    public ValueOutput<bool> IsCharging = new("Is Charging");
    public ValueOutput<float> Battery = new();

    protected override Task Process(PulseContext c)
    {
        var device = Device.Read(c);
        if (device is null) return Task.CompletedTask;

        IsConnected.Write(device.IsConnected, c);
        IsCharging.Write(device.IsCharging, c);
        Battery.Write(device.BatteryPercentage, c);
        return Task.CompletedTask;
    }

    public bool OnUpdate(PulseContext c) => true;
}

[Node("Device Transform", "SteamVR")]
public sealed class SteamVRDeviceTransformSourceNode : Node, IUpdateNode
{
    public ValueInput<TrackedDevice> Device = new();
    public ValueOutput<Vector3> Position = new();
    public ValueOutput<Quaternion> Rotation = new();

    protected override Task Process(PulseContext c)
    {
        var device = Device.Read(c);

        if (device is null)
        {
            Position.Write(Transform.Identity.Position, c);
            Rotation.Write(Transform.Identity.Rotation, c);
        }
        else
        {
            Position.Write(device.Transform.Position, c);
            Rotation.Write(device.Transform.Rotation, c);
        }

        return Task.CompletedTask;
    }

    public bool OnUpdate(PulseContext c) => true;
}

[Node("Controller Trigger", "SteamVR/Input")]
public sealed class SteamVRControllerTriggerNode : Node, IUpdateNode
{
    public ValueInput<Controller> Controller = new();

    public ValueOutput<float> Pull = new();
    public ValueOutput<bool> Touch = new();
    public ValueOutput<bool> Click = new();

    protected override Task Process(PulseContext c)
    {
        var controller = Controller.Read(c);
        if (controller is null) return Task.CompletedTask;

        Pull.Write(controller.Input.Trigger.Pull, c);
        Touch.Write(controller.Input.Trigger.Touch, c);
        Click.Write(controller.Input.Trigger.Click, c);
        return Task.CompletedTask;
    }

    public bool OnUpdate(PulseContext c) => true;
}

[Node("Controller Stick", "SteamVR/Input")]
public sealed class SteamVRControllerStickNode : Node, IUpdateNode
{
    public ValueInput<Controller> Controller = new();

    public ValueOutput<Vector2> Position = new();
    public ValueOutput<bool> Touch = new();
    public ValueOutput<bool> Click = new();

    protected override Task Process(PulseContext c)
    {
        var controller = Controller.Read(c);
        if (controller is null) return Task.CompletedTask;

        Position.Write(controller.Input.Stick.Position, c);
        Touch.Write(controller.Input.Stick.Touch, c);
        Click.Write(controller.Input.Stick.Click, c);
        return Task.CompletedTask;
    }

    public bool OnUpdate(PulseContext c) => true;
}

[Node("Controller Primary", "SteamVR/Input")]
public sealed class SteamVRControllerPrimaryNode : Node, IUpdateNode
{
    public ValueInput<Controller> Controller = new();

    public ValueOutput<bool> Touch = new();
    public ValueOutput<bool> Click = new();

    protected override Task Process(PulseContext c)
    {
        var controller = Controller.Read(c);
        if (controller is null) return Task.CompletedTask;

        Touch.Write(controller.Input.Primary.Touch, c);
        Click.Write(controller.Input.Primary.Click, c);
        return Task.CompletedTask;
    }

    public bool OnUpdate(PulseContext c) => true;
}

[Node("Controller Secondary", "SteamVR/Input")]
public sealed class SteamVRControllerSecondaryNode : Node, IUpdateNode
{
    public ValueInput<Controller> Controller = new();

    public ValueOutput<bool> Touch = new();
    public ValueOutput<bool> Click = new();

    protected override Task Process(PulseContext c)
    {
        var controller = Controller.Read(c);
        if (controller is null) return Task.CompletedTask;

        Touch.Write(controller.Input.Secondary.Touch, c);
        Click.Write(controller.Input.Secondary.Click, c);
        return Task.CompletedTask;
    }

    public bool OnUpdate(PulseContext c) => true;
}

[Node("Controller Grip", "SteamVR/Input")]
public sealed class SteamVRControllerGripNode : Node, IUpdateNode
{
    public ValueInput<Controller> Controller = new();

    public ValueOutput<float> Pull = new();
    public ValueOutput<bool> Click = new();

    protected override Task Process(PulseContext c)
    {
        var controller = Controller.Read(c);
        if (controller is null) return Task.CompletedTask;

        Pull.Write(controller.Input.Grip.Pull, c);
        Click.Write(controller.Input.Grip.Click, c);
        return Task.CompletedTask;
    }

    public bool OnUpdate(PulseContext c) => true;
}

[Node("Controller Pad", "SteamVR/Input")]
public sealed class SteamVRControllerPadNode : Node, IUpdateNode
{
    public ValueInput<Controller> Controller = new();

    public ValueOutput<Vector2> Position = new();
    public ValueOutput<bool> Touch = new();
    public ValueOutput<bool> Click = new();

    protected override Task Process(PulseContext c)
    {
        var controller = Controller.Read(c);
        if (controller is null) return Task.CompletedTask;

        Position.Write(controller.Input.Pad.Position, c);
        Touch.Write(controller.Input.Pad.Touch, c);
        Click.Write(controller.Input.Pad.Click, c);
        return Task.CompletedTask;
    }

    public bool OnUpdate(PulseContext c) => true;
}

[Node("Controller Skeleton", "SteamVR/Input")]
public sealed class SteamVRControllerSkeletonNode : Node, IUpdateNode
{
    public ValueInput<Controller> Controller = new();

    public ValueOutput<float> Index = new();
    public ValueOutput<float> Middle = new();
    public ValueOutput<float> Ring = new();
    public ValueOutput<float> Pinky = new();

    protected override Task Process(PulseContext c)
    {
        var controller = Controller.Read(c);
        if (controller is null) return Task.CompletedTask;

        Index.Write(controller.Input.Skeleton.Index, c);
        Middle.Write(controller.Input.Skeleton.Middle, c);
        Ring.Write(controller.Input.Skeleton.Ring, c);
        Pinky.Write(controller.Input.Skeleton.Pinky, c);
        return Task.CompletedTask;
    }

    public bool OnUpdate(PulseContext c) => true;
}