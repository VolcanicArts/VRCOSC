// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Collections;

[Node("For", "Collections")]
public sealed class ForNode : Node, IFlowInput
{
    public FlowCall OnIteration = new("On Iteration");
    public FlowContinuation OnEnd = new("On End");

    public ValueInput<int> Count = new();
    public ValueOutput<int> Index = new();

    protected override void Process(PulseContext c)
    {
        for (var i = 0; i < Count.Read(c); i++)
        {
            Index.Write(i, c);
            OnIteration.Execute(c);
        }

        OnEnd.Execute(c);
    }
}