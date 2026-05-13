// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Utility;

[Node("Drive Variable")]
public sealed class DriveVariableNode<T> : Node, IUpdateNode, IHasVariableReference
{
    public int UpdateOffset => 1;
    public override string DisplayName => $"{base.DisplayName}\n{graphVariable.Name.Value}";

    private GraphVariable<T> graphVariable => (GraphVariable<T>)NodeGraph.GraphVariables[VariableId];

    public GlobalStore<T> CurrValue = new();

    [NodeProperty("variable_id")]
    public Guid VariableId { get; set; }

    public ValueInput<T> Value = new();

    protected override Task Process(PulseContext c)
    {
        CurrValue.Write(Value.Read(c), c);
        return Task.CompletedTask;
    }

    public void OnUpdate(PulseContext c) => graphVariable.Write(CurrValue.Read(c));
}

[Node("Direct Write Variable")]
public sealed class DirectWriteVariableNode<T> : ActionNode, IHasVariableReference
{
    public override string DisplayName => $"{base.DisplayName}\n{graphVariable.Name.Value}";

    private GraphVariable<T> graphVariable => (GraphVariable<T>)NodeGraph.GraphVariables[VariableId];

    [NodeProperty("variable_id")]
    public Guid VariableId { get; set; }

    public ValueInput<T> Value = new();

    protected override void DoAction(PulseContext c) => graphVariable.Write(Value.Read(c));
}

[Node("Indirect Write Variable", "Variables")]
public sealed class IndirectWriteVariableNode<T> : ActionNode
{
    public ValueInput<GraphVariable<T>> Reference = new();
    public ValueInput<T> Value = new();

    protected override void DoAction(PulseContext c) => Reference.Read(c)?.Write(Value.Read(c));
}

[Node("Variable Reference")]
public sealed class VariableReferenceNode<T>() : ValueComputeNode<GraphVariable<T>>("Reference"), IHasVariableReference
{
    public override string DisplayName => $"{base.DisplayName}\n{graphVariable.Name.Value}";

    private GraphVariable<T> graphVariable => (GraphVariable<T>)NodeGraph.GraphVariables[VariableId];

    [NodeProperty("variable_id")]
    public Guid VariableId { get; set; }

    protected override GraphVariable<T> ComputeValue(PulseContext c) => graphVariable;
}

[Node("Variable Reference To Value", "Variables")]
[NodeForceReprocess]
public sealed class VariableReferenceToValueNode<T>() : SimpleValueTransformNode<GraphVariable<T>?, T>(r => r is null ? default! : r.Value.Value, "Reference", "Value"), IContinuousNode
{
    public int UpdateOffset => -1;
}

[Node("Variable Source")]
[NodeForceReprocess]
public sealed class VariableSourceNode<T>() : ValueSourceNode<T>("Value"), IHasVariableReference
{
    public override int UpdateOffset => -1;

    public override string DisplayName => $"{base.DisplayName}\n{graphVariable.Name.Value}";

    private GraphVariable<T> graphVariable => (GraphVariable<T>)NodeGraph.GraphVariables[VariableId];

    [NodeProperty("variable_id")]
    public Guid VariableId { get; set; }

    protected override T ComputeValue(PulseContext c) => graphVariable.Value.Value;
}