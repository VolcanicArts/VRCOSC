// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.App.SDK.Nodes.Types.Flow;

[Node("Delay", "Flow")]
[NodeFlowInput]
[NodeFlowOutput("")]
[NodeValueInput("Delay")]
public class DelayNode : Node
{
    [NodeProcess]
    private async Task process(TimeSpan delay)
    {
        await Task.Delay(delay);
        SetFlow(0);
    }
}