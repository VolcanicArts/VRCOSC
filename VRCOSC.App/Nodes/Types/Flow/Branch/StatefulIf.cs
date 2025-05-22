// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow.Branch;

[Node("Stateful If", "Flow")]
public sealed class StatefulIfNode : Node, IFlowInput
{
    public FlowContinuation OnBecameTrue = new("On Became True");
    public FlowContinuation OnBecameFalse = new("On Became False");
    public FlowContinuation OnStillTrue = new("On Still True");
    public FlowContinuation OnStillFalse = new("On Still False");

    public GlobalStore<bool> PrevCondition = new();

    public ValueInput<bool> Condition = new();

    protected override void Process(PulseContext c)
    {
        var prevCondition = PrevCondition.Read(c);
        var condition = Condition.Read(c);

        if (!prevCondition && condition)
        {
            PrevCondition.Write(condition, c);
            OnBecameTrue.Execute(c);
            return;
        }

        if (prevCondition && !condition)
        {
            PrevCondition.Write(condition, c);
            OnBecameFalse.Execute(c);
            return;
        }

        if (prevCondition && condition)
        {
            PrevCondition.Write(condition, c);
            OnStillTrue.Execute(c);
            return;
        }

        if (!prevCondition && !condition)
        {
            PrevCondition.Write(condition, c);
            OnStillFalse.Execute(c);
            return;
        }

        throw new Exception();
    }
}