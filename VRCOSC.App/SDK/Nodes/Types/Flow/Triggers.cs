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
    private int process()
    {
        return 0;
    }
}

[Node("Update Delay", "Flow")]
public sealed class UpdateDelayNode : Node, IFlowOutput, IFlowTrigger
{
    public NodeFlowRef[] FlowOutputs => [new()];

    private DateTime? lastExecuted;

    [NodeProcess]
    private int process
    (
        [NodeValue("Interval Milliseconds")] int milliseconds
    )
    {
        if (milliseconds == 0) return -1;

        lastExecuted ??= DateTime.Now;

        if (lastExecuted + TimeSpan.FromMilliseconds(milliseconds) <= DateTime.Now)
        {
            lastExecuted = DateTime.Now;
            return 0;
        }

        return -1;
    }
}

[Node("Fire While True", "Flow")]
public sealed class FireWhileTrueNode : Node, IFlowOutput, IFlowTrigger
{
    public NodeFlowRef[] FlowOutputs => [new()];

    [NodeProcess]
    private int process
    (
        [NodeValue("Condition")] bool condition
    )
    {
        return condition ? 0 : -1;
    }
}

[Node("Fire While False", "Flow")]
public sealed class FireWhileFalseNode : Node, IFlowOutput, IFlowTrigger
{
    public NodeFlowRef[] FlowOutputs => [new()];

    [NodeProcess]
    private int process
    (
        [NodeValue("Condition")] bool condition
    )
    {
        return !condition ? 0 : -1;
    }
}

[Node("Fire On True", "Flow")]
public sealed class FireOnTrueNode : Node, IFlowOutput, IFlowTrigger
{
    public NodeFlowRef[] FlowOutputs => [new()];

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

[Node("Fire On False", "Flow")]
public sealed class FireOnFalseNode : Node, IFlowOutput, IFlowTrigger
{
    public NodeFlowRef[] FlowOutputs => [new()];

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

[Node("Fire On Change", "Flow")]
public sealed class FireOnChangeNode<T> : Node, IFlowOutput, IFlowTrigger
{
    public NodeFlowRef[] FlowOutputs => [new()];

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