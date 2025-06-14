// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Actions;

[Node("Mute Set", "Actions/Player")]
public sealed class PlayerMuteSetNode : Node, IFlowInput
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
public sealed class PlayerMuteToggleNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    protected override void Process(PulseContext c)
    {
        Player.ToggleVoice();
        Next.Execute(c);
    }
}

[Node("Jump", "Actions/Player")]
public sealed class PlayerJumpNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    protected override void Process(PulseContext c)
    {
        Player.Jump();
        Next.Execute(c);
    }
}

[Node("Look Horizontal", "Actions/Player")]
public sealed class PlayerLookHorizontalNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<float> Angle = new();

    protected override void Process(PulseContext c)
    {
        Player.LookHorizontal(Angle.Read(c));
        Next.Execute(c);
    }
}

[Node("Move Vertical", "Actions/Player")]
public sealed class PlayerMoveVerticalNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<float> Percentage = new();

    protected override void Process(PulseContext c)
    {
        Player.MoveVertical(Percentage.Read(c));
        Next.Execute(c);
    }
}

[Node("Move Horizontal", "Actions/Player")]
public sealed class PlayerMoveHorizontalNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<float> Percentage = new();

    protected override void Process(PulseContext c)
    {
        Player.MoveHorizontal(Percentage.Read(c));
        Next.Execute(c);
    }
}

[Node("Set Run", "Actions/Player")]
public sealed class PlayerSetRunNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<bool> Run = new();

    protected override void Process(PulseContext c)
    {
        if (Run.Read(c))
        {
            Player.Run();
        }
        else
        {
            Player.StopRun();
        }

        Next.Execute(c);
    }
}

[Node("Change Avatar", "Actions/Player")]
public sealed class PlayerChangeAvatarNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<string> AvatarId = new("Avatar Id");

    protected override void Process(PulseContext c)
    {
        var avatarId = AvatarId.Read(c);
        if (string.IsNullOrEmpty(avatarId)) return;

        AppManager.GetInstance().VRChatOscClient.Send($"{VRChatOSCConstants.ADDRESS_AVATAR_CHANGE}", avatarId);
    }
}