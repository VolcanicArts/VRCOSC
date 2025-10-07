// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Inputs;

[Node("Value")]
public class ValueNode<T> : Node
{
    private T value = default!;

    [NodeProperty("value")]
    public T Value
    {
        get => value;
        set
        {
            this.value = value;
            NodeGraph.TriggerTree(this);
        }
    }

    public ValueOutput<T> Output = new();

    protected override Task Process(PulseContext c)
    {
        Output.Write(Value, c);
        return Task.CompletedTask;
    }
}