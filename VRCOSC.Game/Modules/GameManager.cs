// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Platform;
using Valve.VR;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.OpenVR;
using VRCOSC.Game.OpenVR.Metadata;
using VRCOSC.OSC.VRChat;

namespace VRCOSC.Game.Modules;

public partial class GameManager : CompositeComponent
{
    private const double openvr_check_interval = 1000;
    private const double vrchat_process_check_interval = 5000;
    private const int startstop_delay = 250;

    [Resolved]
    private VRCOSCConfigManager configManager { get; set; } = null!;

    [Resolved]
    private NotificationContainer notifications { get; set; } = null!;

    [Resolved(name: "EditingModule")]
    private Bindable<Module?> editingModule { get; set; } = null!;

    [Resolved(name: "InfoModule")]
    private Bindable<Module?> infoModule { get; set; } = null!;

    private Bindable<bool> autoStartStop = null!;
    private bool hasAutoStarted;

    public readonly VRChatOscClient OscClient = new();
    public readonly ModuleManager ModuleManager = new();
    public readonly Bindable<GameManagerState> State = new(GameManagerState.Stopped);
    public Player Player = null!;
    public OVRClient OVRClient = null!;
    public ChatBoxInterface ChatBoxInterface = null!;

    [BackgroundDependencyLoader]
    private void load(Storage storage)
    {
        autoStartStop = configManager.GetBindable<bool>(VRCOSCSetting.AutoStartStop);

        Player = new Player(OscClient);

        OVRClient = new OVRClient(new OVRMetadata
        {
            ApplicationType = EVRApplicationType.VRApplication_Background,
            ApplicationManifest = storage.GetFullPath(@"openvr/app.vrmanifest"),
            ActionManifest = storage.GetFullPath(@"openvr/action_manifest.json")
        });
        OVRHelper.OnError += m => Logger.Log($"[OpenVR] {m}");

        ChatBoxInterface = new ChatBoxInterface(OscClient, configManager.GetBindable<int>(VRCOSCSetting.ChatBoxTimeSpan));

        AddInternal(ModuleManager);
    }

    protected override void Update()
    {
        OVRClient.Update();
    }

    protected override void UpdateAfterChildren()
    {
        ChatBoxInterface.Update();
    }

    protected override void LoadComplete()
    {
        Scheduler.AddDelayed(checkForOpenVR, openvr_check_interval, true);
        Scheduler.AddDelayed(checkForVRChat, vrchat_process_check_interval, true);

        State.BindValueChanged(e => Logger.Log($"{nameof(GameManager)} state changed to {e.NewValue}"));

        // We reset hasAutoStarted here so that turning auto start off and on again will cause it to work normally
        autoStartStop.BindValueChanged(e =>
        {
            if (!e.NewValue) hasAutoStarted = false;
        });
    }

    public void Start() => Schedule(() => _ = startAsync());

    private async Task startAsync()
    {
        if (State.Value is GameManagerState.Starting or GameManagerState.Started)
            throw new InvalidOperationException($"Cannot start {nameof(GameManager)} when state is {State.Value}");

        if (editingModule.Value is not null || infoModule.Value is not null)
        {
            hasAutoStarted = false;
            return;
        }

        if (!initialiseOscClient())
        {
            hasAutoStarted = false;
            return;
        }

        State.Value = GameManagerState.Starting;

        await Task.Delay(startstop_delay);

        OscClient.OnParameterReceived += onParameterReceived;
        Player.Initialise();
        ChatBoxInterface.Initialise();
        sendControlValues();
        ModuleManager.Start();

        State.Value = GameManagerState.Started;
    }

    public void Stop() => Schedule(() => _ = stopAsync());

    private async Task stopAsync()
    {
        if (State.Value is GameManagerState.Stopping or GameManagerState.Stopped)
            throw new InvalidOperationException($"Cannot stop {nameof(GameManager)} when state is {State.Value}");

        State.Value = GameManagerState.Stopping;

        await OscClient.DisableReceive();
        ModuleManager.Stop();
        ChatBoxInterface.Shutdown();
        Player.ResetAll();
        OscClient.OnParameterReceived -= onParameterReceived;
        OscClient.DisableSend();

        await Task.Delay(startstop_delay);

        State.Value = GameManagerState.Stopped;
    }

    private bool initialiseOscClient()
    {
        try
        {
            var ipAddress = configManager.Get<string>(VRCOSCSetting.IPAddress);
            var sendPort = configManager.Get<int>(VRCOSCSetting.SendPort);
            var receivePort = configManager.Get<int>(VRCOSCSetting.ReceivePort);

            OscClient.Initialise(ipAddress, sendPort, receivePort);
            OscClient.Enable();
            return true;
        }
        catch (SocketException)
        {
            notifications.Notify(new InvalidOSCAttributeNotification("IP address"));
            return false;
        }
        catch (FormatException)
        {
            notifications.Notify(new InvalidOSCAttributeNotification("IP address"));
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            notifications.Notify(new InvalidOSCAttributeNotification("port"));
            return false;
        }
    }

    private void checkForOpenVR() => Task.Run(() =>
    {
        static bool isOpenVROpen() => Process.GetProcessesByName(@"vrmonitor").Any();
        if (isOpenVROpen()) OVRClient.Init();
    });

    private void checkForVRChat()
    {
        if (!configManager.Get<bool>(VRCOSCSetting.AutoStartStop)) return;

        static bool isVRChatOpen() => Process.GetProcessesByName(@"vrchat").Any();

        // hasAutoStarted is checked here to ensure that modules aren't started immediately
        // after a user has manually stopped the modules
        if (isVRChatOpen() && State.Value == GameManagerState.Stopped && !hasAutoStarted)
        {
            Start();
            hasAutoStarted = true;
        }

        if (!isVRChatOpen() && State.Value == GameManagerState.Started)
        {
            Stop();
            hasAutoStarted = false;
        }
    }

    private void sendControlValues()
    {
        OscClient.SendValue(@$"{VRChatOscConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}/VRCOSC/Controls/ChatBox", ChatBoxInterface.SendEnabled);
    }

    private void onParameterReceived(VRChatOscData data)
    {
        if (data.IsAvatarChangeEvent)
        {
            sendControlValues();
            return;
        }

        if (!data.IsAvatarParameter) return;

        Player.Update(data.ParameterName, data.Values[0]);

        switch (data.ParameterName)
        {
            case @"VRCOSC/Controls/ChatBox":
                ChatBoxInterface.SendEnabled = (bool)data.Values[0];
                break;
        }
    }
}

public enum GameManagerState
{
    Starting,
    Started,
    Stopping,
    Stopped
}
