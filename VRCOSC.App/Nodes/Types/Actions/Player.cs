// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.OSC.VRChat;

namespace VRCOSC.App.Nodes.Types.Actions;

[Node("Mute Set", "Actions/Player")]
public sealed class PlayerMuteSetNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<bool> Muted = new();

    protected override async Task Process(PulseContext c)
    {
        if (Muted.Read(c))
            c.GetPlayer().Mute();
        else
            c.GetPlayer().UnMute();

        await Next.Execute(c);
    }
}

[Node("Mute Toggle", "Actions/Player")]
public sealed class PlayerMuteToggleNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    protected override async Task Process(PulseContext c)
    {
        c.GetPlayer().ToggleVoice();
        await Next.Execute(c);
    }
}

[Node("Push To Talk", "Actions/Player")]
public sealed class PlayerPushToTalkNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<bool> Active = new();

    protected override async Task Process(PulseContext c)
    {
        c.GetPlayer().PushToTalk(Active.Read(c));
        await Next.Execute(c);
    }
}

[Node("Jump", "Actions/Player")]
public sealed class PlayerJumpNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    protected override async Task Process(PulseContext c)
    {
        c.GetPlayer().Jump();
        await Next.Execute(c);
    }
}

[Node("Look Horizontal", "Actions/Player")]
public sealed class PlayerLookHorizontalNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<float> Angle = new();

    protected override async Task Process(PulseContext c)
    {
        c.GetPlayer().LookHorizontal(Angle.Read(c));
        await Next.Execute(c);
    }
}

[Node("Move Vertical", "Actions/Player")]
public sealed class PlayerMoveVerticalNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<float> Percentage = new();

    protected override async Task Process(PulseContext c)
    {
        c.GetPlayer().MoveVertical(Percentage.Read(c));
        await Next.Execute(c);
    }
}

[Node("Move Horizontal", "Actions/Player")]
public sealed class PlayerMoveHorizontalNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<float> Percentage = new();

    protected override async Task Process(PulseContext c)
    {
        c.GetPlayer().MoveHorizontal(Percentage.Read(c));
        await Next.Execute(c);
    }
}

[Node("Set Run", "Actions/Player")]
public sealed class PlayerSetRunNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<bool> Run = new();

    protected override async Task Process(PulseContext c)
    {
        if (Run.Read(c))
            c.GetPlayer().Run();
        else
            c.GetPlayer().StopRun();

        await Next.Execute(c);
    }
}

[Node("Change Avatar", "Actions/Player")]
public sealed class PlayerChangeAvatarNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<string> AvatarId = new("Avatar Id");

    protected override Task Process(PulseContext c)
    {
        var avatarId = AvatarId.Read(c);
        if (string.IsNullOrEmpty(avatarId)) return Task.CompletedTask;

        AppManager.GetInstance().VRChatOscClient.Send($"{VRChatOSCConstants.ADDRESS_AVATAR_CHANGE}", avatarId);
        return Task.CompletedTask;
    }
}