// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Base;

[Node("Value", "Values")]
public sealed class ValueNode<T> : Node
{
    public T Value { get; set; } = default!;

    [NodeProcess]
    private void process
    (
        [NodeValue] ref T outValue
    )
    {
        outValue = Value;
    }
}

public abstract class ConstantNode<T> : Node
{
    public required T Value { get; init; }

    [NodeProcess]
    private void process
    (
        [NodeValue] ref T outValue
    )
    {
        outValue = Value;
    }
}

public abstract class InputNode : Node
{
}