// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Fire If True", "Flow")]
public sealed class FireIfTrueNode : Node
{
    public FlowContinuation Next = new("Next");

    public ValueInput<bool> Condition = new();

    protected override async Task Process(PulseContext c)
    {
        if (Condition.Read(c))
            await Next.Execute(c);
    }
}