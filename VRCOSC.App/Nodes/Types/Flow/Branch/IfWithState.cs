// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow.Branch;

[Node("If With State", "Flow")]
public sealed class IfWithStateNode : Node, IFlowInput, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs =>
    [
        new("On Became True"),
        new("On Became False"),
        new("On Still True"),
        new("On Still False")
    ];

    private bool prevCondition;

    [NodeProcess]
    private Task process
    (
        FlowContext context,
        [NodeValue("Condition")] bool condition
    )
    {
        if (!prevCondition && condition)
        {
            prevCondition = condition;
            return TriggerFlow(context, 0);
        }

        if (prevCondition && !condition)
        {
            prevCondition = condition;
            return TriggerFlow(context, 1);
        }

        if (prevCondition && condition)
        {
            prevCondition = condition;
            return TriggerFlow(context, 2);
        }

        if (!prevCondition && !condition)
        {
            prevCondition = condition;
            return TriggerFlow(context, 3);
        }

        throw new Exception();
    }
}