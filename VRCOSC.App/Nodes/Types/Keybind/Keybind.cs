// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.SDK.Utils;

namespace VRCOSC.App.Nodes.Types.Keybind;

[Node("Press Keybind", "Keybind")]
public class KeybindPressNode : Node, IFlowInput
{
    public FlowContinuation Next = new();

    public ValueInput<SDK.Utils.Keybind> Keybind = new();
    public ValueInput<int> DurationMilliseconds = new("Duration Milliseconds");

    protected override async Task Process(PulseContext c)
    {
        var keybind = Keybind.Read(c);
        if (keybind is null) return;

        await KeySimulator.PressKeybind(keybind, DurationMilliseconds.Read(c));
        await Next.Execute(c);
    }
}

[Node("Hold/Release Keybind", "Keybind")]
public class KeybindHoldReleaseNode : Node, IFlowInput
{
    public GlobalStore<bool> PrevCondition = new();

    public FlowContinuation Next = new("Next");

    public ValueInput<SDK.Utils.Keybind> Keybind = new();
    public ValueInput<bool> Condition = new();

    protected override async Task Process(PulseContext c)
    {
        var keybind = Keybind.Read(c);
        if (keybind is null) return;

        var condition = Condition.Read(c);

        if (!PrevCondition.Read(c) && condition)
        {
            await KeySimulator.HoldKeybind(keybind);
            PrevCondition.Write(condition, c);
            await Next.Execute(c);
            return;
        }

        if (PrevCondition.Read(c) && !condition)
        {
            await KeySimulator.ReleaseKeybind(keybind);
            PrevCondition.Write(condition, c);
            await Next.Execute(c);
            return;
        }
    }
}

[Node("On Keybind Pressed", "Keybind")]
public sealed class OnKeybindPressedNode : Node, IUpdateNode
{
    public GlobalStore<bool> Pressed = new();

    public FlowContinuation OnPressed = new("On Pressed");

    public ValueInput<SDK.Utils.Keybind> Keybind = new();
    public ValueInput<bool> AllowHold = new("Allow Hold");

    protected override async Task Process(PulseContext c)
    {
        await OnPressed.Execute(c);
    }

    public bool OnUpdate(PulseContext c)
    {
        var keybind = Keybind.Read(c);
        if (keybind is null) return false;

        var result = keybind.Keys.TrueForAll(key => AppManager.GetInstance().GlobalKeyboardHook.GetKeyState(key)) &&
                     keybind.Modifiers.TrueForAll(key => AppManager.GetInstance().GlobalKeyboardHook.GetKeyState(key));

        if (AllowHold.Read(c))
        {
            Pressed.Write(result, c);
            return result;
        }

        if (result && !Pressed.Read(c))
        {
            Pressed.Write(result, c);
            return true;
        }

        Pressed.Write(result, c);
        return false;
    }
}