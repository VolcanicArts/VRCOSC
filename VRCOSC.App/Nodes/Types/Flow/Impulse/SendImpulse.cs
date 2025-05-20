// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow.Impulse;

[Node("Send Impulse", "Flow/Impulse")]
public sealed class SendImpulseNode : Node, IFlowInput
{
    public string? ImpulseName { get; set; }

    [NodeProcess]
    private async Task process
    (
        FlowContext context
    )
    {
        if (string.IsNullOrEmpty(ImpulseName)) return;

        await NodeField.TriggerImpulse(context, ImpulseName);
    }
}