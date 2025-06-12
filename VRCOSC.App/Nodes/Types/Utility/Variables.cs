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

        NodeField.WriteVariable(name, Value.Read(c), Persistent.Read(c));
        OnWrite.Execute(c);
    }
}

[Node("Direct Write Variable", "Variables")]
public sealed class DirectWriteVariableNode<T> : Node, IFlowInput
{
    [NodeProperty("test")]
    public string Name { get; set; } = string.Empty;

    public FlowContinuation OnWrite = new("On Write");

    public ValueInput<T> Value = new();
    public ValueInput<bool> Persistent = new();

    protected override void Process(PulseContext c)
    {
        if (string.IsNullOrEmpty(Name)) return;

        NodeField.WriteVariable(Name, Value.Read(c), Persistent.Read(c));
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

        if (NodeField.Variables.TryGetValue(name, out var valueRef))
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
public sealed class VariableSourceNode<T> : Node
{
    [NodeProperty("test")]
    public string Name { get; set; } = string.Empty;

    public ValueOutput<T> Value = new();

    protected override void Process(PulseContext c)
    {
        if (string.IsNullOrEmpty(Name)) return;

        var value = default(T);

        if (NodeField.Variables.TryGetValue(Name, out var valueRef))
        {
            var foundValue = valueRef.GetValue()!;
            if (foundValue is not T foundValueCast) return;

            value = foundValueCast;
        }

        Value.Write(value!, c);
    }
}