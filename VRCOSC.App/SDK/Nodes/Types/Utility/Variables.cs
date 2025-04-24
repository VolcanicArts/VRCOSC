// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Utility;

[Node("Write Variable", "Utility")]
public sealed class WriteVariableNode<T> : Node, IFlowInput, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs => [new("On Write")];

    [NodeProcess]
    private int process
    (
        [NodeValue("Name")] string? name,
        [NodeValue("Value")] T value,
        [NodeValue("Persistent")] bool persistent
    )
    {
        if (string.IsNullOrEmpty(name)) return -1;

        NodeScape.WriteVariable(name, value, persistent);

        return 0;
    }
}

[Node("Read Variable", "Utility")]
public sealed class ReadVariableNode<T> : Node, IFlowInput, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs => [new("On Read")];

    [NodeProcess]
    private int process
    (
        [NodeValue("Name")] string? name,
        [NodeValue("Value")] ref T value
    )
    {
        if (string.IsNullOrEmpty(name)) return -1;
        if (!NodeScape.Variables.TryGetValue(name, out var foundValue)) return 0;
        if (foundValue is not T foundValueCast) return -1;

        value = foundValueCast;
        return 0;
    }
}