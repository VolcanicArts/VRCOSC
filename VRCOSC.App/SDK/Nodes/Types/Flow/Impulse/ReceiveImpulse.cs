// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading;
using System.Threading.Tasks;

namespace VRCOSC.App.SDK.Nodes.Types.Flow.Impulse;

[Node("Receive Impulse", "Flow/Impulse")]
public sealed class ReceiveImpulseNode : Node, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs => [new("On Received")];

    public string? ImpulseName { get; set; }

    [NodeProcess]
    private Task process
    (
        CancellationToken token
    ) => TriggerFlow(token, 0);
}