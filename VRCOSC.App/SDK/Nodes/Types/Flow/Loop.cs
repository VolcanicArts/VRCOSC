// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;

namespace VRCOSC.App.SDK.Nodes.Types.Flow;

[Node("While", "Loop")]
public class WhileNode : Node
{
    private readonly NodeFlowRef loopFlowRef;
    private readonly NodeFlowRef finishedFlowRef;

    public WhileNode()
    {
        AddFlow("*", ConnectionSide.Input);
        loopFlowRef = AddFlow("On Loop", ConnectionSide.Output, NodeFlowFlag.Loop);
        finishedFlowRef = AddFlow("On Finished", ConnectionSide.Output);
    }

    [NodeProcess(["Condition"], [])]
    private void process(bool condition) => SetFlow(condition ? loopFlowRef : finishedFlowRef);
}

[Node("For", "Loop")]
public class ForNode : Node
{
    private readonly NodeFlowRef loopFlowRef;
    private readonly NodeFlowRef finishedFlowRef;

    private int index;

    public ForNode()
    {
        AddFlow("*", ConnectionSide.Input);
        finishedFlowRef = AddFlow("On Finished", ConnectionSide.Output);
        loopFlowRef = AddFlow("On Loop", ConnectionSide.Output, NodeFlowFlag.Loop);
    }

    [NodeProcess(["Count"], ["Index"])]
    private int process(int count)
    {
        if (index < count)
        {
            SetFlow(loopFlowRef);
            return index++;
        }

        index = 0;
        SetFlow(finishedFlowRef);
        return -1;
    }
}

[Node("For Each", "Loop")]
public sealed class ForEachNode<T> : Node
{
    private readonly NodeFlowRef loopFlowRef;
    private readonly NodeFlowRef finishedFlowRef;

    private int index;

    public ForEachNode()
    {
        AddFlow("*", ConnectionSide.Input);
        finishedFlowRef = AddFlow("On Finished", ConnectionSide.Output);
        loopFlowRef = AddFlow("On Loop", ConnectionSide.Output, NodeFlowFlag.Loop);
    }

    [NodeProcess(["Enumerable"], ["Element"])]
    private T? process(IEnumerable<T> enumerable)
    {
        if (index < enumerable.Count())
        {
            SetFlow(loopFlowRef);
            return enumerable.ElementAt(index++);
        }

        index = 0;
        SetFlow(finishedFlowRef);
        return default;
    }
}