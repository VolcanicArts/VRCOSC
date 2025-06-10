// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Utility;

[Node("Indirect Write Variable", "Utility")]
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

[Node("Indirect Read Variable", "Utility")]
public sealed class IndirectReadVariableNode<T> : Node
{
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
    }
}