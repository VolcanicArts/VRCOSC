// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Flow;

[Node("Trigger", "Debug")]
[NodeFlowInput(true)]
[NodeFlowOutput("Trigger")]
public class TriggerNode : Node
{
    [NodeProcess]
    private void process() => SetFlow(0);
}