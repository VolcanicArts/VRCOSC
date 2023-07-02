// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Linq;
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Avatar;
using VRCOSC.Game.OSC.VRChat;
using WinRT;

namespace VRCOSC.Game.App;

public class VRChat
{
    private const string avatar_id_format = "avtr_XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX";

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

    public void HandleAvatarChange(VRChatOscData data)
    {
        var avatarIdRaw = data.ParameterValue.As<string>();

        if (!avatarIdRaw.StartsWith("local"))
        {
            // truncation required as custom OSC library seems to be receiving 3 extra bytes?
            var avatarId = avatarIdRaw[..avatar_id_format.Length];
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
