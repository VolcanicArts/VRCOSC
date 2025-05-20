// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.SDK.Parameters;

namespace VRCOSC.App.Nodes.Types.Flow;

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
        FlowContext context,
        [NodeValue("Value")] Ref<T> outValue
    )
    {
        outValue.Value = (T)registeredParameter.Value;
        await TriggerFlow(context, 0);
    }
}

[Node("On Parameter Received", "Flow")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public sealed class OnParameterReceivedNode<T> : Node, IFlowOutput, IParameterReceiver where T : struct
{
    public NodeFlowRef[] FlowOutputs => [new("On Received")];

    private readonly ParameterType parameterType = ParameterTypeFactory.CreateFrom<T>();

    [NodeProcess]
    private async Task process
    (
        ParameterReceiverFlowContext context,
        [NodeValue("Name")] string? name,
        [NodeValue("Value")] Ref<T> outValue
    )
    {
        var parameter = context.Parameter;
        if (string.IsNullOrEmpty(name) || parameter.Name != name || parameter.Type != parameterType) return;

        outValue.Value = (T)parameter.Value;
        await TriggerFlow(context, 0);
    }
}