// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using VRCOSC.App.ChatBox;

namespace VRCOSC.App.Nodes.Types.ChatBox;

[Node("Override ChatBox Text", "ChatBox")]
public sealed class ChatBoxOverrideTextNode : Node, IFlowInput
{
    public FlowContinuation Next = new();

    public ValueInput<string> Input = new();
    public ValueInput<bool> MinimalBackground = new();

    protected override Task Process(PulseContext c)
    {
        var input = Input.Read(c);

        if (input is null)
        {
            ChatBoxManager.GetInstance().PulseText = null;
            return Next.Execute(c);
        }

        var minimalBackground = MinimalBackground.Read(c);

        ChatBoxManager.GetInstance().PulseText = input.Replace(Environment.NewLine, "\n");
        ChatBoxManager.GetInstance().PulseMinimalBackground = minimalBackground;

        return Next.Execute(c);
    }
}

[Node("Is ChatBox Layer Enabled", "ChatBox")]
public sealed class ChatBoxIsLayerEnabledNode : Node, IFlowInput
{
    public FlowContinuation Next = new();

    public ValueInput<int> Layer = new();

    public ValueOutput<bool> IsEnabled = new();

    protected override Task Process(PulseContext c)
    {
        var layer = Layer.Read(c);
        layer = int.Clamp(layer, 0, ChatBoxManager.GetInstance().Timeline.LayerCount - 1);

        var isEnabled = ChatBoxManager.GetInstance().Timeline.LayerEnabled[layer];
        IsEnabled.Write(isEnabled, c);
        return Next.Execute(c);
    }
}

[Node("Set ChatBox Layer Enabled", "ChatBox")]
public sealed class ChatBoxSetLayerEnabledNode : Node, IFlowInput
{
    public FlowContinuation Next = new();

    public ValueInput<int> Layer = new();
    public ValueInput<bool> Enabled = new();

    protected override Task Process(PulseContext c)
    {
        var layer = Layer.Read(c);
        layer = int.Clamp(layer, 0, ChatBoxManager.GetInstance().Timeline.LayerCount - 1);

        ChatBoxManager.GetInstance().Timeline.SetLayerEnabled(layer, Enabled.Read(c));
        return Next.Execute(c);
    }
}