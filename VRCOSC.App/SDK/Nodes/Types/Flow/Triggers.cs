// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;

namespace VRCOSC.App.SDK.Nodes.Types.Flow;

[Node("On Update", "Flow/Trigger")]
public sealed class UpdateTriggerNode : Node
{
    private readonly NodeFlowRef outFlow;

    public UpdateTriggerNode()
    {
        outFlow = AddFlow("", ConnectionSide.Output);
    }

    [NodeTrigger]
    private bool shouldTrigger() => true;

    [NodeProcess([], [])]
    private void process() => SetFlow(outFlow);
}

[Node("Fire On True", "Flow/Trigger")]
public sealed class FireOnTrueNode : Node
{
    private readonly NodeFlowRef outFlow;
    private bool previousValue;

    public FireOnTrueNode()
    {
        outFlow = AddFlow("", ConnectionSide.Output);
    }

    [NodeTrigger]
    private bool shouldTrigger(bool value)
    {
        var shouldTrigger = !previousValue && value;
        previousValue = value;
        return shouldTrigger;
    }

    [NodeProcess(["Condition"], [""])]
    private void process(bool value) => SetFlow(outFlow);
}

[Node("Fire On False", "Flow/Trigger")]
public sealed class FireOnFalseNode : Node
{
    private readonly NodeFlowRef outFlow;
    private bool previousValue;

    public FireOnFalseNode()
    {
        outFlow = AddFlow("", ConnectionSide.Output);
    }

    [NodeTrigger]
    private bool shouldTrigger(bool value)
    {
        var shouldTrigger = previousValue && !value;
        previousValue = value;
        return shouldTrigger;
    }

    [NodeProcess(["Condition"], [""])]
    private void process(bool value) => SetFlow(outFlow);
}

[Node("Fire On Change", "Flow/Trigger")]
public sealed class FireOnChangeNode<T> : Node
{
    private readonly NodeFlowRef outFlow;
    private T? previousValue;

    public FireOnChangeNode()
    {
        outFlow = AddFlow("", ConnectionSide.Output);
    }

    [NodeTrigger]
    private bool shouldTrigger(T value)
    {
        var result = value is IEquatable<T> valueE ? valueE.Equals(previousValue) : EqualityComparer<T>.Default.Equals(value, previousValue);
        previousValue = value;
        return !result;
    }

    [NodeProcess(["*"], [""])]
    private void process(T value) => SetFlow(outFlow);
}