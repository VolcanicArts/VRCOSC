// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Parameters;

namespace VRCOSC.App.SDK.Nodes.Types.Flow;

[Node("On Registered Parameter Received", "")]
public sealed class RegisteredParameterReceivedNode<T> : Node
{
    private readonly RegisteredParameter registeredParameter;
    private readonly NodeFlowRef onReceivedFlow;

    public RegisteredParameterReceivedNode(RegisteredParameter registeredParameter)
    {
        this.registeredParameter = registeredParameter;

        onReceivedFlow = AddFlow($"On {registeredParameter.Name} Received", ConnectionSide.Output);
    }

    [NodeTrigger]
    private bool wasParameterReceived() => true;

    [NodeProcess([], ["Value"])]
    private T process()
    {
        SetFlow(onReceivedFlow);

        // TODO More checks here
        return (T)registeredParameter.Value;
    }
}

[Node("On Parameter Received", "Flow/Parameters")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public sealed class ParameterReceivedNode<T> : Node where T : struct
{
    private readonly NodeFlowRef onReceivedFlow;

    public ParameterReceivedNode()
    {
        onReceivedFlow = AddFlow("On Received", ConnectionSide.Output);
    }

    [NodeTrigger]
    private bool wasParameterReceived(string parameterName)
    {
        return true;
    }

    [NodeProcess(["Name"], ["Value"])]
    private T process(string parameterName)
    {
        SetFlow(onReceivedFlow);

        var receivedParameter = new ReceivedParameter(parameterName, default(T));
        return (T)receivedParameter.Value;
    }
}