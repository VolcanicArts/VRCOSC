// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Collections;

[Node("For", "Collections")]
public sealed class ForNode : Node
{
    public FlowInput FlowInput = new();
    public FlowOutput OnIteration = new(scope: true);
    public FlowOutput OnEnd = new();

    public ValueInput<int> Count = new();
    public ValueOutput<int> Index = new();

    protected override async Task Process(IPulseContext c)
    {
        var count = Count.Read(c);

        for (var i = 0; i < count; i++)
        {
            Index.Write(i, c);
            await OnIteration.Execute(c);
        }

        Index.Write(0, c);
        await OnEnd.Execute(c);
    }
}