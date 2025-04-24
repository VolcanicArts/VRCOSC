// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

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
    private int process
    (
        [NodeValue("Condition")] bool condition
    )
    {
        return condition ? 0 : 1;
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
    private int process
    (
        [NodeValue("Condition")] bool condition
    )
    {
        if (!prevCondition && condition)
        {
            prevCondition = condition;
            return 0;
        }

        if (prevCondition && !condition)
        {
            prevCondition = condition;
            return 1;
        }

        if (prevCondition && condition)
        {
            prevCondition = condition;
            return 2;
        }

        if (!prevCondition && !condition)
        {
            prevCondition = condition;
            return 3;
        }

        throw new Exception();
    }
}