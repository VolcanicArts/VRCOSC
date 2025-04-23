// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.SDK.Nodes.Types.Flow;

[Node("Delay", "Flow")]
public class DelayNode : Node, IFlowInput, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs { get; set; } = new NodeFlowRef[1];

    [NodeProcess]
    private async Task<int> process
    (
        [NodeValue("Delay Milliseconds")] int delay
    )
    {
        await Task.Delay(delay);
        return 0;
    }
}