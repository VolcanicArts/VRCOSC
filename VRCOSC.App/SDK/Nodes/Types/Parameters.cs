// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Parameters;

namespace VRCOSC.App.SDK.Nodes.Types;

[Node("On Registered Parameter Received")]
[NodeFlowInput(true)]
[NodeFlowOutput("")]
[NodeValueOutput([typeof(RegisteredParameter)], ["Parameter"])]
public class ParameterReceivedTriggerNode : TriggerNode
{
    [NodeProcess]
    private int process()
    {
        return 0;
        // if parameter has been received, trigger 0, else nothing
    }
}

[Node("Get Parameter")]
[NodeFlowInput]
[NodeFlowOutput("")]
[NodeValueInput("Parameter Name")]
[NodeValueOutput([typeof(ReceivedParameter)], ["Parameter"])]
public class GetParameterNode : Node
{
    [NodeProcess]
    private int process(string parameterName)
    {
        return 0;
    }
}

[Node("Check Parameter Validity")]
[NodeFlowInput]
[NodeFlowOutput("Is Valid", "Just Became Valid", "Just Became Invalid")]
[NodeValueInput("Parameter")]
public class CheckParameterValidity : Node
{
    [NodeProcess]
    private int process(ReceivedParameter parameter, bool condition)
    {
        return 0;
        // if condition was false, now true, branch to just became valid
    }
}