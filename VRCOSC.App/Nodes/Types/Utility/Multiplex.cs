// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Utility;

[Node("Conditional", "Utility")]
public sealed class ConditionalNode<T> : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Condition")] bool condition,
        [NodeValue("When True")] T valueTrue,
        [NodeValue("When False")] T valueFalse,
        [NodeValue("Output")] Ref<T> outOutput
    )
    {
        outOutput.Value = condition ? valueTrue : valueFalse;
    }
}

[Node("Multiplex", "Utility")]
public sealed class MultiplexNode<T> : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Index")] int index,
        [NodeValue("Value")] [NodeVariableSize] T[] inputs,
        [NodeValue("Element")] Ref<T> outElement,
        [NodeValue("Input Count")] Ref<int> outInputCount
    )
    {
        outInputCount.Value = inputs.Length;

        if (index >= inputs.Length) return;

        outElement.Value = inputs[index];
    }
}

[Node("Demultiplex", "Utility")]
public sealed class DemultiplexNode<T> : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Index")] int index,
        [NodeValue("Value")] T value,
        [NodeValue("Default Value")] T defaultValue,
        [NodeValue("Value")] [NodeVariableSize] Ref<T[]> outOutputs
    )
    {
        for (var i = 0; i < outOutputs.Value.Length; i++)
        {
            outOutputs.Value[i] = i == index ? value : defaultValue;
        }
    }
}