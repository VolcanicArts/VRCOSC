// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Utility;

[Node("Conditional", "Utility")]
public sealed class ConditionalNode<T> : ValueComputeNode<T>
{
    [InputMode(InputModes.Connection)]
    public ValueInput<bool> Condition = new();

    public ValueInput<T> True = new();
    public ValueInput<T> False = new();

    protected override T ComputeValue(IPulseContext c) => Condition.Read(c) ? True.Read(c) : False.Read(c);
}

[Node("Multiplex", "Utility")]
[NodeForceReprocess]
public sealed class MultiplexNode<T> : Node
{
    [InputMode(InputModes.Connection)]
    public ValueInput<int> Index = new();

    public ValueInputList<T> Inputs = new();
    public ValueOutput<T> Element = new();
    public ValueOutput<int> InputCount = new();

    protected override Task Process(IPulseContext c)
    {
        InputCount.Write(Inputs.Count, c);

        var index = Index.Read(c);
        if (index >= Inputs.Count || index < 0) return Task.CompletedTask;

        Element.Write(Inputs.Read(index, c), c);
        return Task.CompletedTask;
    }
}

[Node("Demultiplex", "Utility")]
[NodeForceReprocess]
public sealed class DemultiplexNode<T> : Node
{
    [InputMode(InputModes.Connection)]
    public ValueInput<int> Index = new();

    public ValueInput<T> Value = new();
    public ValueInput<T> DefaultValue = new();
    public ValueOutputList<T> Outputs = new();

    protected override Task Process(IPulseContext c)
    {
        var index = Index.Read(c);
        var value = Value.Read(c);
        var defaultValue = DefaultValue.Read(c);
        var length = Outputs.Count;

        for (var i = 0; i < length; i++)
        {
            Outputs.Write(i, i == index ? value : defaultValue, c);
        }

        return Task.CompletedTask;
    }
}