// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types;

[Node("While")]
[NodeFlowInput]
[NodeFlowOutput("Finished", "Loop")]
[NodeFlowLoop(1)]
[NodeValueInput("Condition")]
public class WhileNode : Node
{
    [NodeProcess]
    private int process(bool condition) => condition ? 1 : 0;
}

[Node("For")]
[NodeFlowInput]
[NodeFlowOutput("Finished", "Loop")]
[NodeFlowLoop(1)]
[NodeValueInput("Count")]
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
            index++;
            return loop_slot;
        }

        index = 0;
        return finished_slot;
    }
}