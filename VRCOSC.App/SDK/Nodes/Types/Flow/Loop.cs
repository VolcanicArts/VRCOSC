// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;

namespace VRCOSC.App.SDK.Nodes.Types.Flow;

[Node("While", "Loop")]
[NodeFlowInput]
[NodeFlowOutput("Finished", "Loop")]
[NodeFlowLoop(1)]
[NodeValueInput("Condition")]
public class WhileNode : Node
{
    [NodeProcess]
    private void process(bool condition) => SetFlow(condition ? 1 : 0);
}

[Node("For", "Loop")]
[NodeFlowInput]
[NodeFlowOutput("Finished", "Loop")]
[NodeFlowLoop(1)]
[NodeValueInput("Count")]
[NodeValueOutput("Index")]
public class ForNode : Node
{
    private const int finished_slot = 0;
    private const int loop_slot = 1;

    private int index;

    [NodeProcess]
    private int process(int count)
    {
        if (index < count)
        {
            SetFlow(loop_slot);
            return index++;
        }

        index = 0;
        SetFlow(finished_slot);
        return 0;
    }
}

[Node("For Each", "Loop")]
[NodeFlowInput]
[NodeFlowOutput("Finished", "Loop")]
[NodeFlowLoop(1)]
[NodeValueInput("List")]
[NodeValueOutput("Element")]
public class ForEachNode : Node
{
    private const int finished_slot = 0;
    private const int loop_slot = 1;

    private int index;

    [NodeProcess]
    private T process<T>(IEnumerable<T> enumerable)
    {
        if (index < enumerable.Count())
        {
            SetFlow(loop_slot);
            return enumerable.ElementAt(index++);
        }

        index = 0;
        SetFlow(finished_slot);
        return enumerable.Last();
    }
}