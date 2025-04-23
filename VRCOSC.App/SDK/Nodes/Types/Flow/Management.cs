// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Flow;

[Node("Sequence", "Flow")]
public sealed class FlowSequenceNode : Node, IFlowInput, IFlowOutput
{
    [NodeVariableSize(2)]
    public NodeFlowRef[] FlowOutputs { get; set; } = [];

    private int currentFlowPosition;

    [NodeProcess]
    private int process()
    {
        if (currentFlowPosition == FlowOutputs.Length)
        {
            currentFlowPosition = 0;
            return -1;
        }

        return currentFlowPosition++;
    }
}

[Node("Value Relay", "")]
public sealed class ValueRelayNode<T> : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] T value,
        [NodeValue] ref T outValue
    )
    {
        outValue = value;
    }
}