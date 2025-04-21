// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Flow;

[Node("Sequence", "Flow")]
public sealed class FlowSequenceNode : Node
{
    public FlowSequenceNode()
    {
        AddFlow("*", ConnectionSide.Input);
        AddFlow("*", ConnectionSide.Output);
        AddFlow("*", ConnectionSide.Output);
    }

    internal void AddOutFlow() => AddFlow("*", ConnectionSide.Output);

    private int currentFlowPosition;

    [NodeProcess([], [])]
    private void process()
    {
        SetFlow(GetFlowAt(currentFlowPosition, ConnectionSide.Output));
    }
}

[Node("Passthrough", "")]
public sealed class PassthroughNode<T> : Node
{
    [NodeProcess([""], [""])]
    private T process(T value) => value;
}