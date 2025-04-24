// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes.Types.Base;

namespace VRCOSC.App.SDK.Nodes.Types.Inputs;

[Node("Button Input", "")]
public sealed class ButtonInputNode : InputNode, IFlowOutput, IFlowTrigger
{
    public NodeFlowRef[] FlowOutputs => [new("On Trigger")];

    public string Label => "Trigger";
    public bool Clicked { get; set; }

    public void OnClick() => Clicked = true;

    [NodeProcess]
    private int process()
    {
        if (!Clicked) return -1;

        Clicked = false;
        return 0;
    }
}