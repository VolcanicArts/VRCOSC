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
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Platform;
using Valve.VR;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Modules.Avatar;
using VRCOSC.Game.OpenVR;
using VRCOSC.Game.OpenVR.Metadata;
using VRCOSC.Game.OSC;
using VRCOSC.Game.OSC.Client;
using VRCOSC.Game.OSC.VRChat;

namespace VRCOSC.Game.Modules;

public partial class GameManager : CompositeComponent
{
    private const double openvr_check_interval = 1000;
    private const double vrchat_process_check_interval = 5000;
    private const int startstop_delay = 250;

    [Resolved]
    private VRCOSCConfigManager configManager { get; set; } = null!;

    [Resolved]
    private RouterManager routerManager { get; set; } = null!;

    [Resolved]
    private NotificationContainer notifications { get; set; } = null!;

    [Resolved(name: "EditingModule")]
    private Bindable<Module?> editingModule { get; set; } = null!;

    [Resolved(name: "InfoModule")]
    private Bindable<Module?> infoModule { get; set; } = null!;

    private Bindable<bool> autoStartStop = null!;
    private bool hasAutoStarted;
    private readonly List<VRChatOscData> oscDataCache = new();
    private readonly object oscDataCacheLock = new();

    public readonly VRChatOscClient VRChatOscClient = new();
    public readonly ModuleManager ModuleManager = new();
    public readonly Bindable<GameManagerState> State = new(GameManagerState.Stopped);
    public OSCRouter OSCRouter = null!;
    public Player Player = null!;
    public OVRClient OVRClient = null!;
    public ChatBoxInterface ChatBoxInterface = null!;
    public AvatarConfig? AvatarConfig;

    [BackgroundDependencyLoader]
    private void load(Storage storage)
    {
        autoStartStop = configManager.GetBindable<bool>(VRCOSCSetting.AutoStartStop);

        OSCRouter = new OSCRouter(VRChatOscClient);
        Player = new Player(VRChatOscClient);

        OVRClient = new OVRClient(new OVRMetadata
        {
            ApplicationType = EVRApplicationType.VRApplication_Background,
            ApplicationManifest = storage.GetFullPath(@"openvr/app.vrmanifest"),
            ActionManifest = storage.GetFullPath(@"openvr/action_manifest.json")
        });
        OVRHelper.OnError += m => Logger.Log($"[OpenVR] {m}");

        ChatBoxInterface = new ChatBoxInterface(VRChatOscClient, configManager.GetBindable<int>(VRCOSCSetting.ChatBoxTimeSpan));

        AddInternal(ModuleManager);
    }

    protected override void Update()
    {
        if (State.Value != GameManagerState.Started) return;

        OVRClient.Update();

        handleOscDataCache();
    }

    protected override void UpdateAfterChildren()
    {
        if (State.Value != GameManagerState.Started) return;

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

        VRChatOscClient.OnParameterReceived += data =>
        {
            lock (oscDataCacheLock)
            {
                oscDataCache.Add(data);
            }
        };
    }

    private const string avatar_id_format = "avtr_XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX";

    private void handleOscDataCache()
    {
        lock (oscDataCacheLock)
        {
            oscDataCache.ForEach(data =>
            {
                if (data.IsAvatarChangeEvent)
                {
                    var avatarId = ((string)data.ParameterValue)[..avatar_id_format.Length];
                    AvatarConfig = AvatarConfigLoader.LoadConfigFor(avatarId);

                    sendControlValues();
                }
                else
                {
                    Player.Update(data.ParameterName, data.Values[0]);

                    switch (data.ParameterName)
                    {
                        case @"VRCOSC/Controls/ChatBox":
                            ChatBoxInterface.SendEnabled = (bool)data.Values[0];
                            break;
                    }
                }

                ModuleManager.OnParameterReceived(data);
            });
            oscDataCache.Clear();
        }
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

        lock (oscDataCacheLock)
        {
            oscDataCache.Clear();
        }

        if (!initialiseOscClient())
        {
            hasAutoStarted = false;
            return;
        }

        State.Value = GameManagerState.Starting;

        await Task.Delay(startstop_delay);

        VRChatOscClient.Enable(OscClientFlag.Send);
        Player.Initialise();
        ChatBoxInterface.Initialise();
        sendControlValues();
        ModuleManager.Start();
        VRChatOscClient.Enable(OscClientFlag.Receive);

        OSCRouter.Initialise(routerManager.Store);
        OSCRouter.Enable();

        State.Value = GameManagerState.Started;
    }

    public void Stop() => Schedule(() => _ = stopAsync());

    private async Task stopAsync()
    {
        if (State.Value is GameManagerState.Stopping or GameManagerState.Stopped)
            throw new InvalidOperationException($"Cannot stop {nameof(GameManager)} when state is {State.Value}");

        State.Value = GameManagerState.Stopping;

        await VRChatOscClient.Disable(OscClientFlag.Receive);

        lock (oscDataCacheLock)
        {
            oscDataCache.Clear();
        }

        await OSCRouter.Disable();
        ModuleManager.Stop();
        ChatBoxInterface.Shutdown();
        Player.ResetAll();
        await VRChatOscClient.Disable(OscClientFlag.Send);

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

            VRChatOscClient.Initialise(ipAddress, sendPort, receivePort);
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

        if (isOpenVROpen())
        {
            OVRClient.Init();
            OVRClient.SetAutoLaunch(configManager.Get<bool>(VRCOSCSetting.AutoStartOpenVR));
        }
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
        VRChatOscClient.SendValue(@$"{VRChatOscConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}/VRCOSC/Controls/ChatBox", ChatBoxInterface.SendEnabled);
    }
}

public enum GameManagerState
{
    Starting,
    Started,
    Stopping,
    Stopped
}
