// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VRCOSC.App.OSC.VRChat;

namespace VRCOSC.App.SDK.VRChat;

public class VRChatClient
{
    public readonly Player Player;
    public readonly Instance Instance;

    public bool LastKnownOpenState { get; private set; }

    internal VRChatClient(VRChatOSCClient oscClient)
    {
        Player = new Player(oscClient);
        Instance = new Instance();
    }

    public void Teardown()
    {
        Player.ResetAll();
    }

    public async Task HandleAvatarChange()
    {
        await Player.RetrieveAll();
    }

    public bool HasOpenStateChanged(out bool openState)
    {
        var newOpenState = Process.GetProcessesByName("vrchat").Any();

        if (newOpenState == LastKnownOpenState)
        {
            openState = LastKnownOpenState;
            return false;
        }

        openState = LastKnownOpenState = newOpenState;
        return true;
    }
}