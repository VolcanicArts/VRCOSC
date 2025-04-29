// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading;
using System.Threading.Tasks;

namespace VRCOSC.App.SDK.Nodes.Types.Flow.Impulse;

[Node("Send Impulse", "Flow/Impulse")]
public sealed class SendImpulseNode : Node, IFlowInput
{
    public string? ImpulseName { get; set; }

    [NodeProcess]
    private async Task process
    (
        CancellationToken token
    )
    {
        if (string.IsNullOrEmpty(ImpulseName)) return;

        await NodeScape.TriggerImpulse(token, ImpulseName);
    }
}