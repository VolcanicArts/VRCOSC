// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics;
using System.Linq;
using VRCOSC.App.OSC.VRChat;

namespace VRCOSC.App.SDK.VRChat;

public class VRChatClient
{
    public readonly Player Player;

    private bool lastKnownOpenState;

    internal VRChatClient(VRChatOSCClient oscClient)
    {
        Player = new Player(oscClient);
    }

    public void Teardown()
    {
        Player.ResetAll();
    }

    public async void HandleAvatarChange()
    {
        await Player.RetrieveAll();
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