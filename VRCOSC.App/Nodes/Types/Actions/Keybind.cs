// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.SDK.Utils;

namespace VRCOSC.App.Nodes.Types.Actions;

[Node("Press Keybind", "Keybind")]
public class KeybindPressNode : Node, IFlowInput
{
    public ValueInput<Keybind> Keybind = new();
    public ValueInput<int> DurationMilliseconds = new();

    protected override void Process(PulseContext c)
    {
        KeySimulator.PressKeybind(Keybind.Read(c), DurationMilliseconds.Read(c)).Wait(c.Token);
    }
}

[Node("Hold/Release Keybind", "Keybind")]
public class KeybindHoldReleaseNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<Keybind> Keybind = new();
    public ValueInput<bool> Condition = new();

    public GlobalStore<bool> PrevCondition = new();

    protected override void Process(PulseContext c)
    {
        var condition = Condition.Read(c);

        if (!PrevCondition.Read(c) && condition)
        {
            KeySimulator.HoldKeybind(Keybind.Read(c)).Wait(c.Token);
            PrevCondition.Write(condition, c);
            Next.Execute(c);
            return;
        }

        if (PrevCondition.Read(c) && !condition)
        {
            KeySimulator.ReleaseKeybind(Keybind.Read(c)).Wait(c.Token);
            PrevCondition.Write(condition, c);
            Next.Execute(c);
            return;
        }
    }
}