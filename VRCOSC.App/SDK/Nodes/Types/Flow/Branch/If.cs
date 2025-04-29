// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading;

namespace VRCOSC.App.SDK.Nodes.Types.Flow.Branch;

[Node("If", "Flow")]
public sealed class IfNode : Node, IFlowInput, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs =>
    [
        new("On True"),
        new("On False")
    ];

    [NodeProcess]
    private void process
    (
        CancellationToken token,
        [NodeValue("Condition")] bool condition
    )
    {
        TriggerFlow(token, condition ? 0 : 1);
    }
}