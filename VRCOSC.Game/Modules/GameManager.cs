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
using osu.Framework.Development;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Threading;
using Valve.VR;
using VRCOSC.Game.ChatBox;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Graphics.Startup;
using VRCOSC.Game.Modules.Avatar;
using VRCOSC.Game.Modules.Manager;
using VRCOSC.Game.Modules.Serialisation;
using VRCOSC.Game.Modules.Sources;
using VRCOSC.Game.OpenVR;
using VRCOSC.Game.OpenVR.Metadata;
using VRCOSC.Game.OSC;
using VRCOSC.Game.OSC.Client;
using VRCOSC.Game.OSC.VRChat;

namespace VRCOSC.Game.Modules;

public partial class GameManager : Component
{
    private const double openvr_check_interval = 1000;
    private const double vrchat_process_check_interval = 5000;
    private const int startstop_delay = 250;

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
    public IModuleManager ModuleManager = null!;
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
        ModuleManager.AddSource(new InternalModuleSource());
        ModuleManager.AddSource(new ExternalModuleSource(storage));
        ModuleManager.SetSerialiser(new ModuleSerialiser(storage));
        ModuleManager.InjectModuleDependencies(host, this, secrets, new Scheduler(() => ThreadSafety.IsUpdateThread, Clock));
        ModuleManager.Load();
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
                    Player.Update(data.ParameterName, data.ParameterValue);

                    switch (data.ParameterName)
                    {
                        case @"VRCOSC/Controls/ChatBox":
                            ChatBoxManager.SendEnabled = (bool)data.ParameterValue;
                            break;
                    }
                }

                foreach (var module in ModuleManager)
                {
                    module.OnParameterReceived(data);
                }
            });
            oscDataCache.Clear();
        }
    }

    public void Restart() => Task.Run(async () =>
    {
        Stop();

        while (State.Value != GameManagerState.Stopped) { }

        await Task.Delay(250);
        Start();
    });

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

        AvatarConfig = null;

        if (!initialiseOscClient())
        {
            hasAutoStarted = false;
            return;
        }

        var moduleEnabled = new Dictionary<string, bool>();
        ModuleManager.ForEach(module => moduleEnabled.Add(module.SerialisedName, module.Enabled.Value));

        State.Value = GameManagerState.Starting;

        await Task.Delay(startstop_delay);

        enableOscFlag(OscClientFlag.Send);
        Player.Initialise();
        ChatBoxManager.Initialise(VRChatOscClient, configManager.GetBindable<int>(VRCOSCSetting.ChatBoxTimeSpan), moduleEnabled);
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

        startupManager.Start();

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
        ChatBoxManager.Shutdown();
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
