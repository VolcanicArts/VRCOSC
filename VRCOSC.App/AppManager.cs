// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using VRCOSC.App.Actions;
using VRCOSC.App.Modules;
using VRCOSC.App.OSC;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.Profiles;
using VRCOSC.App.Settings;
using VRCOSC.App.Utils;

namespace VRCOSC.App;

public class AppManager
{
    private static AppManager? instance;
    public static AppManager GetInstance() => instance ??= new AppManager();

    private ProgressAction? progressAction;

    public ProgressAction? ProgressAction
    {
        get => progressAction;
        set
        {
            progressAction = value;

            if (progressAction is not null)
            {
                progressAction.OnComplete += () => ProgressAction = null;
                ((MainWindow)Application.Current.MainWindow).ShowLoadingOverlay(progressAction);
                _ = progressAction?.Execute();
            }
            else
            {
                ((MainWindow)Application.Current.MainWindow).HideLoadingOverlay();
            }
        }
    }

    public Observable<AppManagerState> State = new(AppManagerState.Stopped);

    private readonly Dictionary<PageLookup, IVRCOSCPage> pageInstances = new();

    public ConnectionManager ConnectionManager;
    public VRChatOscClient VRChatOscClient;

    private Repeater updateTask;

    private readonly Queue<VRChatOscMessage> oscMessageQueue = new();
    private readonly object oscMessageQueueLock = new();

    public AppManager()
    {
        State.Subscribe(newState => Logger.Log("AppManager changed state to " + newState));
    }

    public void RegisterPage(PageLookup pageLookup, IVRCOSCPage instance)
    {
        pageInstances[pageLookup] = instance;
    }

    public void Refresh(PageLookup flags)
    {
        if ((flags & PageLookup.Home) == PageLookup.Home && pageInstances.TryGetValue(PageLookup.Home, out var homePage))
            homePage.Refresh();

        if ((flags & PageLookup.Packages) == PageLookup.Packages && pageInstances.TryGetValue(PageLookup.Packages, out var packagePage))
            packagePage.Refresh();

        if ((flags & PageLookup.Modules) == PageLookup.Modules && pageInstances.TryGetValue(PageLookup.Modules, out var modulesPage))
            modulesPage.Refresh();
    }

    public void Initialise()
    {
        ConnectionManager = new ConnectionManager();
        VRChatOscClient = new VRChatOscClient();

        VRChatOscClient.Init(ConnectionManager);
        ConnectionManager.Init();
    }

    private void update()
    {
        if (State.Value != AppManagerState.Started) return;

        lock (oscMessageQueueLock)
        {
            processOscMessageQueue();
        }
    }

    #region OSC

    private void onParameterReceived(VRChatOscMessage message)
    {
        lock (oscMessageQueueLock)
        {
            oscMessageQueue.Enqueue(message);
        }
    }

    private void processOscMessageQueue()
    {
        while (oscMessageQueue.TryDequeue(out var message))
        {
            if (message.IsAvatarChangeEvent)
            {
                if (ProfileManager.GetInstance().AvatarChange((string)message.ParameterValue)) continue;
            }

            if (message.IsAvatarParameter)
            {
                // var wasPlayerUpdated = VRChatClient.Player.Update(message.ParameterName, message.ParameterValue);
                // if (wasPlayerUpdated) ModuleManager.PlayerUpdate();
            }

            ModuleManager.GetInstance().ParameterReceived(message);
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

        if (SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.UseLegacyPorts))
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

        await Task.WhenAny([
            Task.Run(() => waitForUnity(waitingCancellationSource), requestStartCancellationSource.Token),
            Task.Run(() => waitForVRChat(waitingCancellationSource), requestStartCancellationSource.Token)
        ]);

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
            await Task.Delay(500);
        }
    }

    private async Task waitForVRChat(CancellationTokenSource waitingSource)
    {
        while (!isVRChatOpen() && !requestStartCancellationSource.IsCancellationRequested && !waitingSource.IsCancellationRequested)
        {
            await Task.Delay(500);
        }
    }

    private async Task waitForOSCQuery()
    {
        while (!ConnectionManager.IsConnected && !requestStartCancellationSource.IsCancellationRequested)
        {
            await Task.Delay(500);
        }
    }

    private async Task startAsync()
    {
        State.Value = AppManagerState.Starting;

        //RouterManager.Start();

        VRChatOscClient.EnableSend();
        await ModuleManager.GetInstance().StartAsync();

        updateTask = new Repeater(update);
        updateTask.Start(TimeSpan.FromSeconds(1d / 60d));

        VRChatOscClient.OnParameterReceived += onParameterReceived;
        VRChatOscClient.EnableReceive();

        State.Value = AppManagerState.Started;
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

        //RouterManager.Stop();

        await VRChatOscClient.DisableReceive();
        VRChatOscClient.OnParameterReceived -= onParameterReceived;
        await updateTask.StopAsync();
        await ModuleManager.GetInstance().StopAsync();
        //VRChatClient.Teardown();
        VRChatOscClient.DisableSend();

        lock (oscMessageQueueLock)
        {
            oscMessageQueue.Clear();
        }

        State.Value = AppManagerState.Stopped;
    }

    #endregion

    #region Profiles

    public async void ChangeProfile(Profile newProfile)
    {
        if (ProfileManager.GetInstance().ActiveProfile.Value == newProfile) return;

        Logger.Log($"Changing profile from {ProfileManager.GetInstance().ActiveProfile.Value.Name.Value} to {newProfile.Name.Value}");

        var beforeState = State.Value;

        if (State.Value == AppManagerState.Started)
        {
            await StopAsync();
        }

        ModuleManager.GetInstance().UnloadAllModules();
        ProfileManager.GetInstance().ActiveProfile.Value = newProfile;
        ModuleManager.GetInstance().LoadAllModules();
        //RouterManager.Load();

        if (beforeState == AppManagerState.Started)
        {
            await Task.Delay(100);
            await startAsync();
        }
    }

    #endregion
}

[Flags]
public enum PageLookup
{
    Home = 1 << 0,
    Packages = 1 << 1,
    Modules = 1 << 2
}

public enum AppManagerState
{
    Waiting,
    Starting,
    Started,
    Stopping,
    Stopped
}
