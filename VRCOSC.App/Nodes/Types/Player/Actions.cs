// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.OSC.VRChat;

namespace VRCOSC.App.Nodes.Types.Player;

[Node("Mute Set", "Player/Actions")]
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

[Node("Mute Toggle", "Player/Actions")]
public sealed class PlayerMuteToggleNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    protected override async Task Process(PulseContext c)
    {
        c.GetPlayer().ToggleVoice();
        await Next.Execute(c);
    }
}

[Node("Push To Talk", "Player/Actions")]
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

[Node("Jump", "Player/Actions")]
public sealed class PlayerJumpNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    protected override async Task Process(PulseContext c)
    {
        c.GetPlayer().Jump();
        await Next.Execute(c);
    }
}

[Node("Look Horizontal", "Player/Actions")]
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

[Node("Move Vertical", "Player/Actions")]
public sealed class PlayerMoveVerticalNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<float> Percentage = new();

    protected override async Task Process(PulseContext c)
    {
        var percentage = Percentage.Read(c);
        percentage = float.Clamp(percentage, 0f, 1f);

        c.GetPlayer().MoveVertical(percentage);
        await Next.Execute(c);
    }
}

[Node("Move Horizontal", "Player/Actions")]
public sealed class PlayerMoveHorizontalNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<float> Percentage = new();

    protected override async Task Process(PulseContext c)
    {
        var percentage = Percentage.Read(c);
        percentage = float.Clamp(percentage, 0f, 1f);

        c.GetPlayer().MoveHorizontal(percentage);
        await Next.Execute(c);
    }
}

[Node("Set Run", "Player/Actions")]
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

[Node("Change Avatar", "Player/Actions")]
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