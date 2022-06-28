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
using osu.Framework.Graphics;
using osu.Framework.Platform;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.Containers.Screens;
using VRCOSC.Game.Graphics.Updater;
using VRCOSC.Game.Util;

namespace VRCOSC.Game.Modules;

public sealed class ModuleManager : Drawable
{
    private bool running;
    private bool autoStarted;
    private Bindable<bool> autoStartStop;

    public readonly OscClient OSCClient = new();
    private readonly TerminalLogger terminal = new(nameof(ModuleManager));

    private List<Module> modules = null!;

    [Resolved]
    private VRCOSCConfigManager configManager { get; set; }

    [Resolved]
    private ScreenManager screenManager { get; set; }

    [Resolved]
    private VRCOSCUpdateManager updateManager { get; set; }

    [BackgroundDependencyLoader]
    private void load(Storage storage)
    {
        modules = ReflectiveEnumerator.GetEnumerableOfType<Module>();

        var moduleStorage = storage.GetStorageForDirectory("modules");

        foreach (ModuleType type in Enum.GetValues(typeof(ModuleType)))
        {
            foreach (var module in modules.Where(module => module.ModuleType.Equals(type)))
            {
                module.Initialise(moduleStorage, OSCClient);
                module.CreateAttributes();
                module.PerformLoad();
            }
        }

        Scheduler.AddDelayed(checkForVrChat, 5000, true);

        autoStartStop = configManager.GetBindable<bool>(VRCOSCSetting.AutoStartStop);
        autoStartStop.ValueChanged += e =>
        {
            if (!e.NewValue) autoStarted = false;
        };
    }

    private void checkForVrChat()
    {
        if (updateManager.Updating) return;

        var vrChat = Process.GetProcessesByName("vrchat");

        if (vrChat.Length != 0 && autoStartStop.Value && !running && !autoStarted)
        {
            screenManager.ShowTerminal();
            autoStarted = true;
        }

        if (vrChat.Length == 0 && autoStartStop.Value && running)
        {
            screenManager.HideTerminal();
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
            OSCClient.Initialise(ipAddress, sendPort, receivePort);
            OSCClient.Enable();
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

        modules.ForEach(module => module.start());
        running = true;
    }

    public async Task Stop()
    {
        if (!running) return;

        running = false;

        foreach (var module in modules)
        {
            await module.stop();
        }

        await OSCClient.Disable();
    }

    public List<Module> GroupBy(ModuleType moduleType)
    {
        return modules.Where(module => module.ModuleType == moduleType).ToList();
    }

    protected override void Dispose(bool isDisposing)
    {
        if (running) Stop().Wait();
        base.Dispose(isDisposing);
    }
}
