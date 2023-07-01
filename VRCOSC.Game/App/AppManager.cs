// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Development;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Threading;
using Valve.VR;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Managers;
using VRCOSC.Game.OpenVR;
using VRCOSC.Game.OpenVR.Metadata;
using VRCOSC.Game.OSC;
using VRCOSC.Game.OSC.Client;
using VRCOSC.Game.OSC.VRChat;
using WinRT;

namespace VRCOSC.Game.App;

public partial class AppManager : Component
{
    [Resolved]
    private GameHost host { get; set; } = null!;

    [Resolved]
    private Storage storage { get; set; } = null!;

    [Resolved]
    private NotificationContainer notifications { get; set; } = null!;

    [Resolved]
    private VRCOSCConfigManager configManager { get; set; } = null!;

    private static readonly TimeSpan openvr_check_interval = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan vrchat_check_internal = TimeSpan.FromSeconds(5);

    private readonly Queue<VRChatOscData> oscDataQueue = new();

    public readonly Bindable<AppManagerState> State = new(AppManagerState.Stopped);
    public readonly ModuleManager ModuleManager;
    public readonly ChatBoxManager ChatBoxManager;
    public readonly StartupManager StartupManager;
    public readonly RouterManager RouterManager;
    public readonly VRChatOscClient OSCClient;
    public readonly OSCRouter OSCRouter;
    public readonly OVRClient OVRClient;
    public readonly VRChat VRChat;

    private bool isRunning => State.Value == AppManagerState.Started;

    public AppManager()
    {
        ModuleManager = new ModuleManager();
        ChatBoxManager = new ChatBoxManager();
        StartupManager = new StartupManager();
        RouterManager = new RouterManager();
        OSCClient = new VRChatOscClient();
        OSCRouter = new OSCRouter(OSCClient);
        OVRClient = new OVRClient();
        VRChat = new VRChat();
    }

    #region Management

    [BackgroundDependencyLoader]
    private void load()
    {
        initialiseOVRClient();
        initialiseModuleManager();
        initialiseChatBoxManager();
        initialiseStartupManager();
        initialiseRouterManager();
        initialiseDelayedTasks();

        OSCClient.OnParameterReceived += data => Schedule(() => oscDataQueue.Enqueue(data));
        State.BindValueChanged(e => Logger.Log($"{nameof(AppManager)} state changed to {e.NewValue}"));
    }

    protected override void Update()
    {
        if (!isRunning) return;

        OVRClient.Update();

        processOSCDataQueue();

        ModuleManager.Update();
        ChatBoxManager.Update();
    }

    #endregion

    #region Initialisation

    private void initialiseOVRClient()
    {
        OVRClient.SetMetadata(new OVRMetadata
        {
            ApplicationType = EVRApplicationType.VRApplication_Background,
            ApplicationManifest = storage.GetFullPath(@"openvr/app.vrmanifest"),
            ActionManifest = storage.GetFullPath(@"openvr/action_manifest.json")
        });

        OVRHelper.OnError += m => Logger.Log($"[OpenVR] {m}");
    }

    private void initialiseModuleManager()
    {
        ModuleManager.Initialise(host, this, new Scheduler(() => ThreadSafety.IsUpdateThread, Clock), storage, notifications);
        ModuleManager.Load();
    }

    private void initialiseChatBoxManager()
    {
        ChatBoxManager.Load(storage, this, notifications);
    }

    private void initialiseStartupManager()
    {
        StartupManager.Initialise(storage, notifications);
        StartupManager.Load();
    }

    private void initialiseRouterManager()
    {
        RouterManager.Initialise(storage, notifications);
        RouterManager.Load();
    }

    private void initialiseDelayedTasks()
    {
        Scheduler.AddDelayed(checkForOpenVR, openvr_check_interval.TotalMilliseconds, true);
        Scheduler.AddDelayed(checkForVRChat, vrchat_check_internal.TotalMilliseconds, true);
    }

    #endregion

    #region OSC

    private void processOSCDataQueue()
    {
        while (oscDataQueue.TryDequeue(out var data))
        {
            if (data.IsAvatarChangeEvent)
            {
                VRChat.HandleAvatarChange(data);
                sendControlParameters();
            }

            if (data.IsAvatarParameter)
            {
                var wasPlayerUpdated = VRChat.Player.Update(data.ParameterName, data.ParameterValue);
                if (wasPlayerUpdated) ModuleManager.PlayerUpdate();

                if (data.ParameterName.StartsWith("VRCOSC/Controls")) processControlParameters(data);
            }

            ModuleManager.ParameterReceived(data);
        }
    }

    private void sendControlParameters()
    {
        OSCClient.SendValue($"{VRChatOscConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}/VRCOSC/Controls/ChatBox", ChatBoxManager.SendEnabled);
    }

    private void processControlParameters(VRChatOscData data)
    {
        switch (data.ParameterName)
        {
            case "VRCOSC/Controls/ChatBox":
                ChatBoxManager.SendEnabled = data.ParameterValue.As<bool>();
                break;
        }
    }

    #endregion

    #region Start

    public async void Start()
    {
        if (State.Value is AppManagerState.Starting or AppManagerState.Started) return;

        State.Value = AppManagerState.Starting;

        oscDataQueue.Clear();

        if (!initialiseOSCClient())
        {
            State.Value = AppManagerState.Stopped;
            return;
        }

        // Allow the UI to switch to the running screen if auto-starting so the terminal logs all start events
        await Task.Delay(50);

        enableOSCFlag(OscClientFlag.Send);
        VRChat.Initialise(OSCClient);
        ChatBoxManager.Initialise(OSCClient, configManager.GetBindable<int>(VRCOSCSetting.ChatBoxTimeSpan));
        StartupManager.Start();
        sendControlParameters();
        ModuleManager.Start();
        enableOSCFlag(OscClientFlag.Receive);

        initialiseOSCRouter();

        State.Value = AppManagerState.Started;
    }

    private bool initialiseOSCClient()
    {
        try
        {
            var sendEndpoint = new IPEndPoint(IPAddress.Parse(configManager.Get<string>(VRCOSCSetting.SendAddress)), configManager.Get<int>(VRCOSCSetting.SendPort));
            var receiveEndpoint = new IPEndPoint(IPAddress.Parse(configManager.Get<string>(VRCOSCSetting.ReceiveAddress)), configManager.Get<int>(VRCOSCSetting.ReceivePort));

            OSCClient.Initialise(sendEndpoint, receiveEndpoint);
            return true;
        }
        catch (Exception e)
        {
            notifications.Notify(new ExceptionNotification(e.Message));
            Logger.Error(e, $"{nameof(AppManager)} experienced an exception");
            return false;
        }
    }

    private void enableOSCFlag(OscClientFlag flag)
    {
        try
        {
            OSCClient.Enable(flag);
        }
        catch (Exception e)
        {
            notifications.Notify(new PortInUseNotification(flag == OscClientFlag.Send ? configManager.Get<int>(VRCOSCSetting.SendPort) : configManager.Get<int>(VRCOSCSetting.ReceivePort)));
            Logger.Error(e, $"{nameof(AppManager)} experienced an exception");
        }
    }

    private void initialiseOSCRouter()
    {
        try
        {
            OSCRouter.Initialise(RouterManager.Store);
            OSCRouter.Enable();
        }
        catch (Exception e)
        {
            notifications.Notify(new PortInUseNotification("Cannot initialise a port from OSCRouter"));
            Logger.Error(e, $"{nameof(AppManager)} experienced an exception");
        }
    }

    #endregion

    #region Restart

    public async void Restart() => await RestartAsync();

    public async Task RestartAsync()
    {
        await StopAsync();
        await Task.Delay(250);
        Start();
    }

    #endregion

    #region Stop

    public async void Stop() => await StopAsync();

    public async Task StopAsync()
    {
        if (State.Value is AppManagerState.Stopping or AppManagerState.Stopped) return;

        State.Value = AppManagerState.Stopping;

        await OSCClient.Disable(OscClientFlag.Receive);
        await OSCRouter.Disable();
        ModuleManager.Stop();
        ChatBoxManager.Teardown();
        VRChat.Teardown();
        await OSCClient.Disable(OscClientFlag.Send);

        State.Value = AppManagerState.Stopped;
    }

    #endregion

    #region Processes

    private void checkForOpenVR() => Task.Run(() =>
    {
        OVRClient.Init();
        OVRClient.SetAutoLaunch(configManager.Get<bool>(VRCOSCSetting.AutoStartOpenVR));
    });

    private void checkForVRChat()
    {
        if (!configManager.Get<bool>(VRCOSCSetting.AutoStartStop) || !VRChat.HasOpenStateChanged()) return;

        if (VRChat.IsClientOpen && State.Value == AppManagerState.Stopped) Start();
        if (!VRChat.IsClientOpen && State.Value == AppManagerState.Started) Stop();
    }

    #endregion
}

public enum AppManagerState
{
    Starting,
    Started,
    Stopping,
    Stopped
}
