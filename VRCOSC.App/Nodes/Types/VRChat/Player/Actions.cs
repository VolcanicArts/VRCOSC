// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.VRChat.Player;

[Node("Mute Set", "VRChat/Player/Actions")]
public sealed class PlayerMuteSetNode : ActionNode
{
    public ValueInput<bool> Muted = new();

    protected override void DoAction(IPulseContext c)
    {
        if (Muted.Read(c))
            AppManager.GetInstance().VRChatClient.Player.Mute();
        else
            AppManager.GetInstance().VRChatClient.Player.UnMute();
    }
}

[Node("Mute Toggle", "VRChat/Player/Actions")]
public sealed class PlayerMuteToggleNode : ActionNode
{
    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.Player.ToggleVoice();
}

[Node("Push To Talk", "VRChat/Player/Actions")]
public sealed class PlayerPushToTalkNode : ActionNode
{
    public ValueInput<bool> Active = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.Player.PushToTalk(Active.Read(c));
}

[Node("Jump", "VRChat/Player/Actions")]
public sealed class PlayerJumpNode : ActionNode
{
    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.Player.Jump();
}

[Node("Look Vertical", "VRChat/Player/Actions")]
public sealed class PlayerLookVerticalNode : ActionNode
{
    public ValueInput<float> Angle = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.Player.LookVertical(Angle.Read(c));
}

[Node("Look Horizontal", "VRChat/Player/Actions")]
public sealed class PlayerLookHorizontalNode : ActionNode
{
    public ValueInput<float> Angle = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.Player.LookHorizontal(Angle.Read(c));
}

[Node("Move Vertical", "VRChat/Player/Actions")]
public sealed class PlayerMoveVerticalNode : ActionNode
{
    public ValueInput<float> Amount = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.Player.MoveVertical(float.Clamp(Amount.Read(c), -1f, 1f));
}

[Node("Move Horizontal", "VRChat/Player/Actions")]
public sealed class PlayerMoveHorizontalNode : ActionNode
{
    public ValueInput<float> Amount = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.Player.MoveHorizontal(float.Clamp(Amount.Read(c), -1f, 1f));
}

[Node("Set Run", "VRChat/Player/Actions")]
public sealed class PlayerSetRunNode : ActionNode
{
    public ValueInput<bool> Run = new();

    protected override void DoAction(IPulseContext c)
    {
        if (Run.Read(c))
            AppManager.GetInstance().VRChatClient.Player.Run();
        else
            AppManager.GetInstance().VRChatClient.Player.StopRun();
    }
}

[Node("Change Avatar", "VRChat/Player/Actions")]
public sealed class PlayerChangeAvatarNode : ActionNode
{
    public ValueInput<string> AvatarId = new("Avatar Id");

    protected override void DoAction(IPulseContext c)
    {
        var avatarId = AvatarId.Read(c);

        if (!string.IsNullOrEmpty(avatarId))
        {
            AppManager.GetInstance().VRChatOscClient.Send($"{VRChatOSCConstants.ADDRESS_AVATAR_CHANGE}", avatarId);
        }
    }
}

[Node("Grab", "VRChat/Player/Actions")]
public sealed class PlayerGrabNode : ActionNode
{
    public ValueInput<Chirality> Chirality = new();

    protected override void DoAction(IPulseContext c)
    {
        if (Chirality.Read(c) == Utils.Chirality.Left)
            AppManager.GetInstance().VRChatClient.Player.GrabLeft();
        else
            AppManager.GetInstance().VRChatClient.Player.GrabRight();
    }
}

[Node("Drop", "VRChat/Player/Actions")]
public sealed class PlayerDropNode : ActionNode
{
    public ValueInput<Chirality> Chirality = new();

    protected override void DoAction(IPulseContext c)
    {
        if (Chirality.Read(c) == Utils.Chirality.Left)
            AppManager.GetInstance().VRChatClient.Player.DropLeft();
        else
            AppManager.GetInstance().VRChatClient.Player.DropRight();
    }
}

[Node("Use", "VRChat/Player/Actions")]
public sealed class PlayerUseNode : ActionNode
{
    public ValueInput<Chirality> Chirality = new();

    protected override void DoAction(IPulseContext c)
    {
        if (Chirality.Read(c) == Utils.Chirality.Left)
            AppManager.GetInstance().VRChatClient.Player.UseLeft();
        else
            AppManager.GetInstance().VRChatClient.Player.UseRight();
    }
}

[Node("Enter Safe Mode", "VRChat/Player/Actions")]
public sealed class PlayerEnterSafeModeNode : ActionNode
{
    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.Player.EnableSafeMode();
}

[Node("Toggle Quick Menu", "VRChat/Player/Actions")]
public sealed class PlayerToggleQuickMenuNode : ActionNode
{
    public ValueInput<Chirality> Chirality = new();

    protected override void DoAction(IPulseContext c)
    {
        if (Chirality.Read(c) == Utils.Chirality.Left)
            AppManager.GetInstance().VRChatClient.Player.ToggleLeftQuickMenu();
        else
            AppManager.GetInstance().VRChatClient.Player.ToggleRightQuickMenu();
    }
}