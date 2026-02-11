// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using System.Threading.Tasks;
using VRCOSC.App.SDK.Utils;

namespace VRCOSC.App.Nodes.Types.Keybind;

[Node("Press Keybind", "Keybind")]
public sealed class KeybindPressNode : Node, IFlowInput
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
public sealed class KeybindHoldReleaseNode : Node, IFlowInput
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

[Node("Keybind Source", "Keybind")]
public sealed class KeybindSourceNode : Node, IActiveUpdateNode, IHasKeybindProperty
{
    public int UpdateOffset => 0;

    public GlobalStore<bool> DownStore = new();

    [NodeProperty("keybind")]
    public SDK.Utils.Keybind Keybind { get; set; } = new();

    public ValueOutput<bool> Down = new();

    protected override Task Process(PulseContext c)
    {
        var keybindDown = isKeybindDown();
        Down.Write(keybindDown, c);
        return Task.CompletedTask;
    }

    public Task<bool> OnUpdate(PulseContext c)
    {
        var wasDown = DownStore.Read(c);
        var isDown = isKeybindDown();
        DownStore.Write(isDown, c);

        return Task.FromResult(wasDown != isDown);
    }

    private bool isKeybindDown() => Keybind.Keys.All(key => AppManager.GetInstance().GlobalKeyboardHook.GetKeyState(key))
                                    && Keybind.Modifiers.All(key => AppManager.GetInstance().GlobalKeyboardHook.GetKeyState(key));
}