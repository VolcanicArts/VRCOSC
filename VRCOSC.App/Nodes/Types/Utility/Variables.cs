// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using VRCOSC.App.Nodes.Variables;

namespace VRCOSC.App.Nodes.Types.Utility;

[Node("Direct Write Variable")]
public sealed class DirectWriteVariableNode<T> : Node, IFlowInput, IHasVariableReference
{
    public override string DisplayName => $"{base.DisplayName}\n{graphVariable.Name.Value}";

    private GraphVariable<T> graphVariable => (GraphVariable<T>)NodeGraph.GraphVariables[VariableId];

    [NodeProperty("variable_id")]
    public Guid VariableId { get; set; }

    public FlowContinuation OnWrite = new("On Write");

    public ValueInput<T> Value = new();

    protected override async Task Process(PulseContext c)
    {
        NodeGraph.WriteVariable(graphVariable, Value.Read(c));
        await OnWrite.Execute(c);
    }
}

[Node("Indirect Write Variable", "Variables")]
public sealed class IndirectWriteVariableNode<T> : Node, IFlowInput
{
    public FlowContinuation OnWrite = new("On Write");

    public ValueInput<GraphVariable<T>> Reference = new();
    public ValueInput<T> Value = new();

    protected override async Task Process(PulseContext c)
    {
        var reference = Reference.Read(c);
        if (reference is null) return;

        NodeGraph.WriteVariable(reference, Value.Read(c));
        await OnWrite.Execute(c);
    }
}

[Node("Variable Reference")]
public sealed class VariableReferenceNode<T> : Node, IHasVariableReference
{
    public override string DisplayName => $"{base.DisplayName}\n{graphVariable.Name.Value}";

    private GraphVariable<T> graphVariable => (GraphVariable<T>)NodeGraph.GraphVariables[VariableId];

    [NodeProperty("variable_id")]
    public Guid VariableId { get; set; }

    public ValueOutput<GraphVariable<T>> Reference = new();

    protected override Task Process(PulseContext c)
    {
        Reference.Write(graphVariable, c);
        return Task.CompletedTask;
    }
}

[Node("Variable Source")]
[NodeForceReprocess]
public sealed class VariableSourceNode<T> : Node, IHasVariableReference
{
    public override string DisplayName => $"{base.DisplayName}\n{graphVariable.Name.Value}";

    private GraphVariable<T> graphVariable => (GraphVariable<T>)NodeGraph.GraphVariables[VariableId];

    [NodeProperty("variable_id")]
    public Guid VariableId { get; set; }

    public ValueOutput<T> Value = new();

    protected override Task Process(PulseContext c)
    {
        Value.Write(graphVariable.Value.Value, c);
        return Task.CompletedTask;
    }
}