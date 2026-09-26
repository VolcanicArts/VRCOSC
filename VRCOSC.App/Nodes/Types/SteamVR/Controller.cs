// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using VRCOSC.App.OpenVR.Device;

namespace VRCOSC.App.Nodes.Types.SteamVR;

public abstract class SteamVRControllerConsumeNode() : ValueConsumeNode<Controller?>(nameof(Controller)), IContinuousNode
{
    public int UpdateOffset => 0;

    protected override void ConsumeValue(Controller? controller, IPulseContext c)
    {
        if (controller is null) return;

        ConsumeController(controller, c);
    }

    protected abstract void ConsumeController(Controller controller, IPulseContext c);
}

[Node("Controller Stick", "SteamVR/Input")]
public sealed class SteamVRControllerStickNode : SteamVRControllerConsumeNode
{
    public ValueOutput<Vector2> Position = new();
    public ValueOutput<bool> Touch = new();
    public ValueOutput<bool> Click = new();

    protected override void ConsumeController(Controller controller, IPulseContext c)
    {
        var stick = controller.Input.Stick;
        Position.Write(stick.Position, c);
        Touch.Write(stick.Touch, c);
        Click.Write(stick.Click, c);
    }
}

[Node("Controller Trigger", "SteamVR/Input")]
public sealed class SteamVRControllerTriggerNode : SteamVRControllerConsumeNode
{
    public ValueOutput<float> Pull = new();
    public ValueOutput<bool> Touch = new();
    public ValueOutput<bool> Click = new();

    protected override void ConsumeController(Controller controller, IPulseContext c)
    {
        var trigger = controller.Input.Triggers.Trigger;
        Pull.Write(trigger.Depth, c);
        Touch.Write(trigger.Touch, c);
        Click.Write(trigger.Click, c);
    }
}

[Node("Controller Bumper", "SteamVR/Input")]
public sealed class SteamVRControllerBumperNode : SteamVRControllerConsumeNode
{
    public ValueOutput<bool> Touch = new();
    public ValueOutput<bool> Click = new();

    protected override void ConsumeController(Controller controller, IPulseContext c)
    {
        var bumper = controller.Input.Triggers.Bumper;
        Touch.Write(bumper.Touch, c);
        Click.Write(bumper.Click, c);
    }
}

[Node("Controller Button North", "SteamVR/Input")]
public sealed class SteamVRControllerButtonNorthNode : SteamVRControllerConsumeNode
{
    public ValueOutput<bool> Touch = new();
    public ValueOutput<bool> Click = new();

    protected override void ConsumeController(Controller controller, IPulseContext c)
    {
        var button = controller.Input.Buttons.North;
        Touch.Write(button.Touch, c);
        Click.Write(button.Click, c);
    }
}

[Node("Controller Button East", "SteamVR/Input")]
public sealed class SteamVRControllerButtonEastNode : SteamVRControllerConsumeNode
{
    public ValueOutput<bool> Touch = new();
    public ValueOutput<bool> Click = new();

    protected override void ConsumeController(Controller controller, IPulseContext c)
    {
        var button = controller.Input.Buttons.East;
        Touch.Write(button.Touch, c);
        Click.Write(button.Click, c);
    }
}

[Node("Controller Button South", "SteamVR/Input")]
public sealed class SteamVRControllerButtonSouthNode : SteamVRControllerConsumeNode
{
    public ValueOutput<bool> Touch = new();
    public ValueOutput<bool> Click = new();

    protected override void ConsumeController(Controller controller, IPulseContext c)
    {
        var button = controller.Input.Buttons.South;
        Touch.Write(button.Touch, c);
        Click.Write(button.Click, c);
    }
}

[Node("Controller Button West", "SteamVR/Input")]
public sealed class SteamVRControllerButtonWestNode : SteamVRControllerConsumeNode
{
    public ValueOutput<bool> Touch = new();
    public ValueOutput<bool> Click = new();

    protected override void ConsumeController(Controller controller, IPulseContext c)
    {
        var button = controller.Input.Buttons.West;
        Touch.Write(button.Touch, c);
        Click.Write(button.Click, c);
    }
}

[Node("Controller System", "SteamVR/Input")]
public sealed class SteamVRControllerSystemNode : SteamVRControllerConsumeNode
{
    public ValueOutput<bool> Touch = new();
    public ValueOutput<bool> Click = new();

    protected override void ConsumeController(Controller controller, IPulseContext c)
    {
        var button = controller.Input.System;
        Touch.Write(button.Touch, c);
        Click.Write(button.Click, c);
    }
}

[Node("Controller Menu", "SteamVR/Input")]
public sealed class SteamVRControllerMenuNode : SteamVRControllerConsumeNode
{
    public ValueOutput<bool> Touch = new();
    public ValueOutput<bool> Click = new();

    protected override void ConsumeController(Controller controller, IPulseContext c)
    {
        var button = controller.Input.Menu;
        Touch.Write(button.Touch, c);
        Click.Write(button.Click, c);
    }
}

[Node("Controller Grip", "SteamVR/Input")]
public sealed class SteamVRControllerGripNode : SteamVRControllerConsumeNode
{
    public ValueOutput<float> Pull = new();
    public ValueOutput<bool> Touch = new();
    public ValueOutput<bool> Click = new();

    protected override void ConsumeController(Controller controller, IPulseContext c)
    {
        var grip = controller.Input.Grip;
        Pull.Write(grip.Depth, c);
        Touch.Write(grip.Touch, c);
        Click.Write(grip.Click, c);
    }
}

[Node("Controller Pad", "SteamVR/Input")]
public sealed class SteamVRControllerPadNode : SteamVRControllerConsumeNode
{
    public ValueOutput<Vector2> Position = new();
    public ValueOutput<bool> Touch = new();
    public ValueOutput<bool> Click = new();

    protected override void ConsumeController(Controller controller, IPulseContext c)
    {
        var pad = controller.Input.Pad;
        Position.Write(pad.Position, c);
        Touch.Write(pad.Touch, c);
        Click.Write(pad.Click, c);
    }
}

[Node("Controller Skeleton", "SteamVR/Input")]
public sealed class SteamVRControllerSkeletonNode : SteamVRControllerConsumeNode
{
    public ValueOutput<float> Index = new();
    public ValueOutput<float> Middle = new();
    public ValueOutput<float> Ring = new();
    public ValueOutput<float> Pinky = new();

    protected override void ConsumeController(Controller controller, IPulseContext c)
    {
        var skeleton = controller.Input.Skeleton;
        Index.Write(skeleton.Index, c);
        Middle.Write(skeleton.Middle, c);
        Ring.Write(skeleton.Ring, c);
        Pinky.Write(skeleton.Pinky, c);
    }
}