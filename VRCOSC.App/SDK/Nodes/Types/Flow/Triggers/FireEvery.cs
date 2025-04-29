// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading;
using System.Threading.Tasks;

namespace VRCOSC.App.SDK.Nodes.Types.Flow.Triggers;

[Node("Fire Every", "Flow")]
public sealed class FireEveryNode : Node, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs => [new()];

    [NodeProcess]
    private async Task process
    (
        CancellationToken token,
        [NodeValue("Interval Milliseconds")] [NodeReactive] int milliseconds
    )
    {
        if (milliseconds == 0) return;

        while (true)
        {
            await TriggerFlow(token, 0);
            await Task.Delay(milliseconds, token);
        }
    }
}