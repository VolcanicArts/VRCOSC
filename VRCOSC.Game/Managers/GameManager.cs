// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Avatar;
using VRCOSC.Game.OpenVR;
using VRCOSC.Game.OpenVR.Metadata;
using VRCOSC.Game.OSC;
using VRCOSC.Game.OSC.Client;
using VRCOSC.Game.OSC.VRChat;

namespace VRCOSC.Game.Managers;

public partial class GameManager : Component
{
    private const double openvr_check_interval = 1000;
    private const double vrchat_process_check_interval = 5000;

    private readonly TerminalLogger logger = new("VRCOSC");

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

    [Resolved]
    private Storage storage { get; set; } = null!;

    [Resolved]
    private GameHost host { get; set; } = null!;

    [Resolved]
    private IVRCOSCSecrets secrets { get; set; } = null!;

    [Resolved]
    private ChatBoxManager chatBoxManager { get; set; } = null!;

    [Resolved]
    private StartupManager startupManager { get; set; } = null!;

    private Bindable<bool> autoStartStop = null!;
    private bool previousVRChatState;
    private bool hasAutoStarted;
    private readonly List<VRChatOscData> oscDataCache = new();
    private readonly object oscDataCacheLock = new();

    public readonly VRChatOscClient VRChatOscClient = new();
    public readonly Bindable<GameManagerState> State = new(GameManagerState.Stopped);
    public ModuleManager ModuleManager = null!;
    public OSCRouter OSCRouter = null!;
    public Player Player = null!;
    public OVRClient OVRClient = null!;
    public ChatBoxManager ChatBoxManager => chatBoxManager;
    public AvatarConfig? AvatarConfig;

    [BackgroundDependencyLoader]
    private void load()
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

        setupModules();

        chatBoxManager.Load(storage, this, notifications);
    }

    private void setupModules()
    {
        ModuleManager = new ModuleManager();
        ModuleManager.InjectModuleDependencies(host, this, secrets, new Scheduler(() => ThreadSafety.IsUpdateThread, Clock));
        ModuleManager.Load(storage, notifications);
    }

    protected override void Update()
    {
        if (State.Value != GameManagerState.Started) return;

        OVRClient.Update();

        handleOscDataCache();

        ModuleManager.Update();

        ChatBoxManager.Update();
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

        editingModule.BindValueChanged(e =>
        {
            if (e.NewValue is null && e.OldValue is not null) e.OldValue.Serialise();
        }, true);
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
                    var avatarIdRaw = (string)data.ParameterValue;

                    if (!avatarIdRaw.StartsWith("local"))
                    {
                        var avatarId = avatarIdRaw[..avatar_id_format.Length];
                        AvatarConfig = AvatarConfigLoader.LoadConfigFor(avatarId);
                    }

                    sendControlValues();
                }
                else
                {
                    var wasPlayerUpdate = Player.Update(data.ParameterName, data.ParameterValue);
                    if (wasPlayerUpdate) ModuleManager.PlayerUpdate();

                    switch (data.ParameterName)
                    {
                        case @"VRCOSC/Controls/ChatBox":
                            ChatBoxManager.SendEnabled = (bool)data.ParameterValue;
                            break;
                    }
                }

                ModuleManager.ParamaterReceived(data);
            });
            oscDataCache.Clear();
        }
    }

    public async void Restart() => await RestartAsync();

    public async Task RestartAsync()
    {
        await StopAsync();
        await Task.Delay(250);
        Start();
    }

    public void Start()
    {
        if (State.Value is GameManagerState.Starting or GameManagerState.Started) return;

        lock (oscDataCacheLock) { oscDataCache.Clear(); }

        if (!initialiseOscClient())
        {
            hasAutoStarted = false;
            return;
        }

        var moduleEnabled = new Dictionary<string, bool>();
        ModuleManager.Modules.ForEach(module => moduleEnabled.Add(module.SerialisedName, module.Enabled.Value));

        AvatarConfig = null;

        State.Value = GameManagerState.Starting;

        enableOscFlag(OscClientFlag.Send);
        Player.Initialise();
        ChatBoxManager.Initialise(VRChatOscClient, configManager.GetBindable<int>(VRCOSCSetting.ChatBoxTimeSpan), moduleEnabled);
        startupManager.Start();
        sendControlValues();
        ModuleManager.Start();
        enableOscFlag(OscClientFlag.Receive);

        try
        {
            OSCRouter.Initialise(routerManager.Store);
            OSCRouter.Enable();
        }
        catch
        {
            notifications.Notify(new PortInUseNotification("Cannot initialise a port from OSCRouter"));
        }

        State.Value = GameManagerState.Started;
    }

    private void enableOscFlag(OscClientFlag flag)
    {
        try
        {
            VRChatOscClient.Enable(flag);
        }
        catch
        {
            notifications.Notify(new PortInUseNotification(flag == OscClientFlag.Send ? configManager.Get<int>(VRCOSCSetting.SendPort) : configManager.Get<int>(VRCOSCSetting.ReceivePort)));
        }
    }

    public async void Stop() => await StopAsync();

    public async Task StopAsync()
    {
        if (State.Value is GameManagerState.Stopping or GameManagerState.Stopped) return;

        State.Value = GameManagerState.Stopping;

        await VRChatOscClient.Disable(OscClientFlag.Receive);
        await OSCRouter.Disable();
        ModuleManager.Stop();
        ChatBoxManager.Shutdown();
        Player.ResetAll();
        await VRChatOscClient.Disable(OscClientFlag.Send);

        State.Value = GameManagerState.Stopped;
    }

    private bool initialiseOscClient()
    {
        try
        {
            var sendEndpoint = new IPEndPoint(IPAddress.Parse(configManager.Get<string>(VRCOSCSetting.SendAddress)), configManager.Get<int>(VRCOSCSetting.SendPort));
            var receiveEndpoint = new IPEndPoint(IPAddress.Parse(configManager.Get<string>(VRCOSCSetting.ReceiveAddress)), configManager.Get<int>(VRCOSCSetting.ReceivePort));

            VRChatOscClient.Initialise(sendEndpoint, receiveEndpoint);
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
        OVRClient.Init();
        OVRClient.SetAutoLaunch(configManager.Get<bool>(VRCOSCSetting.AutoStartOpenVR));
    });

    private void checkForVRChat()
    {
        if (!configManager.Get<bool>(VRCOSCSetting.AutoStartStop)) return;

        var vrChatCurrentState = Process.GetProcessesByName(@"vrchat").Any();
        var vrChatStateChanged = vrChatCurrentState != previousVRChatState;
        if (!vrChatStateChanged) return;

        previousVRChatState = vrChatCurrentState;

        // hasAutoStarted is checked here to ensure that modules aren't started immediately
        // after a user has manually stopped the modules
        if (vrChatCurrentState && State.Value == GameManagerState.Stopped && !hasAutoStarted)
        {
            Start();
            hasAutoStarted = true;
        }

        if (!vrChatCurrentState && State.Value == GameManagerState.Started)
        {
            logger.Log("VRChat is no longer open. Stopping modules");
            Stop();
            hasAutoStarted = false;
        }
    }

    private void sendControlValues()
    {
        VRChatOscClient.SendValue(@$"{VRChatOscConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}/VRCOSC/Controls/ChatBox", ChatBoxManager.SendEnabled);
    }
}

public enum GameManagerState
{
    Starting,
    Started,
    Stopping,
    Stopped
}
