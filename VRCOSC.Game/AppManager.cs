// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
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
        localScheduler.AddDelayed(checkForVRChatAutoStart, 1000, true);
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

    private void checkForOpenVR() => Task.Run(() =>
    {
        OVRClient.Init();
        OVRClient.SetAutoLaunch(configManager.Get<bool>(VRCOSCSetting.OVRAutoOpen));
    });

    private void checkForVRChatAutoStart()
    {
        if (!VRChatClient.HasOpenStateChanged(out var clientOpenState)) return;

        if (clientOpenState && State.Value == AppManagerState.Stopped && configManager.Get<bool>(VRCOSCSetting.VRCAutoStart)) RequestStart();
        if (!clientOpenState && State.Value == AppManagerState.Started && configManager.Get<bool>(VRCOSCSetting.VRCAutoStop)) Stop();
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
            await startAsync();
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

    private CancellationTokenSource requestStartCancellationSource = null!;

    public async void ForceStart()
    {
        CancelStartRequest();
        initialiseOSCClient(9000, 9001);
        await startAsync();
    }

    public void CancelStartRequest()
    {
        requestStartCancellationSource.Cancel();
        State.Value = AppManagerState.Stopped;
    }

    public async void RequestStart()
    {
        if (State.Value is AppManagerState.Waiting or AppManagerState.Starting or AppManagerState.Started) return;

        requestStartCancellationSource = new CancellationTokenSource();

        if (configManager.Get<bool>(VRCOSCSetting.UseLegacyPorts))
        {
            initialiseOSCClient(9000, 9001);
            await startAsync();
            return;
        }

        State.Value = AppManagerState.Waiting;
        await Task.Run(waitForStart, requestStartCancellationSource.Token);
    }

    private async Task waitForStart()
    {
        Logger.Log("Waiting for starting conditions");

        var waitingCancellationSource = new CancellationTokenSource();

        await Task.WhenAny(new[]
        {
            Task.Run(() => waitForUnity(waitingCancellationSource), requestStartCancellationSource.Token),
            Task.Run(() => waitForVRChat(waitingCancellationSource), requestStartCancellationSource.Token)
        });

        waitingCancellationSource.Cancel();

        if (requestStartCancellationSource.IsCancellationRequested) return;

        if (isVRChatOpen())
        {
            Logger.Log("Found VRChat. Waiting for OSCQuery");
            ConnectionManager.Reset();
            await waitForOSCQuery();
            if (requestStartCancellationSource.IsCancellationRequested) return;

            initialiseOSCClient(ConnectionManager.VRChatSendPort!.Value, ConnectionManager.VRCOSCReceivePort);
        }
        else
        {
            if (isUnityOpen())
            {
                Logger.Log("Found Unity");
                initialiseOSCClient(9000, 9001);
            }
        }

        await startAsync();
    }

    private static bool isVRChatOpen() => Process.GetProcessesByName("vrchat").Any();
    private static bool isUnityOpen() => Process.GetProcessesByName("unity").Any();

    private async Task waitForUnity(CancellationTokenSource waitingSource)
    {
        while (!isUnityOpen() && !requestStartCancellationSource.IsCancellationRequested && !waitingSource.IsCancellationRequested)
        {
            await Task.Delay(500, requestStartCancellationSource.Token);
        }
    }

    private async Task waitForVRChat(CancellationTokenSource waitingSource)
    {
        while (!isVRChatOpen() && !requestStartCancellationSource.IsCancellationRequested && !waitingSource.IsCancellationRequested)
        {
            await Task.Delay(500, requestStartCancellationSource.Token);
        }
    }

    private async Task waitForOSCQuery()
    {
        while (!ConnectionManager.IsConnected && !requestStartCancellationSource.IsCancellationRequested)
        {
            await Task.Delay(500, requestStartCancellationSource.Token);
        }
    }

    private async Task startAsync()
    {
        State.Value = AppManagerState.Starting;

        VRChatOscClient.EnableSend();
        await ModuleManager.StartAsync();
        VRChatOscClient.OnParameterReceived += onParameterReceived;
        VRChatOscClient.EnableReceive();

        State.Value = AppManagerState.Started;
    }

    private void onParameterReceived(VRChatOscMessage message)
    {
        oscQueueScheduler.Add(() => oscMessageQueue.Enqueue(message));
    }

    private void initialiseOSCClient(int sendPort, int receivePort)
    {
        try
        {
            Logger.Log($"Initialising OSC with send ({sendPort}) and receive ({receivePort})");

            var sendEndpoint = new IPEndPoint(IPAddress.Loopback, sendPort);
            var receiveEndpoint = new IPEndPoint(IPAddress.Loopback, receivePort);

            VRChatOscClient.Initialise(sendEndpoint, receiveEndpoint);
        }
        catch (Exception e)
        {
            Logger.Error(e, $"{nameof(AppManager)} experienced an exception");
        }
    }

    #endregion

    #region Restart

    public async void Restart() => await RestartAsync();

    public async Task RestartAsync()
    {
        await StopAsync();
        await Task.Delay(100);
        RequestStart();
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

        State.Value = AppManagerState.Stopped;
    }

    #endregion
}

public enum AppManagerState
{
    Waiting,
    Starting,
    Started,
    Stopping,
    Stopped
}
