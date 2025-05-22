// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes;

public static class Extensions
{
    public static int VirtualValueInputCount(this Node node)
    {
        if (node.Metadata.Inputs.Length == 0) return 0;
        if (!node.Metadata.ValueInputHasVariableSize) return node.Metadata.Inputs.Length;

        return node.Metadata.Inputs.Length - 1 + node.VariableSize.ValueInputSize;
    }

    public static int VirtualValueOutputCount(this Node node)
    {
        if (node.Metadata.Outputs.Length == 0) return 0;
        if (!node.Metadata.ValueOutputHasVariableSize) return node.Metadata.Outputs.Length;

        return node.Metadata.Outputs.Length - 1 + node.VariableSize.ValueOutputSize;
    }

    public static Type GetTypeOfInputSlot(this Node node, int index)
    {
        if (node.Metadata.InputsCount == 0) throw new Exception($"Cannot get type of input slot when there are no inputs {node.Metadata.Title}");
        if (!node.Metadata.ValueInputHasVariableSize && index >= node.Metadata.InputsCount) throw new IndexOutOfRangeException();
        if (node.Metadata.ValueInputHasVariableSize && index >= node.VirtualValueInputCount()) throw new IndexOutOfRangeException();

        if (node.Metadata.ValueInputHasVariableSize)
        {
            if (index >= node.Metadata.InputsCount - 1) return node.Metadata.Inputs.Last().Type;
        }

        return node.Metadata.Inputs[index].Type;
    }

    public static Type GetTypeOfOutputSlot(this Node node, int index)
    {
        if (node.Metadata.OutputsCount == 0) throw new Exception("Cannot get type of output slot when there are no outputs");
        if (!node.Metadata.ValueOutputHasVariableSize && index >= node.Metadata.OutputsCount) throw new IndexOutOfRangeException();
        if (node.Metadata.ValueOutputHasVariableSize && index >= node.VirtualValueOutputCount()) throw new IndexOutOfRangeException();

        if (node.Metadata.ValueOutputHasVariableSize)
        {
            if (index >= node.Metadata.OutputsCount - 1) return node.Metadata.Outputs.Last().Type;
        }

        return node.Metadata.Outputs[index].Type;
    }
}