// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.SDK.Utils;

namespace VRCOSC.App.Nodes.Types.Actions;

[Node("Press Keybind", "Keybind")]
public class KeybindPressNode : Node, IFlowInput
{
    public FlowContinuation Next = new();

    public ValueInput<Keybind> Keybind = new();
    public ValueInput<int> DurationMilliseconds = new("Duration Milliseconds");

    protected override void Process(PulseContext c)
    {
        var keybind = Keybind.Read(c);
        if (keybind is null) return;

        KeySimulator.PressKeybind(keybind, DurationMilliseconds.Read(c)).Wait(c.Token);
        Next.Execute(c);
    }
}

[Node("Hold/Release Keybind", "Keybind")]
public class KeybindHoldReleaseNode : Node, IFlowInput
{
    public GlobalStore<bool> PrevCondition = new();

    public FlowContinuation Next = new("Next");

    public ValueInput<Keybind> Keybind = new();
    public ValueInput<bool> Condition = new();

    protected override void Process(PulseContext c)
    {
        var keybind = Keybind.Read(c);
        if (keybind is null) return;

        var condition = Condition.Read(c);

        if (!PrevCondition.Read(c) && condition)
        {
            KeySimulator.HoldKeybind(keybind).Wait(c.Token);
            PrevCondition.Write(condition, c);
            Next.Execute(c);
            return;
        }

        if (PrevCondition.Read(c) && !condition)
        {
            KeySimulator.ReleaseKeybind(keybind).Wait(c.Token);
            PrevCondition.Write(condition, c);
            Next.Execute(c);
            return;
        }
    }
}