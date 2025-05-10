// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading;
using System.Threading.Tasks;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow.Triggers;

[Node("Fire While False", "Flow")]
public sealed class FireWhileFalseNode : Node, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs => [new("Is False")];

    [NodeProcess]
    private async Task process
    (
        CancellationToken token,
        [NodeValue("Interval Milliseconds")] [NodeReactive] int milliseconds,
        [NodeValue("Condition")] [NodeReactive] bool condition
    )
    {
        if (condition || milliseconds == 0) return;

        while (!token.IsCancellationRequested)
        {
            await TriggerFlow(token, 0, true);
            if (token.IsCancellationRequested) break;

            await Task.Delay(milliseconds, token);
            if (token.IsCancellationRequested) break;
        }
    }
}