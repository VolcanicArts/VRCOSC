// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using System.Threading.Tasks;
using VRCOSC.App.SDK.Utils;

namespace VRCOSC.App.Nodes.Types.Keybind;

[Node("Press Keybind", "Keybind")]
public sealed class KeybindPressNode : ActionNode
{
    public ValueInput<SDK.Utils.Keybind> Keybind = new();
    public ValueInput<int> DurationMilliseconds = new();

    protected override async Task DoTask(PulseContext c)
    {
        var keybind = Keybind.Read(c);
        if (keybind is null) return;

        await KeySimulator.PressKeybind(keybind, DurationMilliseconds.Read(c));
        await Next.Execute(c);
    }
}

[Node("Hold/Release Keybind", "Keybind")]
public sealed class KeybindHoldReleaseNode : ActionNode
{
    public GlobalStore<bool> PrevCondition = new();

    public ValueInput<SDK.Utils.Keybind> Keybind = new();
    public ValueInput<bool> Condition = new();

    protected override async Task DoTask(PulseContext c)
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

[Node("Keybind Source", "Keybind")]
public sealed class KeybindSourceNode() : ValueSourceNode<bool>("Down"), IHasKeybindProperty
{
    [NodeProperty("keybind")]
    public SDK.Utils.Keybind Keybind { get; set; } = new();

    protected override bool ComputeValue(PulseContext c)
        => (Keybind.Modifiers.Count != 0 || Keybind.Keys.Count != 0)
           && Keybind.Modifiers.All(key => AppManager.GetInstance().GlobalKeyboardHook.GetKeyState(key))
           && Keybind.Keys.All(key => AppManager.GetInstance().GlobalKeyboardHook.GetKeyState(key));
}