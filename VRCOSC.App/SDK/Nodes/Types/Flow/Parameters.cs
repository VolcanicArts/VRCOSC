// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading;
using System.Threading.Tasks;
using VRCOSC.App.SDK.Parameters;

namespace VRCOSC.App.SDK.Nodes.Types.Flow;

[Node("On Registered Parameter Received", "")]
public sealed class RegisteredParameterReceivedNode<T> : Node, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs { get; }

    private readonly RegisteredParameter registeredParameter;

    public RegisteredParameterReceivedNode(RegisteredParameter registeredParameter)
    {
        this.registeredParameter = registeredParameter;

        FlowOutputs = [new($"On {registeredParameter.Name} Received")];
    }

    [NodeProcess]
    private async Task process
    (
        CancellationToken token,
        [NodeValue("Value")] Ref<T> outValue
    )
    {
        outValue.Value = (T)registeredParameter.Value;
        await TriggerFlow(token, 0);
    }
}

[Node("On Parameter Received", "Flow")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public sealed class ParameterReceivedNode<T> : Node, IFlowOutput, IAnyParameterReceiver where T : struct
{
    public NodeFlowRef[] FlowOutputs => [new("On Received")];

    private ReceivedParameter? lastReceivedParameter;

    public void OnAnyParameterReceived(ReceivedParameter parameter)
    {
        lastReceivedParameter = parameter;
        TriggerSelf();
    }

    [NodeProcess]
    private async Task process
    (
        CancellationToken token,
        [NodeValue("Parameter Name")] string? parameterName,
        [NodeValue("Value")] Ref<T> outValue
    )
    {
        if (string.IsNullOrEmpty(parameterName)) return;
        if (lastReceivedParameter is null) return;

        if (lastReceivedParameter.Name != parameterName || lastReceivedParameter.Type != ParameterTypeFactory.CreateFrom<T>())
        {
            lastReceivedParameter = null;
            return;
        }

        outValue.Value = (T)lastReceivedParameter.Value;
        await TriggerFlow(token, 0);
        lastReceivedParameter = null;
    }
}