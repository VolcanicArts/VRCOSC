// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types;

[Node("Trigger")]
[NodeFlowInput(true)]
[NodeFlowOutput("Trigger")]
public class TriggerNode : Node
{
    [NodeProcess]
    private int process() => 0;
}