// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using VRCOSC.App.ChatBox;

namespace VRCOSC.App.Nodes.Types.ChatBox;

[Node("Override ChatBox Text", "ChatBox")]
public sealed class ChatBoxOverrideTextNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<string> Input = new();
    public ValueInput<bool> MinimalBackground = new("Minimal Background");

    protected override async Task Process(PulseContext c)
    {
        var input = Input.Read(c);

        if (input is null)
        {
            ChatBoxManager.GetInstance().PulseText = null;
            await Next.Execute(c);
            return;
        }

        var minimalBackground = MinimalBackground.Read(c);

        ChatBoxManager.GetInstance().PulseText = input.Replace(Environment.NewLine, "\n");
        ChatBoxManager.GetInstance().PulseMinimalBackground = minimalBackground;

        await Next.Execute(c);
    }
}

[Node("Is ChatBox Layer Enabled", "ChatBox")]
public sealed class ChatBoxIsLayerEnabledNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<int> Layer = new();

    public ValueOutput<bool> IsEnabled = new("Is Enabled");

    protected override async Task Process(PulseContext c)
    {
        var layer = Layer.Read(c);
        layer = int.Clamp(layer, 0, ChatBoxManager.GetInstance().Timeline.LayerCount - 1);

        var isEnabled = ChatBoxManager.GetInstance().Timeline.LayerEnabled[layer];
        IsEnabled.Write(isEnabled, c);
        await Next.Execute(c);
    }
}

[Node("Set ChatBox Layer Enabled", "ChatBox")]
public sealed class ChatBoxSetLayerEnabledNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<int> Layer = new();
    public ValueInput<bool> Enabled = new("Enabled");

    protected override async Task Process(PulseContext c)
    {
        var layer = Layer.Read(c);
        layer = int.Clamp(layer, 0, ChatBoxManager.GetInstance().Timeline.LayerCount - 1);

        ChatBoxManager.GetInstance().Timeline.SetLayerEnabled(layer, Enabled.Read(c));
        await Next.Execute(c);
    }
}