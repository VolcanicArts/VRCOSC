// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Parameters;

namespace VRCOSC.App.SDK.Nodes.Types.Flow;

[Node("On Registered Parameter Received", "")]
public sealed class RegisteredParameterReceivedNode<T> : Node, IFlowOutput, IFlowTrigger
{
    public NodeFlowRef[] FlowOutputs { get; }

    private readonly RegisteredParameter registeredParameter;

    public RegisteredParameterReceivedNode(RegisteredParameter registeredParameter)
    {
        this.registeredParameter = registeredParameter;

        FlowOutputs = [new($"On {registeredParameter.Name} Received")];
    }

    [NodeProcess]
    private int process
    (
        [NodeValue("Value")] ref T outValue
    )
    {
        outValue = (T)registeredParameter.Value;
        return 0;
    }
}

[Node("On Parameter Received", "Flow")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public sealed class ParameterReceivedNode<T> : Node, IFlowOutput, IFlowTrigger where T : struct
{
    public NodeFlowRef[] FlowOutputs => [new("On Received")];

    [NodeProcess]
    private int process
    (
        [NodeValue("Parameter Name")] string? parameterName,
        [NodeValue("Value")] ref T outValue
    )
    {
        if (string.IsNullOrEmpty(parameterName)) return -1;

        var receivedParameter = new ReceivedParameter(parameterName, default(T));
        outValue = (T)receivedParameter.Value;
        return 0;
    }
}