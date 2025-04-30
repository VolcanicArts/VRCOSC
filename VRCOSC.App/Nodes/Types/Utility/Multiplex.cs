// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Utility;

[Node("Boolean Multiplex", "Utility")]
public sealed class BooleanMultiplexNode<T> : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("When True")] T valueTrue,
        [NodeValue("When False")] T valueFalse,
        [NodeValue("Condition")] bool condition,
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
        [NodeValue("Inputs")] [NodeVariableSize] T[] inputs,
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
        [NodeValue("Outputs")] [NodeVariableSize] Ref<T[]> outOutputs
    )
    {
        for (var i = 0; i < outOutputs.Value.Length; i++)
        {
            outOutputs.Value[i] = i == index ? value : defaultValue;
        }
    }
}