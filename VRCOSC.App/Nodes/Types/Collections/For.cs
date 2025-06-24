// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Collections;

[Node("For", "Collections")]
public sealed class ForNode : Node, IFlowInput
{
    public FlowCall OnIteration = new("On Iteration");
    public FlowContinuation OnEnd = new("On End");

    public ValueInput<int> Count = new();
    public ValueOutput<int> Index = new();

    protected override async Task Process(PulseContext c)
    {
        for (var i = 0; i < Count.Read(c); i++)
        {
            Index.Write(i, c);
            await OnIteration.Execute(c);
        }

        await OnEnd.Execute(c);
    }
}