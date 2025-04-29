// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading;
using System.Threading.Tasks;

namespace VRCOSC.App.SDK.Nodes.Types.Flow.Triggers;

[Node("Fire While False", "Flow")]
public sealed class FireWhileFalseNode : Node, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs => [new()];

    [NodeProcess]
    private async Task process
    (
        CancellationToken token,
        [NodeValue("Interval Milliseconds")] [NodeReactive] int milliseconds,
        [NodeValue("Condition")] [NodeReactive] bool condition
    )
    {
        if (condition) return;

        while (!token.IsCancellationRequested)
        {
            await TriggerFlow(token, 0);
            if (token.IsCancellationRequested) break;

            await Task.Delay(milliseconds, token);
            if (token.IsCancellationRequested) break;
        }
    }
}