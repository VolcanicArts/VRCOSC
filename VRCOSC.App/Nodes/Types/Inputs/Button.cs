// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.Nodes.Types.Base;

namespace VRCOSC.App.Nodes.Types.Inputs;

[Node("Button Input", "")]
public sealed class ButtonInputNode : InputNode, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs => [new("On Trigger")];

    public string Label => "Trigger";

    public void OnClick() => TriggerSelf();

    [NodeProcess]
    private Task process(FlowContext context) => TriggerFlow(context, 0);
}