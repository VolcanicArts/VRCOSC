// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Actions;

[Node("Mute Set", "Actions/Player")]
public sealed class VRChatPlayerMuteSetActionNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<bool> Muted = new();

    protected override void Process(PulseContext c)
    {
        if (Muted.Read(c))
            Player.Mute();
        else
            Player.UnMute();

        Next.Execute(c);
    }
}

[Node("Mute Toggle", "Actions/Player")]
public sealed class VRChatPlayerMuteToggleActionNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    protected override void Process(PulseContext c)
    {
        Player.ToggleVoice();
        Next.Execute(c);
    }
}