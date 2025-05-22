// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Inputs;

[Node("Button Input", "")]
public sealed class ButtonInputNode : Node
{
    public FlowContinuation Next = new();

    protected override void Process(PulseContext c)
    {
        Next.Execute(c);
    }
}