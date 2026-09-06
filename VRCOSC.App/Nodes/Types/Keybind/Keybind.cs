// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using VRCOSC.App.Inputs;

namespace VRCOSC.App.Nodes.Types.Keybind;

[Node("Press Keybind", "Input/Keyboard")]
public sealed class KeybindPressNode : AsyncActionNode
{
    private GlobalInputHandler globalInputHandler => AppManager.GetInstance().GlobalInputHandler;

    public ValueInput<SDK.Utils.Keybind> Keybind = new();
    public ValueInput<int> Duration = new("Duration (ms)", 25);

    protected override async Task DoActionAsync(IPulseContext c)
    {
        var keybind = Keybind.Read(c);
        if (keybind is null) return;

        await globalInputHandler.PressKeybind(keybind, TimeSpan.FromMilliseconds(Duration.Read(c)));
        await Next.Execute(c);
    }
}

[Node("Hold/Release Keybind", "Input/Keyboard")]
public sealed class KeybindHoldReleaseNode : AsyncActionNode
{
    private GlobalInputHandler globalInputHandler => AppManager.GetInstance().GlobalInputHandler;

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
            globalInputHandler.HoldKeybind(keybind);
            PrevCondition.Write(condition, c);
            await Next.Execute(c);
            return;
        }

        if (PrevCondition.Read(c) && !condition)
        {
            globalInputHandler.ReleaseKeybind(keybind);
            PrevCondition.Write(condition, c);
            await Next.Execute(c);
            return;
        }
    }
}

[Node("Keybind Source", "Input/Keyboard")]
public sealed class KeybindSourceNode() : ValueSourceNode<bool>("Down")
{
    [InputMode(InputModes.Inline)]
    public ValueInput<SDK.Utils.Keybind> Keybind = new();

    protected override bool ComputeValue(IPulseContext c)
    {
        var keybind = Keybind.Read(c);
        if (keybind is null || keybind.AllKeys.Length == 0) return false;

        return AppManager.GetInstance().GlobalInputHandler.GetKeybindState(keybind);
    }
}