// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Development;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Threading;
using Valve.VR;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Managers;
using VRCOSC.Game.Modules;
using VRCOSC.Game.OpenVR;
using VRCOSC.Game.OpenVR.Metadata;
using VRCOSC.Game.OSC;
using VRCOSC.Game.OSC.VRChat;

namespace VRCOSC.Game.App;

public partial class AppManager : Component
{
    [Resolved]
    private GameHost host { get; set; } = null!;

    [Resolved]
    private Storage storage { get; set; } = null!;

    [Resolved]
    private VRCOSCConfigManager configManager { get; set; } = null!;

    private static readonly TimeSpan openvr_check_interval = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan vrchat_check_interval = TimeSpan.FromSeconds(5);

    private readonly Queue<VRChatOscMessage> oscMessageQueue = new();
    private ScheduledDelegate? runningModulesDelegate;

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
        initialiseVRChat();
        initialiseDelayedTasks();

        OSCClient.OnParameterReceived += data => Schedule(() => oscMessageQueue.Enqueue(data));
        State.BindValueChanged(e => Logger.Log($"{nameof(AppManager)} state changed to {e.NewValue}"));
    }

    protected override void Update()
    {
        if (!isRunning) return;

        OVRClient.Update();

        processOscMessageQueue();

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
            ApplicationManifest = storage.GetFullPath("openvr/app.vrmanifest"),
            ActionManifest = storage.GetFullPath("openvr/action_manifest.json")
        });

        OVRHelper.OnError += m => Logger.Log($"[OpenVR] {m}");
    }

    private void initialiseModuleManager()
    {
        ModuleManager.Initialise(host, this, new Scheduler(() => ThreadSafety.IsUpdateThread, Clock), storage);
        ModuleManager.Load();
    }

    private void initialiseChatBoxManager()
    {
        ChatBoxManager.Initialise(storage, this, OSCClient, configManager.GetBindable<int>(VRCOSCSetting.ChatBoxTimeSpan));
        ChatBoxManager.Load();
    }

    private void initialiseStartupManager()
    {
        StartupManager.Initialise(storage);
        StartupManager.Load();
    }

    private void initialiseRouterManager()
    {
        RouterManager.Initialise(storage);
        RouterManager.Load();
    }

    private void initialiseVRChat()
    {
        VRChat.Initialise(OSCClient);
    }

    private void initialiseDelayedTasks()
    {
        Scheduler.AddDelayed(checkForOpenVR, openvr_check_interval.TotalMilliseconds, true);
        Scheduler.AddDelayed(checkForVRChat, vrchat_check_interval.TotalMilliseconds, true);
    }

    #endregion

    #region OSC

    private void processOscMessageQueue()
    {
        while (oscMessageQueue.TryDequeue(out var message))
        {
            if (message.IsAvatarChangeEvent)
            {
                VRChat.HandleAvatarChange(message);
                sendControlParameters();
            }

            if (message.IsAvatarParameter)
            {
                var wasPlayerUpdated = VRChat.Player.Update(message.ParameterName, message.ParameterValue);
                if (wasPlayerUpdated) ModuleManager.PlayerUpdate();

                if (message.ParameterName.StartsWith("VRCOSC/Controls")) processControlParameters(message);
            }

            ModuleManager.ParameterReceived(message);
        }
    }

    private void sendControlParameters()
    {
        OSCClient.SendValue($"{VRChatOscConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}/VRCOSC/Controls/ChatBox", ChatBoxManager.SendEnabled);
    }

    private void processControlParameters(VRChatOscMessage message)
    {
        switch (message.ParameterName)
        {
            case "VRCOSC/Controls/ChatBox":
                ChatBoxManager.SendEnabled = (bool)message.ParameterValue;
                break;
        }
    }

    private void scheduleModuleEnabledParameters()
    {
        runningModulesDelegate = Scheduler.AddDelayed(() =>
        {
            ModuleManager.Modules.ForEach(module => sendModuleRunningState(module, ModuleManager.IsModuleRunning(module)));
        }, TimeSpan.FromSeconds(1).TotalMilliseconds, true);
    }

    private void cancelRunningModulesDelegate()
    {
        runningModulesDelegate?.Cancel();
        runningModulesDelegate = null;
        ModuleManager.Modules.ForEach(module => sendModuleRunningState(module, false));
    }

    private void sendModuleRunningState(Module module, bool running)
    {
        OSCClient.SendValue($"{VRChatOscConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}/VRCOSC/Modules/{module.GetType().Name.Replace("Module", string.Empty)}", running);
    }

    #endregion

    #region Start

    public void Start()
    {
        if (State.Value is AppManagerState.Starting or AppManagerState.Started) return;

        if (!initialiseOSCClient()) return;
        if (!OSCClient.EnableSend() || !OSCClient.EnableReceive()) return;

        State.Value = AppManagerState.Starting;

        // Continue with start 1 update loop later to allow the UI to switch to the running screen if auto-starting so the terminal logs all start events
        Scheduler.Add(() =>
        {
            ChatBoxManager.Start();
            StartupManager.Start();
            ModuleManager.Start();
            scheduleModuleEnabledParameters();
            sendControlParameters();
            startOSCRouter();

            State.Value = AppManagerState.Started;
        });
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
            Notifications.Notify(new ExceptionNotification(e.Message));
            Logger.Error(e, $"{nameof(AppManager)} experienced an exception");
            return false;
        }
    }

    private void startOSCRouter()
    {
        try
        {
            OSCRouter.Start(RouterManager.Store);
        }
        catch (Exception e)
        {
            Notifications.Notify(new PortInUseNotification("Cannot initialise a port from OSCRouter"));
            Logger.Error(e, $"{nameof(AppManager)} experienced an exception");
        }
    }

    #endregion

    #region Restart

    public async void Restart() => await RestartAsync();

    public async Task RestartAsync()
    {
        await StopAsync();
        await Task.Delay(500);
        Start();
    }

    #endregion

    #region Stop

    public async void Stop() => await StopAsync();

    public async Task StopAsync()
    {
        if (State.Value is AppManagerState.Stopping or AppManagerState.Stopped) return;

        State.Value = AppManagerState.Stopping;

        await OSCClient.DisableReceive();
        await OSCRouter.Disable();
        cancelRunningModulesDelegate();
        ModuleManager.Stop();
        ChatBoxManager.Teardown();
        VRChat.Teardown();
        OSCClient.DisableSend();
        oscMessageQueue.Clear();

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
