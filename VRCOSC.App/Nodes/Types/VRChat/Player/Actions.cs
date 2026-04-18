// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.VRChat.Player;

[Node("Mute Set", "VRChat/Player/Actions")]
public sealed class PlayerMuteSetNode : SimpleActionNode
{
    public ValueInput<bool> Muted = new();

    protected override void DoAction(PulseContext c)
    {
        if (Muted.Read(c))
            c.GetPlayer().Mute();
        else
            c.GetPlayer().UnMute();
    }
}

[Node("Mute Toggle", "VRChat/Player/Actions")]
public sealed class PlayerMuteToggleNode : SimpleActionNode
{
    protected override void DoAction(PulseContext c) => c.GetPlayer().ToggleVoice();
}

[Node("Push To Talk", "VRChat/Player/Actions")]
public sealed class PlayerPushToTalkNode : SimpleActionNode
{
    public ValueInput<bool> Active = new();

    protected override void DoAction(PulseContext c) => c.GetPlayer().PushToTalk(Active.Read(c));
}

[Node("Jump", "VRChat/Player/Actions")]
public sealed class PlayerJumpNode : SimpleActionNode
{
    protected override void DoAction(PulseContext c) => c.GetPlayer().Jump();
}

[Node("Look Vertical", "VRChat/Player/Actions")]
public sealed class PlayerLookVerticalNode : SimpleActionNode
{
    public ValueInput<float> Angle = new();

    protected override void DoAction(PulseContext c) => c.GetPlayer().LookVertical(Angle.Read(c));
}

[Node("Look Horizontal", "VRChat/Player/Actions")]
public sealed class PlayerLookHorizontalNode : SimpleActionNode
{
    public ValueInput<float> Angle = new();

    protected override void DoAction(PulseContext c) => c.GetPlayer().LookHorizontal(Angle.Read(c));
}

[Node("Move Vertical", "VRChat/Player/Actions")]
public sealed class PlayerMoveVerticalNode : SimpleActionNode
{
    public ValueInput<float> Amount = new();

    protected override void DoAction(PulseContext c) => c.GetPlayer().MoveVertical(float.Clamp(Amount.Read(c), -1f, 1f));
}

[Node("Move Horizontal", "VRChat/Player/Actions")]
public sealed class PlayerMoveHorizontalNode : SimpleActionNode
{
    public ValueInput<float> Amount = new();

    protected override void DoAction(PulseContext c) => c.GetPlayer().MoveHorizontal(float.Clamp(Amount.Read(c), -1f, 1f));
}

[Node("Set Run", "VRChat/Player/Actions")]
public sealed class PlayerSetRunNode : SimpleActionNode
{
    public ValueInput<bool> Run = new();

    protected override void DoAction(PulseContext c)
    {
        if (Run.Read(c))
            c.GetPlayer().Run();
        else
            c.GetPlayer().StopRun();
    }
}

[Node("Change Avatar", "VRChat/Player/Actions")]
public sealed class PlayerChangeAvatarNode : SimpleActionNode
{
    public ValueInput<string> AvatarId = new("Avatar Id");

    protected override void DoAction(PulseContext c)
    {
        var avatarId = AvatarId.Read(c);

        if (!string.IsNullOrEmpty(avatarId))
        {
            AppManager.GetInstance().VRChatOscClient.Send($"{VRChatOSCConstants.ADDRESS_AVATAR_CHANGE}", avatarId);
        }
    }
}

[Node("Grab", "VRChat/Player/Actions")]
public sealed class PlayerGrabNode : SimpleActionNode
{
    public ValueInput<Chirality> Chirality = new();

    protected override void DoAction(PulseContext c)
    {
        if (Chirality.Read(c) == Utils.Chirality.Left)
            c.GetPlayer().GrabLeft();
        else
            c.GetPlayer().GrabRight();
    }
}

[Node("Drop", "VRChat/Player/Actions")]
public sealed class PlayerDropNode : SimpleActionNode
{
    public ValueInput<Chirality> Chirality = new();

    protected override void DoAction(PulseContext c)
    {
        if (Chirality.Read(c) == Utils.Chirality.Left)
            c.GetPlayer().DropLeft();
        else
            c.GetPlayer().DropRight();
    }
}

[Node("Use", "VRChat/Player/Actions")]
public sealed class PlayerUseNode : SimpleActionNode
{
    public ValueInput<Chirality> Chirality = new();

    protected override void DoAction(PulseContext c)
    {
        if (Chirality.Read(c) == Utils.Chirality.Left)
            c.GetPlayer().UseLeft();
        else
            c.GetPlayer().UseRight();
    }
}

[Node("Enter Safe Mode", "VRChat/Player/Actions")]
public sealed class PlayerEnterSafeModeNode : SimpleActionNode
{
    protected override void DoAction(PulseContext c) => c.GetPlayer().EnableSafeMode();
}

[Node("Toggle Quick Menu", "VRChat/Player/Actions")]
public sealed class PlayerToggleQuickMenuNode : SimpleActionNode
{
    public ValueInput<Chirality> Chirality = new();

    protected override void DoAction(PulseContext c)
    {
        if (Chirality.Read(c) == Utils.Chirality.Left)
            c.GetPlayer().ToggleLeftQuickMenu();
        else
            c.GetPlayer().ToggleRightQuickMenu();
    }
}