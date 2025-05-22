// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Utility;

[Node("Write Variable", "Utility")]
public sealed class WriteVariableNode<T> : Node, IFlowInput
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

[Node("Read Variable", "Utility")]
public sealed class ReadVariableNode<T> : Node, IFlowInput
{
    public FlowContinuation OnRead = new("On Read");

    public ValueInput<string> Name = new(string.Empty);
    public ValueOutput<T> Value = new();

    protected override void Process(PulseContext c)
    {
        var name = Name.Read(c);

        if (string.IsNullOrEmpty(name)) return;
        if (!NodeField.Variables.TryGetValue(name, out var foundValue)) return;
        if (foundValue is not T foundValueCast) return;

        Value.Write(foundValueCast, c);
        OnRead.Execute(c);
    }
}