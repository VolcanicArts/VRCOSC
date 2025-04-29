// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading;
using System.Threading.Tasks;

namespace VRCOSC.App.SDK.Nodes.Types.Flow.Triggers;

[Node("Fire On Change", "Flow")]
public sealed class FireOnChangeNode<T> : Node, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs => [new()];

    [NodeProcess]
    private Task process
    (
        CancellationToken token,
        [NodeValue("Value")] [NodeReactive] T value
    ) => TriggerFlow(token, 0);
}