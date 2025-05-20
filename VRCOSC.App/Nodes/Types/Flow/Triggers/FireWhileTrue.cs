// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow.Triggers;

[Node("Fire While True", "Flow")]
public sealed class FireWhileTrueNode : Node, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs => [new("Is True")];

    [NodeProcess]
    private async Task process
    (
        FlowContext context,
        [NodeValue("Interval Milliseconds")] [NodeReactive] int milliseconds,
        [NodeValue("Condition")] [NodeReactive] bool condition
    )
    {
        if (!condition || milliseconds == 0) return;

        while (!context.Token.IsCancellationRequested)
        {
            await TriggerFlow(context, 0, true);
            if (context.Token.IsCancellationRequested) break;

            await Task.Delay(milliseconds, context.Token);
            if (context.Token.IsCancellationRequested) break;
        }
    }
}