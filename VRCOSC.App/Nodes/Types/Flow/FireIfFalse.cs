// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Fire If False", "Flow")]
public sealed class FireIfFalseNode : Node
{
    public FlowCall Next = new("Next");

    [NodeReactive]
    public ValueInput<bool> Input = new();

    protected override void Process(PulseContext c)
    {
        if (!Input.Read(c))
            Next.Execute(c);
    }
}