// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.ChatBox;

namespace VRCOSC.App.Nodes.Types.ChatBox;

[Node("Override ChatBox Text", "ChatBox")]
public sealed class ChatBoxOverrideTextNode : ActionNode
{
    public ValueInput<string> Input = new();
    public ValueInput<bool> MinimalBackground = new();

    protected override void DoAction(IPulseContext c)
    {
        var input = Input.Read(c);

        if (input is null)
        {
            ChatBoxManager.GetInstance().PulseText = null;
            return;
        }

        var minimalBackground = MinimalBackground.Read(c);

        ChatBoxManager.GetInstance().PulseText = input.Replace(Environment.NewLine, "\n");
        ChatBoxManager.GetInstance().PulseMinimalBackground = minimalBackground;
    }
}

[Node("ChatBox Layer Source", "ChatBox")]
public sealed class ChatBoxLayerSourceNode() : ValueSourceNode<bool>("Is Enabled")
{
    public ValueInput<int> Layer = new();

    protected override bool ComputeValue(IPulseContext c)
    {
        var layer = Layer.Read(c);
        if (layer < 0 || layer >= ChatBoxManager.GetInstance().Timeline.LayerCount) return false;

        return ChatBoxManager.GetInstance().Timeline.LayerEnabled[layer];
    }
}

[Node("Set ChatBox Layer Enabled", "ChatBox")]
public sealed class ChatBoxSetLayerEnabledNode : ActionNode
{
    public ValueInput<int> Layer = new();
    public ValueInput<bool> Enabled = new();

    protected override void DoAction(IPulseContext c)
    {
        var layer = Layer.Read(c);
        if (layer < 0 || layer >= ChatBoxManager.GetInstance().Timeline.LayerCount) return;

        ChatBoxManager.GetInstance().Timeline.SetLayerEnabled(layer, Enabled.Read(c));
    }
}