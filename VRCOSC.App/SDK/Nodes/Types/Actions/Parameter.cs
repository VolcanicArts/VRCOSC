// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading;
using System.Threading.Tasks;
using VRCOSC.App.OSC.VRChat;

namespace VRCOSC.App.SDK.Nodes.Types.Actions;

[Node("Send Parameter", "Actions")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public class SendParameterNode<T> : Node, IFlowInput
{
    [NodeProcess]
    private Task process
    (
        CancellationToken _,
        [NodeValue("Parameter Name")] string name,
        [NodeValue("Value")] T value
    )
    {
        AppManager.GetInstance().VRChatOscClient.Send($"{VRChatOscConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}{name}", value);
        return Task.CompletedTask;
    }
}