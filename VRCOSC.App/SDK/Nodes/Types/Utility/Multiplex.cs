// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Utility;

[Node("Multiplex", "Utility")]
public sealed class MultiplexNode<T> : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Index")] int index,
        [NodeValue("Inputs")] [NodeVariableSize(2)] T[] inputs,
        [NodeValue("Element")] ref T element,
        [NodeValue("Input Count")] ref int inputCount
    )
    {
        element = inputs[index];
        inputCount = inputs.Length;
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
        [NodeValue("Outputs")] [NodeVariableSize(2)] ref T[] outputs
    )
    {
        for (var i = 0; i < outputs.Length; i++)
        {
            outputs[i] = i == index ? value : defaultValue;
        }
    }
}