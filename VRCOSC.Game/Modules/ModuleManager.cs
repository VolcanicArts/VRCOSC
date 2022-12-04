// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Platform;
using VRCOSC.Game.Config;
using VRCOSC.Game.Modules.Modules.Clock;
using VRCOSC.Game.Modules.Modules.Discord;
using VRCOSC.Game.Modules.Modules.HardwareStats;
using VRCOSC.Game.Modules.Modules.Heartrate.HypeRate;
using VRCOSC.Game.Modules.Modules.Heartrate.Pulsoid;
using VRCOSC.Game.Modules.Modules.Media;
using VRCOSC.Game.Modules.Modules.OpenVR;
using VRCOSC.Game.Modules.Modules.Random;
//using VRCOSC.Game.Modules.Modules.SpeechToText;
using VRCOSC.Game.Modules.Util;
using VRCOSC.Game.Util;
using VRCOSC.OSC;

namespace VRCOSC.Game.Modules;

public sealed partial class ModuleManager : Component
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
        typeof(OpenVRBatteryModule),
        //typeof(IndexControllerModule),
        typeof(MediaModule),
        typeof(DiscordModule),
        //typeof(SpeechToTextModule),
    };

    private const int vr_chat_process_check_interval_milliseconds = 5000;

    public readonly List<Module> Modules = new();
    public readonly OscClient OscClient = new();
    public readonly Bindable<ManagerState> State = new(ManagerState.Stopped);

    private bool hasAutoStarted;
    private Bindable<bool> autoStartStop = null!;
    private readonly TerminalLogger terminal = new(nameof(ModuleManager));
    private CancellationTokenSource startCancellationTokenSource = null!;
    private ChatBox chatBox = null!;

    [Resolved]
    private VRCOSCConfigManager ConfigManager { get; set; } = null!;

    [Resolved]
    private BindableBool ModuleRun { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load(GameHost host, Storage storage, OpenVrInterface openVrInterface)
    {
        chatBox = new ChatBox(OscClient);
        autoStartStop = ConfigManager.GetBindable<bool>(VRCOSCSetting.AutoStartStop);

        var moduleStorage = storage.GetStorageForDirectory("modules");
        module_types.ForEach(type =>
        {
            var module = (Module)Activator.CreateInstance(type)!;
            module.Initialise(host, moduleStorage, OscClient, chatBox, openVrInterface);
            Modules.Add(module);
        });
    }

    protected override void LoadComplete()
    {
        Scheduler.AddDelayed(checkForVrChat, vr_chat_process_check_interval_milliseconds, true);

        autoStartStop.BindValueChanged(e =>
        {
            // We reset hasAutoStarted here so that turning auto start off and on again will cause it to work normally
            if (!e.NewValue) hasAutoStarted = false;
        });

        ModuleRun.BindValueChanged(e =>
        {
            if (e.NewValue)
                startProxy();
            else
                Task.Run(stop);
        });

        State.BindValueChanged(e => Logger.Log($"ModuleManager now {e.NewValue}"));
    }

    private void startProxy()
    {
        try
        {
            startCancellationTokenSource = new CancellationTokenSource();
            Task.Run(start, startCancellationTokenSource.Token);
        }
        catch (TaskCanceledException) { }
    }

    private void checkForVrChat()
    {
        if (!ConfigManager.Get<bool>(VRCOSCSetting.AutoStartStop)) return;

        static bool isVrChatRunning() => Process.GetProcessesByName("vrchat").Any();

        // hasAutoStarted is checked here to ensure that modules aren't started immediately
        // after a user has manually stopped the modules
        if (isVrChatRunning() && !ModuleRun.Value && !hasAutoStarted)
        {
            ModuleRun.Value = true;
            hasAutoStarted = true;
        }

        if (!isVrChatRunning() && ModuleRun.Value)
        {
            ModuleRun.Value = false;
            hasAutoStarted = false;
        }
    }

    private async Task start()
    {
        State.Value = ManagerState.Starting;

        await Task.Delay(250, startCancellationTokenSource.Token);

        enableOsc();

        if (Modules.All(module => !module.Enabled.Value))
        {
            terminal.Log("You have no modules selected!\nSelect some modules to begin using VRCOSC");
            return;
        }

        chatBox.Init();

        foreach (var module in Modules)
        {
            _ = module.start(startCancellationTokenSource.Token);
        }

        State.Value = ManagerState.Started;
    }

    private void enableOsc()
    {
        var ipAddress = ConfigManager.Get<string>(VRCOSCSetting.IPAddress);
        var sendPort = ConfigManager.Get<int>(VRCOSCSetting.SendPort);
        var receivePort = ConfigManager.Get<int>(VRCOSCSetting.ReceivePort);

        try
        {
            OscClient.Initialise(ipAddress, sendPort, receivePort);
            OscClient.Enable();
        }
        catch (SocketException)
        {
            terminal.Log("An invalid OSC IP address has been provided. Please check your settings");
        }
        catch (ArgumentOutOfRangeException)
        {
            terminal.Log("An invalid OSC port has been provided. Please check your settings");
        }
    }

    private async Task stop()
    {
        State.Value = ManagerState.Stopping;

        startCancellationTokenSource?.Cancel();

        await OscClient.DisableReceive();

        foreach (var module in Modules)
        {
            await module.stop();
        }

        await chatBox.Shutdown();

        OscClient.DisableSend();

        State.Value = ManagerState.Stopped;
    }
}

public enum ManagerState
{
    Starting,
    Started,
    Stopping,
    Stopped
}
