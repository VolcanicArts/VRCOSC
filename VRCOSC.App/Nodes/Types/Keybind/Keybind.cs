// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using System.Threading.Tasks;
using VRCOSC.App.SDK.Utils;

namespace VRCOSC.App.Nodes.Types.Keybind;

[Node("Press Keybind", "Keybind")]
public sealed class KeybindPressNode : AsyncActionNode
{
    public ValueInput<SDK.Utils.Keybind> Keybind = new();
    public ValueInput<int> DurationMilliseconds = new();

    protected override async Task DoActionAsync(IPulseContext c)
    {
        var keybind = Keybind.Read(c);
        if (keybind is null) return;

        await KeySimulator.PressKeybind(keybind, DurationMilliseconds.Read(c));
        await Next.Execute(c);
    }
}

[Node("Hold/Release Keybind", "Keybind")]
public sealed class KeybindHoldReleaseNode : AsyncActionNode
{
    public GlobalStore<bool> PrevCondition = new();

    public ValueInput<SDK.Utils.Keybind> Keybind = new();
    public ValueInput<bool> Condition = new();

    protected override async Task DoActionAsync(IPulseContext c)
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
public sealed class KeybindSourceNode() : ValueSourceNode<bool>("Down")
{
    public ValueInput<SDK.Utils.Keybind> Keybind = new(modes: ValueInputMode.Inline);

    protected override bool ComputeValue(IPulseContext c)
    {
        var keybind = Keybind.Read(c);
        if (keybind is null) return false;

        return (keybind.Modifiers.Count != 0 || keybind.Keys.Count != 0)
               && keybind.Modifiers.All(key => AppManager.GetInstance().GlobalKeyboardHook.GetKeyState(key))
               && keybind.Keys.All(key => AppManager.GetInstance().GlobalKeyboardHook.GetKeyState(key));
    }
}