// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Utility;

[Node("Indirect Write Variable", "Variables")]
public sealed class IndirectWriteVariableNode<T> : Node, IFlowInput
{
    public FlowContinuation OnWrite = new("On Write");

    public ValueInput<string> Name = new(string.Empty);
    public ValueInput<T> Value = new();
    public ValueInput<bool> Persistent = new();

    protected override void Process(PulseContext c)
    {
        var name = Name.Read(c);
        if (string.IsNullOrEmpty(name)) return;

        NodeGraph.WriteVariable(name, Value.Read(c), Persistent.Read(c));
        OnWrite.Execute(c);
    }
}

[Node("Direct Write Variable", "Variables")]
public sealed class DirectWriteVariableNode<T> : Node, IFlowInput, IHasTextProperty
{
    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    public FlowContinuation OnWrite = new("On Write");

    public ValueInput<T> Value = new();
    public ValueInput<bool> Persistent = new();

    protected override void Process(PulseContext c)
    {
        if (string.IsNullOrEmpty(Text)) return;

        NodeGraph.WriteVariable(Text, Value.Read(c), Persistent.Read(c));
        OnWrite.Execute(c);
    }
}

[Node("Read Variable", "Variables")]
public sealed class ReadVariableNode<T> : Node, IFlowInput
{
    public FlowContinuation OnRead = new("On Read");

    public ValueInput<string> Name = new(string.Empty);
    public ValueOutput<T> Value = new();

    protected override void Process(PulseContext c)
    {
        var name = Name.Read(c);

        if (string.IsNullOrEmpty(name)) return;

        var value = default(T);

        if (NodeGraph.Variables.TryGetValue(name, out var valueRef))
        {
            var foundValue = valueRef.GetValue()!;
            if (foundValue is not T foundValueCast) return;

            value = foundValueCast;
        }

        Value.Write(value!, c);
        OnRead.Execute(c);
    }
}

[Node("Variable Source", "Variables")]
[NodeForceReprocess]
public sealed class VariableSourceNode<T> : Node, IHasTextProperty
{
    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    public ValueOutput<T> Value = new();

    protected override void Process(PulseContext c)
    {
        if (string.IsNullOrEmpty(Text)) return;

        var value = default(T);

        if (NodeGraph.Variables.TryGetValue(Text, out var valueRef))
        {
            var foundValue = valueRef.GetValue()!;
            if (foundValue is not T foundValueCast) return;

            value = foundValueCast;
        }

        Value.Write(value!, c);
    }
}