// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading;
using System.Threading.Tasks;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow.Triggers;

[Node("Fire While True", "Flow")]
public sealed class FireWhileTrueNode : Node, IFlowOutput
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
        if (!condition) return;

        if (milliseconds == 0) milliseconds = int.MaxValue;

        while (!token.IsCancellationRequested)
        {
            await TriggerFlow(token, 0);
            if (token.IsCancellationRequested) break;

            await Task.Delay(milliseconds, token);
            if (token.IsCancellationRequested) break;
        }
    }
}