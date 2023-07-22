// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics;
using System.Linq;
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Avatar;
using VRCOSC.Game.OSC.VRChat;
using WinRT;

namespace VRCOSC.Game.App;

public class VRChat
{
    public bool IsClientOpen;

    public Player Player = null!;
    public AvatarConfig? AvatarConfig;

    public void Initialise(VRChatOscClient oscClient)
    {
        Player = new Player(oscClient);
    }

    public void Teardown()
    {
        AvatarConfig = null;
        Player.ResetAll();
    }

    public void HandleAvatarChange(VRChatOscMessage message)
    {
        var avatarId = message.ParameterValue.As<string>();

        if (!avatarId.StartsWith("local"))
        {
            AvatarConfig = AvatarConfigLoader.LoadConfigFor(avatarId);
        }
    }

    public bool HasOpenStateChanged()
    {
        var clientNewOpenState = Process.GetProcessesByName(@"vrchat").Any();
        if (clientNewOpenState == IsClientOpen) return false;

        IsClientOpen = clientNewOpenState;
        return true;
    }
}
