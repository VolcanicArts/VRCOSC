// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Utility;

[Node("Conditional", "Utility")]
public sealed class ConditionalNode<T> : Node
{
    public ValueInput<bool> Condition = new();
    public ValueInput<T> True = new();
    public ValueInput<T> False = new();
    public ValueOutput<T> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(Condition.Read(c) ? True.Read(c) : False.Read(c), c);
    }
}

[Node("Multiplex", "Utility")]
public sealed class MultiplexNode<T> : Node
{
    public ValueInput<int> Index = new();
    public ValueInputList<T> Inputs = new();
    public ValueOutput<T> Element = new();
    public ValueOutput<int> InputCount = new("Input Count");

    protected override void Process(PulseContext c)
    {
        var index = Index.Read(c);
        var inputs = Inputs.Read(c);
        InputCount.Write(inputs.Count, c);

        if (index >= inputs.Count) return;

        Element.Write(inputs[index], c);
    }
}

[Node("Demultiplex", "Utility")]
public sealed class DemultiplexNode<T> : Node
{
    public ValueInput<int> Index = new();
    public ValueInput<T> Value = new();
    public ValueInput<T> DefaultValue = new("Default Value");
    public ValueOutputList<T> Outputs = new();

    protected override void Process(PulseContext c)
    {
        var index = Index.Read(c);
        var value = Value.Read(c);
        var defaultValue = DefaultValue.Read(c);

        for (var i = 0; i < Outputs.Length(c); i++)
        {
            Outputs.Write(i, i == index ? value : defaultValue, c);
        }
    }
}