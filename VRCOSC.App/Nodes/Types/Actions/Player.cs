// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading;
using System.Threading.Tasks;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Actions;

[Node("Mute Set", "Actions/Player")]
public sealed class VRChatPlayerMuteSetActionNode : Node, IFlowInput
{
    [NodeProcess]
    private Task process
    (
        CancellationToken _,
        [NodeValue("Muted")] bool muted
    )
    {
        if (muted)
            Player.Mute();
        else
            Player.UnMute();

        return Task.CompletedTask;
    }
}

[Node("Mute Toggle", "Actions/Player")]
public sealed class VRChatPlayerMuteToggleActionNode : Node, IFlowInput
{
    [NodeProcess]
    private Task process(CancellationToken _)
    {
        Player.ToggleVoice();
        return Task.CompletedTask;
    }
}