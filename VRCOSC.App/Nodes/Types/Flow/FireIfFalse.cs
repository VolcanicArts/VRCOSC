// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Fire If False", "Flow")]
public sealed class FireIfFalseNode : Node
{
    public FlowCall Next = new("Next");

    [NodeReactive]
    public ValueInput<bool> Condition = new();

    protected override async Task Process(PulseContext c)
    {
        if (!Condition.Read(c))
            await Next.Execute(c);
    }
}