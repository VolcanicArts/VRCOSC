// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Inputs;

[Node("Value", "Inputs")]
public sealed class ValueNode<T> : Node
{
    private T value = default!;

    [NodeProperty("value")]
    public T Value
    {
        get => value;
        set
        {
            this.value = value;
            NodeField.WalkForward(this);
        }
    }

    public ValueOutput<T> Output = new();

    protected override void Process(PulseContext c)
    {
        Output.Write(Value, c);
    }
}