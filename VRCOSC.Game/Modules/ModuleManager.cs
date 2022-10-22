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
using VRCOSC.Game.Modules.Modules.Clock;
using VRCOSC.Game.Modules.Modules.Deprecated.Spotify;
using VRCOSC.Game.Modules.Modules.Discord;
using VRCOSC.Game.Modules.Modules.HardwareStats;
using VRCOSC.Game.Modules.Modules.Heartrate.HypeRate;
using VRCOSC.Game.Modules.Modules.Heartrate.Pulsoid;
using VRCOSC.Game.Modules.Modules.Media;
using VRCOSC.Game.Modules.Modules.Random;
using VRCOSC.Game.Util;
using VRCOSC.OSC;

namespace VRCOSC.Game.Modules;

public sealed class ModuleManager : Component
{
    private static readonly IReadOnlyList<Type> module_types = new[]
    {
        typeof(HypeRateModule),
        typeof(PulsoidModule),
        typeof(ClockModule),
        typeof(RandomBoolModule),
        typeof(RandomIntModule),
        typeof(RandomFloatModule),
        typeof(HardwareStatsModule),
        typeof(MediaModule),
        typeof(SpotifyModule),
        typeof(DiscordModule)
        //typeof(SpeechToTextModule)
    };

    private bool autoStarted;
    private Bindable<bool> autoStartStop = null!;
    private readonly TerminalLogger terminal = new(nameof(ModuleManager));

    public readonly List<Module> Modules = new();
    public readonly OscClient OscClient = new();

    private ChatBox ChatBox;

    [Resolved]
    private VRCOSCConfigManager configManager { get; set; } = null!;

    [Resolved]
    private BindableBool modulesRunning { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load(GameHost host, Storage storage)
    {
        ChatBox = new ChatBox(OscClient);

        var moduleStorage = storage.GetStorageForDirectory("modules");
        module_types.ForEach(type =>
        {
            var module = (Module)Activator.CreateInstance(type)!;
            module.Initialise(host, moduleStorage, OscClient, ChatBox);
            Modules.Add(module);
        });

        autoStartStop = configManager.GetBindable<bool>(VRCOSCSetting.AutoStartStop);
    }

    protected override void LoadComplete()
    {
        autoStartStop.BindValueChanged(e =>
        {
            if (e.NewValue)
            {
                Scheduler.Add(checkForVrChat);
                Scheduler.AddDelayed(checkForVrChat, 5000, true);
            }
            else
            {
                Scheduler.CancelDelayedTasks();

                // this is reset when a user turns off autoStartStop in the case
                // that they've manually stopped the modules with autoStartStop enabled
                autoStarted = false;
            }
        }, true);

        modulesRunning.BindValueChanged(e =>
        {
            if (e.NewValue)
                start();
            else
                _ = stop();
        });

        if (modulesRunning.Value) start();
    }

    private static async Task focusVrc()
    {
        var process = Process.GetProcessesByName("vrchat").FirstOrDefault();
        if (process is null) return;

        if (process.MainWindowHandle == IntPtr.Zero)
        {
            ProcessExtensions.ShowMainWindow(process, ShowWindowEnum.Restore);
            await Task.Delay(5);
        }

        ProcessExtensions.ShowMainWindow(process, ShowWindowEnum.ShowDefault);
        await Task.Delay(5);
        ProcessExtensions.SetMainWindowForeground(process);
    }

    private static bool isVrChatRunning => Process.GetProcessesByName("vrchat").Any();

    private void checkForVrChat()
    {
        // autoStarted is checked here to ensure that modules aren't started immediately
        // after a user has manually stopped the modules
        if (isVrChatRunning && !modulesRunning.Value && !autoStarted)
        {
            modulesRunning.Value = true;
            autoStarted = true;
        }

        if (!isVrChatRunning && modulesRunning.Value)
        {
            modulesRunning.Value = false;
            autoStarted = false;
        }
    }

    private void start()
    {
        if (configManager.Get<bool>(VRCOSCSetting.AutoFocus)) _ = focusVrc();

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

        if (Modules.All(module => !module.Enabled.Value))
        {
            terminal.Log("You have no modules selected!");
            terminal.Log("Select some modules to begin using VRCOSC");
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
        if (modulesRunning.Value) _ = stop();
        base.Dispose(isDisposing);
    }
}
