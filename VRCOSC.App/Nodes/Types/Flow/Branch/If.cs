// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow.Branch;

[Node("If", "Flow")]
public sealed class IfNode : Node, IFlowInput, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs =>
    [
        new("On True"),
        new("On False")
    ];

    [NodeProcess]
    private Task process
    (
        FlowContext context,
        [NodeValue("Condition")] bool condition
    )
    {
        return TriggerFlow(context, condition ? 0 : 1);
    }
}