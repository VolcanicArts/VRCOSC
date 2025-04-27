// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Flow;

[Node("If", "Flow")]
public sealed class IfNode : Node, IFlowInput, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs =>
    [
        new("On True"),
        new("On False")
    ];

    [NodeProcess]
    private void process
    (
        [NodeValue("Condition")] bool condition
    )
    {
        TriggerFlow(condition ? 0 : 1);
    }
}

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
    private void process
    (
        [NodeValue("Condition")] bool condition
    )
    {
        if (!prevCondition && condition)
        {
            prevCondition = condition;
            TriggerFlow(0);
            return;
        }

        if (prevCondition && !condition)
        {
            prevCondition = condition;
            TriggerFlow(1);
            return;
        }

        if (prevCondition && condition)
        {
            prevCondition = condition;
            TriggerFlow(2);
            return;
        }

        if (!prevCondition && !condition)
        {
            prevCondition = condition;
            TriggerFlow(3);
            return;
        }
    }
}