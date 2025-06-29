// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using VRCOSC.App.ChatBox;

namespace VRCOSC.App.Nodes.Types.ChatBox;

[Node("Override ChatBox Text", "ChatBox")]
public sealed class OverrideChatBoxTextNode : Node, IFlowInput
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