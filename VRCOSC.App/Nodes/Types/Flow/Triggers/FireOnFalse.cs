// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow.Triggers;

[Node("Fire On False", "Flow")]
public sealed class FireOnFalseNode : Node, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs => [new("On False")];

    [NodeProcess]
    private async Task process
    (
        FlowContext context,
        [NodeValue("Condition")] [NodeReactive] bool condition
    )
    {
        if (!condition) await TriggerFlow(context, 0, true);
    }
}