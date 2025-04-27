// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;

namespace VRCOSC.App.SDK.Nodes.Types.Flow;

[Node("On Update", "Flow")]
public sealed class OnUpdateNode : Node, IFlowOutput, IFlowTrigger
{
    public NodeFlowRef[] FlowOutputs => [new()];

    [NodeProcess]
    private void process() => TriggerFlow(0);
}

[Node("Update Delay", "Flow")]
public sealed class UpdateDelayNode : Node, IFlowOutput, IFlowTrigger
{
    public NodeFlowRef[] FlowOutputs => [new()];

    private DateTime? lastExecuted;

    [NodeProcess]
    private void process
    (
        [NodeValue("Interval Milliseconds")] int milliseconds
    )
    {
        if (milliseconds == 0) return;

        lastExecuted ??= DateTime.Now;

        if (lastExecuted + TimeSpan.FromMilliseconds(milliseconds) <= DateTime.Now)
        {
            lastExecuted = DateTime.Now;
            TriggerFlow(0);
        }
    }
}

[Node("Fire While True", "Flow")]
public sealed class FireWhileTrueNode : Node, IFlowOutput, IFlowTrigger
{
    public NodeFlowRef[] FlowOutputs => [new()];

    [NodeProcess]
    private void process
    (
        [NodeValue("Condition")] bool condition
    )
    {
        if (condition) TriggerFlow(0);
    }
}

[Node("Fire While False", "Flow")]
public sealed class FireWhileFalseNode : Node, IFlowOutput, IFlowTrigger
{
    public NodeFlowRef[] FlowOutputs => [new()];

    [NodeProcess]
    private void process
    (
        [NodeValue("Condition")] bool condition
    )
    {
        if (!condition) TriggerFlow(0);
    }
}

[Node("Fire On True", "Flow")]
public sealed class FireOnTrueNode : Node, IFlowOutput, IFlowTrigger
{
    public NodeFlowRef[] FlowOutputs => [new()];

    private bool previousCondition;

    [NodeProcess]
    private void process
    (
        [NodeValue("Condition")] bool condition
    )
    {
        var shouldTrigger = !previousCondition && condition;
        if (shouldTrigger) TriggerFlow(0);
        previousCondition = condition;
    }
}

[Node("Fire On False", "Flow")]
public sealed class FireOnFalseNode : Node, IFlowOutput, IFlowTrigger
{
    public NodeFlowRef[] FlowOutputs => [new()];

    private bool previousCondition;

    [NodeProcess]
    private void process
    (
        [NodeValue("Condition")] bool condition
    )
    {
        var shouldTrigger = previousCondition && !condition;
        if (shouldTrigger) TriggerFlow(0);
        previousCondition = condition;
    }
}

[Node("Fire On Change", "Flow")]
public sealed class FireOnChangeNode<T> : Node, IFlowOutput, IFlowTrigger
{
    public NodeFlowRef[] FlowOutputs => [new()];

    private T? previousValue;

    [NodeProcess]
    private void process
    (
        [NodeValue("Value")] T value
    )
    {
        var areEqual = value is IEquatable<T> valueE ? valueE.Equals(previousValue) : EqualityComparer<T>.Default.Equals(value, previousValue);
        if (!areEqual) TriggerFlow(0);
        previousValue = value;
    }
}