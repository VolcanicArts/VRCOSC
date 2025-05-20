// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Utility;

[Node("Write Variable", "Utility")]
public sealed class WriteVariableNode<T> : Node, IFlowInput, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs => [new("On Write")];

    [NodeProcess]
    private async Task process
    (
        FlowContext context,
        [NodeValue("Name")] string? name,
        [NodeValue("Value")] T value,
        [NodeValue("Persistent")] bool persistent
    )
    {
        if (string.IsNullOrEmpty(name)) return;

        NodeField.WriteVariable(name, value, persistent);
        await TriggerFlow(context, 0);
    }
}

[Node("Read Variable", "Utility")]
public sealed class ReadVariableNode<T> : Node, IFlowInput, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs => [new("On Read")];

    [NodeProcess]
    private async Task process
    (
        FlowContext context,
        [NodeValue("Name")] string? name,
        [NodeValue("Value")] Ref<T> outValue
    )
    {
        if (string.IsNullOrEmpty(name)) return;
        if (!NodeField.Variables.TryGetValue(name, out var foundValue)) return;
        if (foundValue is not T foundValueCast) return;

        outValue.Value = foundValueCast;
        await TriggerFlow(context, 0);
    }
}