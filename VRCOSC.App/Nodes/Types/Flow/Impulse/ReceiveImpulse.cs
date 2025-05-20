// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow.Impulse;

[Node("Receive Impulse", "Flow/Impulse")]
public sealed class ReceiveImpulseNode : Node, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs => [new("On Received")];

    public string? ImpulseName { get; set; }

    [NodeProcess]
    private Task process
    (
        FlowContext context
    ) => TriggerFlow(context, 0);
}