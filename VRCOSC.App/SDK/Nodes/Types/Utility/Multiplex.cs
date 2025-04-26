// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Utility;

[Node("Boolean Multiplex", "Utility")]
public sealed class BooleanMultiplexNode<T> : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("When True")] T valueTrue,
        [NodeValue("When False")] T valueFalse,
        [NodeValue("Condition")] bool condition,
        [NodeValue("Output")] ref T outOutput
    )
    {
        outOutput = condition ? valueTrue : valueFalse;
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
        [NodeValue("Element")] ref T element,
        [NodeValue("Input Count")] ref int outInputCount
    )
    {
        outInputCount = inputs.Length;

        if (index >= inputs.Length) return;

        element = inputs[index];
    }
}

[Node("Demultiplex", "Utility")]
public sealed class DemultiplexNode<T> : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Value")] T value,
        [NodeValue("Default Value")] T defaultValue,
        [NodeValue("Index")] int index,
        [NodeValue("Outputs")] [NodeVariableSize] ref T[] outOutputs
    )
    {
        for (var i = 0; i < outOutputs.Length; i++)
        {
            outOutputs[i] = i == index ? value : defaultValue;
        }
    }
}