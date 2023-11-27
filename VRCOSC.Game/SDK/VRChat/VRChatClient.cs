// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics;
using System.Linq;
using VRCOSC.Game.OSC.VRChat;

namespace VRCOSC.Game.SDK.VRChat;

public class VRChatClient
{
    public readonly Player Player;
    public bool ClientOpen { get; private set; }

    public VRChatClient(VRChatOscClient oscClient)
    {
        Player = new Player(oscClient);
    }

    public void Teardown()
    {
        Player.ResetAll();
    }

    public bool IsClientOpen() => ClientOpen = Process.GetProcessesByName("vrchat").Any();

    public bool HasOpenStateChanged()
    {
        var clientNewOpenState = IsClientOpen();
        if (clientNewOpenState == ClientOpen) return false;

        ClientOpen = clientNewOpenState;
        return true;
    }
}
