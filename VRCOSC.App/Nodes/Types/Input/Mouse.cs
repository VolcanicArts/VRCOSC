// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Input;
using VRCOSC.App.Inputs;

namespace VRCOSC.App.Nodes.Types.Input;

[Node("Mouse Button Source", "Input/Mouse")]
public sealed class MouseButtonSourceNode : ValueSourceNode<bool>
{
    private GlobalInputHandler globalInputHandler => AppManager.GetInstance().GlobalInputHandler;

    [InputMode(InputModes.Inline)]
    public ValueInput<MouseButton> Button = new();

    protected override bool ComputeValue(IPulseContext c) => globalInputHandler.GetMouseState(Button.Read(c));
}

[Node("Mouse Position Source", "Input/Mouse")]
public sealed class MousePositionSourceNode : ValueSourceNode<Vector2>
{
    private GlobalInputHandler globalInputHandler => AppManager.GetInstance().GlobalInputHandler;

    protected override Vector2 ComputeValue(IPulseContext c) => globalInputHandler.GetMousePosition();
}

[Node("Mouse Wheel Source", "Input/Mouse")]
public sealed class MouseWheelSourceNode : ValueSourceNode<int>
{
    private GlobalInputHandler globalInputHandler => AppManager.GetInstance().GlobalInputHandler;

    protected override int ComputeValue(IPulseContext c) => globalInputHandler.GetMouseWheel();
}

[Node("Mouse Button Press", "Input/Mouse")]
public sealed class MousePressButtonNode : AsyncActionNode
{
    private GlobalInputHandler globalInputHandler => AppManager.GetInstance().GlobalInputHandler;

    public ValueInput<MouseButton> Button = new();
    public ValueInput<int> Delay = new("Delay (ms)", 25);

    protected override Task DoActionAsync(IPulseContext c)
    {
        var delay = TimeSpan.FromMilliseconds(int.Clamp(Delay.Read(c), 0, int.MaxValue));
        return globalInputHandler.PressMouseButton(Button.Read(c), delay);
    }
}

[Node("Mouse Button Hold/Release", "Input/Mouse")]
public sealed class MouseHoldReleaseButtonNode : AsyncActionNode
{
    private GlobalInputHandler globalInputHandler => AppManager.GetInstance().GlobalInputHandler;

    public GlobalStore<bool> PrevCondition = new();

    public ValueInput<MouseButton> Button = new();
    public ValueInput<bool> Condition = new();

    protected override async Task DoActionAsync(IPulseContext c)
    {
        var button = Button.Read(c);
        var condition = Condition.Read(c);

        if (!PrevCondition.Read(c) && condition)
        {
            globalInputHandler.MouseHoldButton(button);
            PrevCondition.Write(condition, c);
            await Next.Execute(c);
            return;
        }

        if (PrevCondition.Read(c) && !condition)
        {
            globalInputHandler.MouseReleaseButton(button);
            PrevCondition.Write(condition, c);
            await Next.Execute(c);
            return;
        }
    }
}

[Node("Mouse Set Position", "Input/Mouse")]
public sealed class MouseSetPositionNode : ActionNode
{
    private GlobalInputHandler globalInputHandler => AppManager.GetInstance().GlobalInputHandler;

    public ValueInput<Vector2> Position = new();
    public ValueInput<bool> IsRelative = new();

    protected override void DoAction(IPulseContext c) => globalInputHandler.SetMousePosition(Position.Read(c), IsRelative.Read(c));
}

[Node("Mouse Scroll", "Input/Mouse")]
public sealed class MouseScrollNode : ActionNode
{
    private GlobalInputHandler globalInputHandler => AppManager.GetInstance().GlobalInputHandler;

    public ValueInput<int> Amount = new();

    protected override void DoAction(IPulseContext c) => globalInputHandler.MouseScroll(Amount.Read(c));
}