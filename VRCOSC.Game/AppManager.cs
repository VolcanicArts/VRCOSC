// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Framework.Development;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Framework.Timing;
using Valve.VR;
using VRCOSC.Config;
using VRCOSC.Modules;
using VRCOSC.OSC;
using VRCOSC.OSC.VRChat;
using VRCOSC.OVR;
using VRCOSC.OVR.Metadata;
using VRCOSC.Packages;
using VRCOSC.Profiles;
using VRCOSC.SDK.VRChat;

namespace VRCOSC;

public class AppManager
{
    public ProfileManager ProfileManager { get; private set; } = null!;
    public ModuleManager ModuleManager { get; private set; } = null!;
    public PackageManager PackageManager { get; private set; } = null!;
    public VRChatOscClient VRChatOscClient { get; private set; } = null!;
    public VRChatClient VRChatClient { get; private set; } = null!;
    public ConnectionManager ConnectionManager { get; private set; } = null!;
    public OVRClient OVRClient { get; private set; } = null!;

    private VRCOSCGame game = null!;
    private VRCOSCConfigManager configManager = null!;
    private Scheduler localScheduler = null!;
    private Scheduler oscQueueScheduler = null!;

    private bool oscInitialised;

    private readonly Queue<VRChatOscMessage> oscMessageQueue = new();

    public readonly Bindable<AppManagerState> State = new(AppManagerState.Stopped);

    public void Initialise(GameHost host, VRCOSCGame game, Storage storage, IClock clock, VRCOSCConfigManager configManager)
    {
        this.game = game;
        this.configManager = configManager;
        localScheduler = new Scheduler(() => ThreadSafety.IsUpdateThread, clock);
        oscQueueScheduler = new Scheduler(() => ThreadSafety.IsUpdateThread, clock);

        ProfileManager = new ProfileManager(this, storage, configManager);
        ModuleManager = new ModuleManager(host, storage, clock, this, configManager);
        PackageManager = new PackageManager(game, this, storage, configManager);
        VRChatOscClient = new VRChatOscClient();
        VRChatClient = new VRChatClient(VRChatOscClient);
        ConnectionManager = new ConnectionManager(clock);
        OVRClient = new OVRClient();

        OVRClient.SetMetadata(new OVRMetadata
        {
            ApplicationType = EVRApplicationType.VRApplication_Background,
            ApplicationManifest = storage.GetFullPath("openvr/app.vrmanifest"),
            ActionManifest = storage.GetFullPath("openvr/action_manifest.json")
        });

        OVRHelper.OnError += m => Logger.Log($"[OpenVR] {m}");

        VRChatOscClient.Init(ConnectionManager);
        ConnectionManager.Init();

        localScheduler.AddDelayed(checkForOpenVR, 1000, true);
        localScheduler.AddDelayed(checkForVRChat, 5000, true);
        localScheduler.AddDelayed(initialiseOSC, 500, true);
    }

    public void FrameworkUpdate()
    {
        ConnectionManager.FrameworkUpdate();
        localScheduler.Update();

        if (State.Value != AppManagerState.Started) return;

        oscQueueScheduler.Update();
        processOscMessageQueue();
        ModuleManager.FrameworkUpdate();
    }

    private void checkForOpenVR() => Task.Run(() => OVRClient.Init());

    private void checkForVRChat()
    {
        var newOpenState = VRChatClient.HasOpenStateChanged();

        if (!configManager.Get<bool>(VRCOSCSetting.VRCAutoStart) || !newOpenState) return;

        if (VRChatClient.ClientOpen && State.Value == AppManagerState.Stopped) Start();
        if (!VRChatClient.ClientOpen && State.Value == AppManagerState.Started) Stop();
    }

    #region Profiles

    public async void ChangeProfile(Profile newProfile)
    {
        if (ProfileManager.ActiveProfile.Value == newProfile) return;

        Logger.Log($"Changing profile from {ProfileManager.ActiveProfile.Value.Name.Value} to {newProfile.Name.Value}");

        var beforeState = State.Value;

        if (State.Value == AppManagerState.Started)
        {
            await StopAsync();
        }

        ModuleManager.UnloadAllModules();
        ProfileManager.ActiveProfile.Value = newProfile;
        ModuleManager.LoadAllModules();

        game.OnListingRefresh?.Invoke();

        if (beforeState == AppManagerState.Started)
        {
            await Task.Delay(100);
            await StartAsync();
        }
    }

    #endregion

    #region OSC

    private void processOscMessageQueue()
    {
        while (oscMessageQueue.TryDequeue(out var message))
        {
            if (message.IsAvatarChangeEvent)
            {
                if (ProfileManager.AvatarChange((string)message.ParameterValue)) continue;
            }

            if (message.IsAvatarParameter)
            {
                var wasPlayerUpdated = VRChatClient.Player.Update(message.ParameterName, message.ParameterValue);
                if (wasPlayerUpdated) ModuleManager.PlayerUpdate();
            }

            ModuleManager.ParameterReceived(message);
        }
    }

    #endregion

    #region Start

    public async void Start() => await StartAsync();

    public async Task StartAsync()
    {
        if (State.Value is AppManagerState.Starting or AppManagerState.Started) return;

        State.Value = AppManagerState.Starting;

        await ModuleManager.StartAsync();
        VRChatOscClient.OnParameterReceived += onParameterReceived;

        State.Value = AppManagerState.Started;
    }

    private void initialiseOSC()
    {
        if (State.Value != AppManagerState.Started || oscInitialised) return;

        oscInitialised = true;

        if (!initialiseOSCClient()) return;

        VRChatOscClient.EnableSend();
        VRChatOscClient.EnableReceive();
    }

    private void onParameterReceived(VRChatOscMessage message)
    {
        oscQueueScheduler.Add(() => oscMessageQueue.Enqueue(message));
    }

    private bool initialiseOSCClient()
    {
        try
        {
            if (!configManager.Get<bool>(VRCOSCSetting.UseLegacyPorts) && !ConnectionManager.IsConnected) return false;

            var sendPort = configManager.Get<bool>(VRCOSCSetting.UseLegacyPorts) ? 9000 : ConnectionManager.SendPort!.Value;
            var receivePort = configManager.Get<bool>(VRCOSCSetting.UseLegacyPorts) ? 9001 : ConnectionManager.ReceivePort;

            Logger.Log($"Initialising OSC with send ({sendPort}) and receive ({receivePort}). UseLegacyPorts: {configManager.Get<bool>(VRCOSCSetting.UseLegacyPorts)}");

            var sendEndpoint = new IPEndPoint(IPAddress.Loopback, sendPort);
            var receiveEndpoint = new IPEndPoint(IPAddress.Loopback, receivePort);

            VRChatOscClient.Initialise(sendEndpoint, receiveEndpoint);
            return true;
        }
        catch (Exception e)
        {
            Logger.Error(e, $"{nameof(AppManager)} experienced an exception");
            return false;
        }
    }

    #endregion

    #region Restart

    public async void Restart() => await RestartAsync();

    public async Task RestartAsync()
    {
        await StopAsync();
        await Task.Delay(100);
        await StartAsync();
    }

    #endregion

    #region Stop

    public async void Stop() => await StopAsync();

    public async Task StopAsync()
    {
        if (State.Value is AppManagerState.Stopping or AppManagerState.Stopped) return;

        State.Value = AppManagerState.Stopping;

        await VRChatOscClient.DisableReceive();
        VRChatOscClient.OnParameterReceived -= onParameterReceived;
        await ModuleManager.StopAsync();
        VRChatClient.Teardown();
        VRChatOscClient.DisableSend();
        oscMessageQueue.Clear();
        oscInitialised = false;

        State.Value = AppManagerState.Stopped;
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
