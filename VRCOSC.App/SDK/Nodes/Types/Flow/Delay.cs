// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.SDK.Nodes.Types.Flow;

[Node("Delay", "Flow")]
public class DelayNode : Node
{
    private readonly NodeFlowRef outFlow;

    public DelayNode()
    {
        AddFlow("*", ConnectionSide.Input);
        outFlow = AddFlow("*", ConnectionSide.Output);
    }

    [NodeProcess(["Delay Milliseconds"], [])]
    private async Task process(int delay)
    {
        await Task.Delay(delay);
        SetFlow(outFlow);
    }
}