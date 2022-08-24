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
using VRCOSC.Game.Modules.Modules.Calculator;
using VRCOSC.Game.Modules.Modules.Clock;
using VRCOSC.Game.Modules.Modules.Discord;
using VRCOSC.Game.Modules.Modules.HardwareStats;
using VRCOSC.Game.Modules.Modules.HypeRate;
using VRCOSC.Game.Modules.Modules.Media;
using VRCOSC.Game.Modules.Modules.Random;
using VRCOSC.Game.Modules.Modules.Spotify;
using VRCOSC.Game.Util;
using VRCOSC.OSC;

namespace VRCOSC.Game.Modules;

public sealed class ModuleManager : Component
{
    private static readonly IReadOnlyList<Type> module_types = new[]
    {
        typeof(RandomBoolModule),
        typeof(RandomIntModule),
        typeof(RandomFloatModule),
        typeof(ClockModule),
        typeof(HardwareStatsModule),
        typeof(SpotifyModule),
        typeof(DiscordModule),
        typeof(MediaModule),
        typeof(HypeRateModule),
        typeof(CalculatorModule)
    };

    private bool autoStarted;
    private Bindable<bool> autoStartStop = null!;
    private readonly TerminalLogger terminal = new(nameof(ModuleManager));

    public readonly List<Module> Modules = new();
    public readonly OscClient OscClient = new();

    [Resolved]
    private VRCOSCConfigManager configManager { get; set; } = null!;

    [Resolved]
    private VRCOSCGame game { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load(Storage storage)
    {
        var moduleStorage = storage.GetStorageForDirectory("modules");
        module_types.ForEach(type =>
        {
            var module = (Module)Activator.CreateInstance(type)!;
            module.Initialise(moduleStorage, OscClient);
            Modules.Add(module);
        });

        autoStartStop = configManager.GetBindable<bool>(VRCOSCSetting.AutoStartStop);
        autoStartStop.ValueChanged += e =>
        {
            if (!e.NewValue) autoStarted = false;
        };

        Scheduler.AddDelayed(checkForVrChat, 5000, true);

        game.ModulesRunning.BindValueChanged(e =>
        {
            if (e.NewValue)
            {
                start();
                if (configManager.Get<bool>(VRCOSCSetting.AutoFocus)) _ = focusVrc();
            }
            else
            {
                _ = stop();
            }
        });
    }

    private static async Task focusVrc()
    {
        var process = Process.GetProcessesByName("vrchat").FirstOrDefault();
        if (process is null) return;

        if (process.MainWindowHandle == IntPtr.Zero)
        {
            ProcessHelper.ShowMainWindow(process, ShowWindowEnum.Restore);
            await Task.Delay(5);
        }

        ProcessHelper.ShowMainWindow(process, ShowWindowEnum.ShowDefault);
        await Task.Delay(5);
        ProcessHelper.SetMainWindowForeground(process);
    }

    private void checkForVrChat()
    {
        if (game.UpdateManager.Updating) return;

        var vrChat = Process.GetProcessesByName("vrchat");

        if (vrChat.Length != 0 && autoStartStop.Value && !game.ModulesRunning.Value && !autoStarted)
        {
            game.ModulesRunning.Value = true;
            autoStarted = true;
        }

        if (vrChat.Length == 0 && autoStartStop.Value && game.ModulesRunning.Value)
        {
            game.ModulesRunning.Value = false;
            autoStarted = false;
        }
    }

    private void start()
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
    }

    private async Task stop()
    {
        await OscClient.DisableReceive();

        foreach (var module in Modules)
        {
            await module.stop();
        }

        OscClient.DisableSend();
    }

    protected override void Dispose(bool isDisposing)
    {
        if (game.ModulesRunning.Value) _ = stop();
        base.Dispose(isDisposing);
    }
}
