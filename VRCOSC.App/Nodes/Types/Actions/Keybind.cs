// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.SDK.Utils;

namespace VRCOSC.App.Nodes.Types.Actions;

[Node("Press Keybind", "Keybind")]
public class KeybindPressNode : Node, IFlowInput
{
    public FlowContinuation Next = new();

    public ValueInput<Keybind> Keybind = new();
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

    public ValueInput<Keybind> Keybind = new();
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