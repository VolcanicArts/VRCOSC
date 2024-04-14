// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics;
using System.Linq;
using VRCOSC.App.OSC.VRChat;

namespace VRCOSC.App.SDK.VRChat;

public class VRChatClient
{
    public readonly Player Player;
    public AvatarConfig? AvatarConfig;

    private bool lastKnownOpenState;

    public VRChatClient(VRChatOscClient oscClient)
    {
        Player = new Player(oscClient);
    }

    public void Teardown()
    {
        Player.ResetAll();
        AvatarConfig = null;
    }

    public void HandleAvatarChange(VRChatOscMessage message)
    {
        var avatarId = (string)message.ParameterValue;

        if (!avatarId.StartsWith("local"))
        {
            AvatarConfig = AvatarConfigLoader.LoadConfigFor(avatarId);
        }
    }

    public bool HasOpenStateChanged(out bool openState)
    {
        var newOpenState = Process.GetProcessesByName("vrchat").Any();

        if (newOpenState == lastKnownOpenState)
        {
            openState = lastKnownOpenState;
            return false;
        }

        openState = lastKnownOpenState = newOpenState;
        return true;
    }
}
