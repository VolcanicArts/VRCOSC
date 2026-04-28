// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.VRChat;

public class VRChatClient
{
    private Process? clientProcess;

    public bool IsOpen { get; private set; }

    [MemberNotNullWhen(true, nameof(User))]
    public bool IsLoggedIn { get; private set; }

    [MemberNotNullWhen(true, nameof(Avatar))]
    public bool IsInAvatar { get; private set; }

    [MemberNotNullWhen(true, nameof(Instance))]
    public bool IsInInstance { get; private set; }

    public User? User { get; private set; }
    public Avatar? Avatar { get; private set; }
    public Instance? Instance { get; private set; }
    public Player Player { get; }
    public UserCamera UserCamera { get; }

    public double FPS => clientProcess is null ? 0d : ProcessFPS.GetProcessFPS(clientProcess);

    internal bool HasOpenStateChanged(out bool openState)
    {
        var processes = Process.GetProcessesByName("vrchat");

        if (processes.Length != 1)
        {
            openState = false;
            return false;
        }

        clientProcess = processes[0];
        var newOpenState = clientProcess is not null;

        if (newOpenState == IsOpen)
        {
            openState = IsOpen;
            return false;
        }

        openState = IsOpen = newOpenState;
        return true;
    }

    public VRChatClient(VRChatOSCClient client)
    {
        Player = new Player(client);
        UserCamera = new UserCamera(client);
    }

    public void UpdateUser(User? user)
    {
        User = IsOpen ? user : null;
        IsLoggedIn = user is not null && IsOpen;
    }

    public void UpdateInstance(Instance? instance)
    {
        Instance = IsOpen ? instance : null;
        IsInInstance = instance is not null && IsOpen;
    }

    public void UpdateAvatar(Avatar? avatar)
    {
        Avatar = IsOpen ? avatar : null;
        IsInAvatar = avatar is not null && IsOpen;
    }
}