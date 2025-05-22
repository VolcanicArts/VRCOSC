// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow.Branch;

[Node("If", "Flow")]
public sealed class IfNode : Node, IFlowInput
{
    public FlowContinuation OnTrue = new("On True");
    public FlowContinuation OnFalse = new("On False");

    public ValueInput<bool> Condition = new();

    protected override void Process(PulseContext c)
    {
        if (Condition.Read(c))
            OnTrue.Execute(c);
        else
            OnFalse.Execute(c);
    }
}