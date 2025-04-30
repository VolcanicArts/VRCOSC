// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Values;

[Node("Value", "Values")]
public sealed class ValueNode<T> : Node
{
    private T value = default!;

    public T Value
    {
        get => value;
        set
        {
            this.value = value;
            NodeScape.WalkForward(this);
        }
    }

    [NodeProcess]
    private void process
    (
        [NodeValue] Ref<T> outValue
    )
    {
        outValue.Value = Value;
    }
}