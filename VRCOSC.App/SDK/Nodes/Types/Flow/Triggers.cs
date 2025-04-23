// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;

namespace VRCOSC.App.SDK.Nodes.Types.Flow;

[Node("On Update", "Flow/Trigger")]
public sealed class UpdateTriggerNode : Node, IFlowOutput, IFlowTrigger
{
    public NodeFlowRef[] FlowOutputs { get; set; } = new NodeFlowRef[1];

    [NodeProcess]
    private int process()
    {
        return 0;
    }
}

[Node("Fire On True", "Flow/Trigger")]
public sealed class FireOnTrueNode : Node, IFlowOutput, IFlowTrigger
{
    public NodeFlowRef[] FlowOutputs { get; set; } = new NodeFlowRef[1];

    private bool previousCondition;

    [NodeProcess]
    private int process
    (
        [NodeValue("Condition")] bool condition
    )
    {
        var shouldTrigger = !previousCondition && condition;
        previousCondition = condition;
        return shouldTrigger ? 0 : -1;
    }
}

[Node("Fire On False", "Flow/Trigger")]
public sealed class FireOnFalseNode : Node, IFlowOutput, IFlowTrigger
{
    public NodeFlowRef[] FlowOutputs { get; set; } = new NodeFlowRef[1];

    private bool previousCondition;

    [NodeProcess]
    private int process
    (
        [NodeValue("Condition")] bool condition
    )
    {
        var shouldTrigger = previousCondition && !condition;
        previousCondition = condition;
        return shouldTrigger ? 0 : -1;
    }
}

[Node("Fire On Change", "Flow/Trigger")]
public sealed class FireOnChangeNode<T> : Node, IFlowOutput, IFlowTrigger
{
    public NodeFlowRef[] FlowOutputs { get; set; } = new NodeFlowRef[1];

    private T? previousValue;

    [NodeProcess]
    private int process
    (
        [NodeValue("Value")] T value
    )
    {
        var result = value is IEquatable<T> valueE ? valueE.Equals(previousValue) : EqualityComparer<T>.Default.Equals(value, previousValue);
        previousValue = value;
        return !result ? 0 : -1;
    }
}