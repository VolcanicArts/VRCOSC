// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types;

[Node("Flow Spit")]
[NodeFlowInput]
[NodeFlowOutput("1", "2", "3", "4")]
[NodeFlowLoop(0, 1, 2)]
public class FlowSpitNode : Node
{
    private int currentFlow;

    [NodeProcess]
    private int process()
    {
        var returnFlow = currentFlow;
        currentFlow++;
        return returnFlow;
    }
}