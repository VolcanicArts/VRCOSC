// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Net;
using osu.Framework.Platform;
using osu.Framework.Timing;
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Remote;
using VRCOSC.Game.OSC.VRChat;

namespace VRCOSC.Game;

public class AppManager
{
    public ModuleManager ModuleManager { get; private set; } = null!;
    public RemoteModuleSourceManager RemoteModuleSourceManager { get; private set; } = null!;
    public VRChatOscClient VRChatOscClient { get; private set; } = null!;

    public void Initialise(Storage storage, IClock clock)
    {
        ModuleManager = new ModuleManager(storage, clock);
        RemoteModuleSourceManager = new RemoteModuleSourceManager(storage);
        VRChatOscClient = new VRChatOscClient();

        VRChatOscClient.Initialise(new IPEndPoint(IPAddress.Loopback, 9000), new IPEndPoint(IPAddress.Loopback, 9001));
        VRChatOscClient.EnableSend();
        VRChatOscClient.EnableReceive();
    }

    public void FrameworkUpdate()
    {
        ModuleManager.FrameworkUpdate();
    }
}
