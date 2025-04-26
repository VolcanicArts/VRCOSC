// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.SDK.Nodes.Targets;

namespace VRCOSC.App.SDK.Nodes.Types.Actions;

[Node("Send Parameter", "Actions")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public class SendParameterNode<T> : Node, IFlowInput
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Parameter Name")] string name,
        [NodeValue("Value")] T value
    )
    {
        AppManager.GetInstance().VRChatOscClient.Send($"{VRChatOscConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}{name}", value);
    }
}

[Node("Create Parameter Target", "Actions")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public sealed class CreateParameterTargetNode<T> : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Name")] string name,
        [NodeValue("Target")] ref ParameterTarget<T> outTarget)
    {
        outTarget = new ParameterTarget<T>(name);
    }
}