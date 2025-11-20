// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.VRChat.Player;

[Node("Mute Set", "VRChat/Player/Actions")]
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

[Node("Mute Toggle", "VRChat/Player/Actions")]
public sealed class PlayerMuteToggleNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    protected override async Task Process(PulseContext c)
    {
        c.GetPlayer().ToggleVoice();
        await Next.Execute(c);
    }
}

[Node("Push To Talk", "VRChat/Player/Actions")]
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

[Node("Jump", "VRChat/Player/Actions")]
public sealed class PlayerJumpNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    protected override async Task Process(PulseContext c)
    {
        c.GetPlayer().Jump();
        await Next.Execute(c);
    }
}

[Node("Look Horizontal", "VRChat/Player/Actions")]
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

[Node("Move Vertical", "VRChat/Player/Actions")]
public sealed class PlayerMoveVerticalNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<float> Amount = new();

    protected override async Task Process(PulseContext c)
    {
        var percentage = Amount.Read(c);
        percentage = float.Clamp(percentage, -1f, 1f);

        c.GetPlayer().MoveVertical(percentage);
        await Next.Execute(c);
    }
}

[Node("Move Horizontal", "VRChat/Player/Actions")]
public sealed class PlayerMoveHorizontalNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<float> Amount = new();

    protected override async Task Process(PulseContext c)
    {
        var percentage = Amount.Read(c);
        percentage = float.Clamp(percentage, -1f, 1f);

        c.GetPlayer().MoveHorizontal(percentage);
        await Next.Execute(c);
    }
}

[Node("Set Run", "VRChat/Player/Actions")]
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

[Node("Change Avatar", "VRChat/Player/Actions")]
public sealed class PlayerChangeAvatarNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<string> AvatarId = new("Avatar Id");

    protected override async Task Process(PulseContext c)
    {
        var avatarId = AvatarId.Read(c);

        if (!string.IsNullOrEmpty(avatarId))
        {
            AppManager.GetInstance().VRChatOscClient.Send($"{VRChatOSCConstants.ADDRESS_AVATAR_CHANGE}", avatarId);
        }

        await Next.Execute(c);
    }
}

[Node("Grab", "VRChat/Player/Actions")]
public sealed class PlayerGrabNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<Chirality> Chirality = new();

    protected override async Task Process(PulseContext c)
    {
        if (Chirality.Read(c) == Utils.Chirality.Left)
            c.GetPlayer().GrabLeft();
        else
            c.GetPlayer().GrabRight();

        await Next.Execute(c);
    }
}

[Node("Drop", "VRChat/Player/Actions")]
public sealed class PlayerDropNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<Chirality> Chirality = new();

    protected override async Task Process(PulseContext c)
    {
        if (Chirality.Read(c) == Utils.Chirality.Left)
            c.GetPlayer().DropLeft();
        else
            c.GetPlayer().DropRight();

        await Next.Execute(c);
    }
}

[Node("Use", "VRChat/Player/Actions")]
public sealed class PlayerUseNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<Chirality> Chirality = new();

    protected override async Task Process(PulseContext c)
    {
        if (Chirality.Read(c) == Utils.Chirality.Left)
            c.GetPlayer().UseLeft();
        else
            c.GetPlayer().UseRight();

        await Next.Execute(c);
    }
}

[Node("Enter Safe Mode", "VRChat/Player/Actions")]
public sealed class PlayerEnterSafeModeNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    protected override async Task Process(PulseContext c)
    {
        c.GetPlayer().EnableSafeMode();

        await Next.Execute(c);
    }
}

[Node("Toggle Quick Menu", "VRChat/Player/Actions")]
public sealed class PlayerToggleQuickMenuNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<Chirality> Chirality = new();

    protected override async Task Process(PulseContext c)
    {
        if (Chirality.Read(c) == Utils.Chirality.Left)
            c.GetPlayer().ToggleLeftQuickMenu();
        else
            c.GetPlayer().ToggleRightQuickMenu();

        await Next.Execute(c);
    }
}