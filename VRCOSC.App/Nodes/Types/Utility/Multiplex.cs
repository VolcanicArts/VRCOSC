// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Utility;

[Node("Conditional", "Utility")]
public sealed class ConditionalNode<T> : ValueComputeNode<T>
{
    public ValueInput<bool> Condition = new();
    public ValueInput<T> True = new();
    public ValueInput<T> False = new();

    protected override T ComputeValue(PulseContext c) => Condition.Read(c) ? True.Read(c) : False.Read(c);
}

[Node("Multiplex", "Utility")]
public sealed class MultiplexNode<T> : Node
{
    public ValueInput<int> Index = new();
    public ValueInputList<T> Inputs = new();
    public ValueOutput<T> Element = new();
    public ValueOutput<int> InputCount = new();

    protected override Task Process(PulseContext c)
    {
        var index = Index.Read(c);
        var inputs = Inputs.Read(c);
        InputCount.Write(inputs.Count, c);

        if (index >= inputs.Count) return Task.CompletedTask;

        Element.Write(inputs[index], c);
        return Task.CompletedTask;
    }
}

[Node("Demultiplex", "Utility")]
public sealed class DemultiplexNode<T> : Node
{
    public ValueInput<int> Index = new();
    public ValueInput<T> Value = new();
    public ValueInput<T> DefaultValue = new();
    public ValueOutputList<T> Outputs = new();

    protected override Task Process(PulseContext c)
    {
        var index = Index.Read(c);
        var value = Value.Read(c);
        var defaultValue = DefaultValue.Read(c);

        for (var i = 0; i < Outputs.Length(c); i++)
        {
            Outputs.Write(i, i == index ? value : defaultValue, c);
        }

        return Task.CompletedTask;
    }
}