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

[Node("Controller Trigger", "SteamVR/Input")]
public sealed class SteamVRControllerTriggerNode : SteamVRControllerConsumeNode
{
    public ValueOutput<float> Pull = new();
    public ValueOutput<bool> Touch = new();
    public ValueOutput<bool> Click = new();

    protected override void ConsumeController(Controller controller, IPulseContext c)
    {
        Pull.Write(controller.Input.Trigger.Pull, c);
        Touch.Write(controller.Input.Trigger.Touch, c);
        Click.Write(controller.Input.Trigger.Click, c);
    }
}

[Node("Controller Stick", "SteamVR/Input")]
public sealed class SteamVRControllerStickNode : SteamVRControllerConsumeNode
{
    public ValueOutput<Vector2> Position = new();
    public ValueOutput<bool> Touch = new();
    public ValueOutput<bool> Click = new();

    protected override void ConsumeController(Controller controller, IPulseContext c)
    {
        Position.Write(controller.Input.Stick.Position, c);
        Touch.Write(controller.Input.Stick.Touch, c);
        Click.Write(controller.Input.Stick.Click, c);
    }
}

[Node("Controller Primary", "SteamVR/Input")]
public sealed class SteamVRControllerPrimaryNode : SteamVRControllerConsumeNode
{
    public ValueOutput<bool> Touch = new();
    public ValueOutput<bool> Click = new();

    protected override void ConsumeController(Controller controller, IPulseContext c)
    {
        Touch.Write(controller.Input.Primary.Touch, c);
        Click.Write(controller.Input.Primary.Click, c);
    }
}

[Node("Controller Secondary", "SteamVR/Input")]
public sealed class SteamVRControllerSecondaryNode : SteamVRControllerConsumeNode
{
    public ValueOutput<bool> Touch = new();
    public ValueOutput<bool> Click = new();

    protected override void ConsumeController(Controller controller, IPulseContext c)
    {
        Touch.Write(controller.Input.Secondary.Touch, c);
        Click.Write(controller.Input.Secondary.Click, c);
    }
}

[Node("Controller System", "SteamVR/Input")]
public sealed class SteamVRControllerSystemNode : SteamVRControllerConsumeNode
{
    public ValueOutput<bool> Touch = new();
    public ValueOutput<bool> Click = new();

    protected override void ConsumeController(Controller controller, IPulseContext c)
    {
        Touch.Write(controller.Input.System.Touch, c);
        Click.Write(controller.Input.System.Click, c);
    }
}

[Node("Controller Grip", "SteamVR/Input")]
public sealed class SteamVRControllerGripNode : SteamVRControllerConsumeNode
{
    public ValueOutput<float> Pull = new();
    public ValueOutput<bool> Click = new();

    protected override void ConsumeController(Controller controller, IPulseContext c)
    {
        Pull.Write(controller.Input.Grip.Pull, c);
        Click.Write(controller.Input.Grip.Click, c);
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
        Position.Write(controller.Input.Pad.Position, c);
        Touch.Write(controller.Input.Pad.Touch, c);
        Click.Write(controller.Input.Pad.Click, c);
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
        Index.Write(controller.Input.Skeleton.Index, c);
        Middle.Write(controller.Input.Skeleton.Middle, c);
        Ring.Write(controller.Input.Skeleton.Ring, c);
        Pinky.Write(controller.Input.Skeleton.Pinky, c);
    }
}