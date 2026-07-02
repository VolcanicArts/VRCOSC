// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.SDK.Utils;

namespace VRCOSC.App.Nodes.Types.Keybind;

[Node("Press Keybind", "Input/Keybind")]
public sealed class KeybindPressNode : AsyncActionNode
{
    public ValueInput<SDK.Utils.Keybind> Keybind = new();
    public ValueInput<int> Duration = new("Duration (ms)", 25);

    protected override async Task DoActionAsync(IPulseContext c)
    {
        var keybind = Keybind.Read(c);
        if (keybind is null) return;

        await KeySimulator.PressKeybind(keybind, Duration.Read(c));
        await Next.Execute(c);
    }
}

[Node("Hold/Release Keybind", "Input/Keybind")]
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

[Node("Keybind Source", "Input/Keybind")]
public sealed class KeybindSourceNode() : ValueSourceNode<bool>("Down")
{
    [InputMode(InputModes.Inline)]
    public ValueInput<SDK.Utils.Keybind> Keybind = new();

    protected override bool ComputeValue(IPulseContext c)
    {
        var keybind = Keybind.Read(c);
        if (keybind is null || keybind.AllKeys.Length == 0) return false;

        return AppManager.GetInstance().GlobalKeyboardHook.AreAllKeysPressed(keybind.AllKeys);
    }
}