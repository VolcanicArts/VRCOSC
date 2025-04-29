// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace VRCOSC.App.SDK.Nodes.Types.Flow.Branch;

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
        CancellationToken token,
        [NodeValue("Condition")] bool condition
    )
    {
        if (!prevCondition && condition)
        {
            prevCondition = condition;
            return TriggerFlow(token, 0);
        }

        if (prevCondition && !condition)
        {
            prevCondition = condition;
            return TriggerFlow(token, 1);
        }

        if (prevCondition && condition)
        {
            prevCondition = condition;
            return TriggerFlow(token, 2);
        }

        if (!prevCondition && !condition)
        {
            prevCondition = condition;
            return TriggerFlow(token, 3);
        }

        throw new Exception();
    }
}