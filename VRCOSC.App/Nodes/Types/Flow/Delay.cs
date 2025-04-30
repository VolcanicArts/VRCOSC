// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading;
using System.Threading.Tasks;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Delay", "Flow")]
public sealed class DelayNode : Node, IFlowInput, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs => [new()];

    [NodeProcess]
    private async Task process
    (
        CancellationToken token,
        [NodeValue("Delay Milliseconds")] int delay
    )
    {
        await Task.Delay(delay, token);
        await TriggerFlow(token, 0);
    }
}