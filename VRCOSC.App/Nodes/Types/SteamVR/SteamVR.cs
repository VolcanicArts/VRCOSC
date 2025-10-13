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
public sealed class SteamVRIsDashboardVisibleNode : UpdateNode<bool>
{
    public ValueOutput<bool> IsVisible = new("Is Visible");

    protected override Task Process(PulseContext c)
    {
        IsVisible.Write(AppManager.GetInstance().OpenVRManager.IsDashboardVisible, c);
        return Task.CompletedTask;
    }

    protected override bool GetValue(PulseContext c) => AppManager.GetInstance().OpenVRManager.IsDashboardVisible;
}

[Node("Is User Present", "SteamVR")]
[NodeCollapsed]
public sealed class SteamVRIsUserPresentNode : UpdateNode<bool>
{
    public ValueOutput<bool> IsPresent = new();

    protected override Task Process(PulseContext c)
    {
        IsPresent.Write(AppManager.GetInstance().OpenVRManager.IsUserPresent, c);
        return Task.CompletedTask;
    }

    protected override bool GetValue(PulseContext c) => AppManager.GetInstance().OpenVRManager.IsUserPresent;
}

[Node("VR FPS", "SteamVR")]
[NodeCollapsed]
public sealed class SteamVRFPSNode : UpdateNode<float>
{
    public ValueOutput<float> FPS = new();

    protected override Task Process(PulseContext c)
    {
        FPS.Write(AppManager.GetInstance().OpenVRManager.FPS, c);
        return Task.CompletedTask;
    }

    protected override float GetValue(PulseContext c) => AppManager.GetInstance().OpenVRManager.FPS;
}

[Node("Device Info", "SteamVR")]
public sealed class SteamVRDeviceInfoNode : UpdateNode<bool, bool, float>
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

    protected override (bool, bool, float) GetValues(PulseContext c)
    {
        var device = Device.Read(c);
        if (device is null) return (false, false, 0f);

        return (device.IsConnected, device.IsCharging, device.BatteryPercentage);
    }
}

[Node("Device Transform", "SteamVR")]
public sealed class SteamVRDeviceTransformSourceNode : UpdateNode<Vector3, Quaternion>
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

    protected override (Vector3, Quaternion) GetValues(PulseContext c)
    {
        var device = Device.Read(c);
        if (device is null) return (Transform.Identity.Position, Transform.Identity.Rotation);

        return (device.Transform.Position, device.Transform.Rotation);
    }
}

[Node("Controller Trigger", "SteamVR/Input")]
public sealed class SteamVRControllerTriggerNode : UpdateNode<float, bool, bool>
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

    protected override (float, bool, bool) GetValues(PulseContext c)
    {
        var controller = Controller.Read(c);
        if (controller is null) return (0f, false, false);

        return (controller.Input.Trigger.Pull, controller.Input.Trigger.Touch, controller.Input.Trigger.Click);
    }
}

[Node("Controller Stick", "SteamVR/Input")]
public sealed class SteamVRControllerStickNode : UpdateNode<Vector2, bool, bool>
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

    protected override (Vector2, bool, bool) GetValues(PulseContext c)
    {
        var controller = Controller.Read(c);
        if (controller is null) return (Vector2.Zero, false, false);

        return (controller.Input.Stick.Position, controller.Input.Stick.Touch, controller.Input.Stick.Click);
    }
}

[Node("Controller Primary", "SteamVR/Input")]
public sealed class SteamVRControllerPrimaryNode : UpdateNode<bool, bool>
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

    protected override (bool, bool) GetValues(PulseContext c)
    {
        var controller = Controller.Read(c);
        if (controller is null) return (false, false);

        return (controller.Input.Primary.Touch, controller.Input.Primary.Click);
    }
}

[Node("Controller Secondary", "SteamVR/Input")]
public sealed class SteamVRControllerSecondaryNode : UpdateNode<bool, bool>
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

    protected override (bool, bool) GetValues(PulseContext c)
    {
        var controller = Controller.Read(c);
        if (controller is null) return (false, false);

        return (controller.Input.Secondary.Touch, controller.Input.Secondary.Click);
    }
}

[Node("Controller Grip", "SteamVR/Input")]
public sealed class SteamVRControllerGripNode : UpdateNode<float, bool>
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

    protected override (float, bool) GetValues(PulseContext c)
    {
        var controller = Controller.Read(c);
        if (controller is null) return (0f, false);

        return (controller.Input.Grip.Pull, controller.Input.Grip.Click);
    }
}

[Node("Controller Pad", "SteamVR/Input")]
public sealed class SteamVRControllerPadNode : UpdateNode<Vector2, bool, bool>
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

    protected override (Vector2, bool, bool) GetValues(PulseContext c)
    {
        var controller = Controller.Read(c);
        if (controller is null) return (Vector2.Zero, false, false);

        return (controller.Input.Pad.Position, controller.Input.Pad.Touch, controller.Input.Pad.Click);
    }
}

[Node("Controller Skeleton", "SteamVR/Input")]
public sealed class SteamVRControllerSkeletonNode : UpdateNode<float, float, float, float>
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

    protected override (float, float, float, float) GetValues(PulseContext c)
    {
        var controller = Controller.Read(c);
        if (controller is null) return (0f, 0f, 0f, 0f);

        return (controller.Input.Skeleton.Index, controller.Input.Skeleton.Middle, controller.Input.Skeleton.Ring, controller.Input.Skeleton.Pinky);
    }
}