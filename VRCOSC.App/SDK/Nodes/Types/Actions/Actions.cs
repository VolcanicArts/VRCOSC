// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Actions;

[Node("Mute Set", "Actions/Player")]
public sealed class VRChatPlayerMuteSetActionNode : Node, IFlowInput
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Muted")] bool muted
    )
    {
        if (muted)
            Player.Mute();
        else
            Player.UnMute();
    }
}

[Node("Mute Toggle", "Actions/Player")]
public sealed class VRChatPlayerMuteToggleActionNode : Node, IFlowInput
{
    [NodeProcess]
    private void process()
    {
        Player.ToggleVoice();
    }
}