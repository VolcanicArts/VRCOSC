// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading;
using System.Threading.Tasks;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow.Triggers;

[Node("Fire On True", "Flow")]
public sealed class FireOnTrueNode : Node, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs => [new("On True")];

    [NodeProcess]
    private async Task process
    (
        CancellationToken token,
        [NodeValue("Condition")] [NodeReactive] bool condition
    )
    {
        if (condition) await TriggerFlow(token, 0);
    }
}