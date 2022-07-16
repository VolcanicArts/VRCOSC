// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using VRCOSC.Game.Config;
using VRCOSC.Game.Util;

namespace VRCOSC.Game.Modules;

public sealed class ModuleManager : Drawable
{
    private bool running;
    private bool autoStarted;
    private Bindable<bool> autoStartStop = null!;
    private readonly TerminalLogger terminal = new(nameof(ModuleManager));
    public readonly IReadOnlyList<Module> Modules = ReflectiveEnumerator.GetEnumerableOfType<Module>()!;
    public readonly OscClient OscClient = new();

    [Resolved]
    private VRCOSCConfigManager configManager { get; set; } = null!;

    public List<Module> GroupBy(ModuleType moduleType) => Modules.Where(module => module.ModuleType == moduleType).ToList();

    [BackgroundDependencyLoader]
    private void load(Storage storage)
    {
        var moduleStorage = storage.GetStorageForDirectory("modules");
        Modules.ForEach(module => module.Initialise(moduleStorage, OscClient));

        autoStartStop = configManager.GetBindable<bool>(VRCOSCSetting.AutoStartStop);
        autoStartStop.ValueChanged += e =>
        {
            if (!e.NewValue) autoStarted = false;
        };

        Scheduler.AddDelayed(checkForVrChat, 5000, true);
    }

    private void checkForVrChat()
    {
        //if (updateManager.Updating) return;

        var vrChat = Process.GetProcessesByName("vrchat");

        if (vrChat.Length != 0 && autoStartStop.Value && !running && !autoStarted)
        {
            //screenManager.ShowTerminal();
            autoStarted = true;
        }

        if (vrChat.Length == 0 && autoStartStop.Value && running)
        {
            //screenManager.HideTerminal();
            autoStarted = false;
        }
    }

    public void Start()
    {
        var ipAddress = configManager.Get<string>(VRCOSCSetting.IPAddress);
        var sendPort = configManager.Get<int>(VRCOSCSetting.SendPort);
        var receivePort = configManager.Get<int>(VRCOSCSetting.ReceivePort);

        try
        {
            OscClient.Initialise(ipAddress, sendPort, receivePort);
            OscClient.Enable();
        }
        catch (SocketException)
        {
            terminal.Log("Exception detected. An invalid OSC IP address has been provided");
            return;
        }
        catch (ArgumentOutOfRangeException)
        {
            terminal.Log("Exception detected. An invalid OSC port has been provided");
            return;
        }

        Modules.ForEach(module => module.start());
        running = true;
    }

    public async Task Stop()
    {
        if (!running) return;

        running = false;

        await OscClient.DisableReceive();

        foreach (var module in Modules)
        {
            await module.stop();
        }

        OscClient.DisableSend();
    }

    protected override void Dispose(bool isDisposing)
    {
        if (running) Stop().Wait();
        base.Dispose(isDisposing);
    }
}
