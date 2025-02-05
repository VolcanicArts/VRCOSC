// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using FastOSC;
using org.mariuszgromada.math.mxparser;
using Valve.VR;
using VRCOSC.App.Actions;
using VRCOSC.App.Audio;
using VRCOSC.App.Audio.Whisper;
using VRCOSC.App.ChatBox;
using VRCOSC.App.Modules;
using VRCOSC.App.OSC;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.OVR;
using VRCOSC.App.Profiles;
using VRCOSC.App.Router;
using VRCOSC.App.SDK.Handlers;
using VRCOSC.App.SDK.OVR;
using VRCOSC.App.SDK.OVR.Metadata;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.SDK.VRChat;
using VRCOSC.App.Settings;
using VRCOSC.App.Startup;
using VRCOSC.App.UI.Themes;
using VRCOSC.App.UI.Windows;
using VRCOSC.App.Utils;
using Module = VRCOSC.App.SDK.Modules.Module;

namespace VRCOSC.App;

public class AppManager
{
#if DEBUG
    public const string APP_NAME = "VRCOSC-Dev";
#else
    public const string APP_NAME = "VRCOSC";
#endif

    private static Version assemblyVersion => Assembly.GetEntryAssembly()?.GetName().Version ?? new Version();
    public static string Version => $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";

    private static AppManager? instance;
    internal static AppManager GetInstance() => instance ??= new AppManager();

    public readonly Storage Storage = new($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/{APP_NAME}");

    public Observable<AppManagerState> State { get; } = new(AppManagerState.Stopped);
    public Observable<Theme> ProxyTheme { get; } = new(Theme.Dark);

    public ConnectionManager ConnectionManager = null!;
    public VRChatOscClient VRChatOscClient = null!;
    public VRChatClient VRChatClient = null!;
    public OVRClient OVRClient = null!;

    public WhisperSpeechEngine SpeechEngine = null!;

    private Repeater? updateTask;
    private Repeater vrchatCheckTask = null!;
    private Repeater openvrCheckTask = null!;
    private Repeater openvrUpdateTask = null!;

    private readonly Queue<VRChatOscMessage> oscMessageQueue = new();
    private readonly object oscMessageQueueLock = new();

    public AppManager()
    {
        License.iConfirmNonCommercialUse("VolcanicArts");

        State.Subscribe(newState => Logger.Log("AppManager changed state to " + newState));
    }

    public void Initialise()
    {
        OSCEncoder.SetEncoding(Encoding.UTF8);
        OSCDecoder.SetEncoding(Encoding.UTF8);

        SettingsManager.GetInstance().GetObservable<Theme>(VRCOSCSetting.Theme).Subscribe(theme => ProxyTheme.Value = theme, true);

        ConnectionManager = new ConnectionManager();
        VRChatOscClient = new VRChatOscClient();
        VRChatClient = new VRChatClient(VRChatOscClient);
        OVRClient = new OVRClient();
        ChatBoxWorldBlacklist.Init();

        SpeechEngine = new WhisperSpeechEngine();

        SpeechEngine.OnPartialResult += result => ModuleManager.GetInstance().GetRunningModulesOfType<ISpeechHandler>().ForEach(module =>
        {
            try
            {
                module.OnPartialSpeechResult(result);
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e, $"{((Module)module).FullID} experienced an issue calling {nameof(ISpeechHandler.OnPartialSpeechResult)}");
            }
        });

        SpeechEngine.OnFinalResult += result => ModuleManager.GetInstance().GetRunningModulesOfType<ISpeechHandler>().ForEach(module =>
        {
            try
            {
                module.OnFinalSpeechResult(result);
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e, $"{((Module)module).FullID} experienced an issue calling {nameof(ISpeechHandler.OnFinalSpeechResult)}");
            }
        });

        var chosenInputDeviceSetting = SettingsManager.GetInstance().GetObservable<string>(VRCOSCSetting.SelectedMicrophoneID);

        if (!string.IsNullOrEmpty(chosenInputDeviceSetting.Value) && AudioDeviceHelper.GetDeviceByID(chosenInputDeviceSetting.Value) is null)
        {
            chosenInputDeviceSetting.Value = string.Empty;
        }

        OVRClient.SetMetadata(new OVRMetadata
        {
            ApplicationType = EVRApplicationType.VRApplication_Background,
            ApplicationManifest = Storage.GetFullPath("runtime/openvr/app.vrmanifest"),
            ActionManifest = Storage.GetFullPath("runtime/openvr/action_manifest.json")
        });

        OVRClient.OnShutdown += () => Application.Current.Dispatcher.Invoke(() =>
        {
            OVRDeviceManager.GetInstance().Serialise();

            if (SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.OVRAutoClose))
            {
                Application.Current.Shutdown();
            }
        });

        OVRHelper.OnError += m => Logger.Log($"[OpenVR] {m}");
    }

    public void InitialLoadComplete()
    {
        OVRDeviceManager.GetInstance().Deserialise();

        VRChatOscClient.Init(ConnectionManager);

        vrchatCheckTask = new Repeater($"{nameof(AppManager)}-{nameof(checkForVRChatAutoStart)}", checkForVRChatAutoStart);
        vrchatCheckTask.Start(TimeSpan.FromSeconds(2));

        openvrCheckTask = new Repeater($"{nameof(AppManager)}-{nameof(checkForOpenVR)}", checkForOpenVR);
        openvrCheckTask.Start(TimeSpan.FromSeconds(2));

        openvrUpdateTask = new Repeater($"{nameof(AppManager)}-{nameof(updateOVRClient)}", updateOVRClient);
        openvrUpdateTask.Start(TimeSpan.FromSeconds(1d / 60d));

        SettingsManager.GetInstance().GetObservable<ConnectionMode>(VRCOSCSetting.ConnectionMode).Subscribe(async _ =>
        {
            if (State.Value == AppManagerState.Waiting)
            {
                CancelStartRequest();
                return;
            }

            await ConnectionManager.Stop();
            await StopAsync();
        });
    }

    public static bool IsAdministrator => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

    private void updateOVRClient()
    {
        if (State.Value == AppManagerState.Started)
            OVRClient.Update();
    }

    private void update()
    {
        if (State.Value != AppManagerState.Started) return;

        lock (oscMessageQueueLock)
        {
            processOscMessageQueue();
        }
    }

    private void checkForOpenVR() => Task.Run(() =>
    {
        OVRClient.Init();
        OVRClient.SetAutoLaunch(SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.OVRAutoOpen));

        OVRDeviceManager.GetInstance().Update();

        RenderOptions.ProcessRenderMode = OVRClient.HasInitialised ? RenderMode.SoftwareOnly : RenderMode.Default;
    });

    private async void checkForVRChatAutoStart()
    {
        if (!VRChatClient.HasOpenStateChanged(out var clientOpenState)) return;

        await ConnectionManager.Stop();

        if (clientOpenState && State.Value == AppManagerState.Stopped && SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.VRCAutoStart)) RequestStart();
        if (!clientOpenState && State.Value == AppManagerState.Started && SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.VRCAutoStop)) _ = StopAsync();
    }

    #region OSC

    private void onParameterReceived(VRChatOscMessage message)
    {
        if (string.IsNullOrEmpty(message.Address)) return;

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
                var avatarId = (string)message.ParameterValue;

                AvatarConfig? avatarConfig = null;

                if (!avatarId.StartsWith("local"))
                {
                    avatarConfig = AvatarConfigLoader.LoadConfigFor(avatarId);
                }

                VRChatClient.HandleAvatarChange();
                ModuleManager.GetInstance().AvatarChange(avatarConfig);

                if (ProfileManager.GetInstance().AvatarChange((string)message.ParameterValue)) continue;

                sendMetadataParameters();
                sendControlParameters();
            }

            if (message.IsAvatarParameter)
            {
                var wasPlayerUpdated = VRChatClient.Player.Update(message.ParameterName, message.ParameterValue);
                if (wasPlayerUpdated) ModuleManager.GetInstance().PlayerUpdate();

                if (message.ParameterName.StartsWith("VRCOSC/Controls")) handleControlParameter(new ReceivedParameter(message.ParameterName, message.ParameterValue));

                ModuleManager.GetInstance().ParameterReceived(message);
            }
        }
    }

    private void sendMetadataParameters()
    {
        foreach (var module in ModuleManager.GetInstance().Modules.Values.SelectMany(moduleList => moduleList))
        {
            sendParameter($"VRCOSC/Metadata/Modules/{module.FullID}", false);
        }

        foreach (var runningModule in ModuleManager.GetInstance().RunningModules)
        {
            sendParameter($"VRCOSC/Metadata/Modules/{runningModule.FullID}", true);
        }
    }

    private void handleControlParameter(ReceivedParameter parameter)
    {
        if (parameter is { Name: "VRCOSC/Controls/ChatBox/Enabled", Type: ParameterType.Bool })
        {
            ChatBoxManager.GetInstance().SendEnabled = parameter.GetValue<bool>();
        }

        if (parameter.Name.StartsWith("VRCOSC/Controls/ChatBox/Layer/"))
        {
            var layerId = int.Parse(parameter.Name.Split("/").Last());
            if (layerId < 0 || layerId > ChatBoxManager.GetInstance().Timeline.LayerCount - 1) return;

            ChatBoxManager.GetInstance().Timeline.LayerEnabled[layerId] = parameter.GetValue<bool>();
        }
    }

    private void sendControlParameters()
    {
        sendParameter("VRCOSC/Controls/ChatBox/Enabled", ChatBoxManager.GetInstance().SendEnabled);

        var layerEnabledValues = ChatBoxManager.GetInstance().Timeline.LayerEnabled;

        for (var i = 0; i < layerEnabledValues.Length; i++)
        {
            var layerEnabled = layerEnabledValues[i];
            sendParameter($"VRCOSC/Controls/ChatBox/Layer/{i}", layerEnabled);
        }
    }

    private void sendParameter(string parameterName, object value)
    {
        VRChatOscClient.Send($"{VRChatOscConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}{parameterName}", value);
    }

    #endregion

    #region Start

    private CancellationTokenSource requestStartCancellationSource = null!;

    public async void ForceStart()
    {
        Logger.Log("Force starting");
        CancelStartRequest();
        await ConnectionManager.Stop();
        initialiseOSCClient(IPAddress.Loopback, 9000, IPAddress.Loopback, 9001);
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

        if (SettingsManager.GetInstance().GetValue<ConnectionMode>(VRCOSCSetting.ConnectionMode) == ConnectionMode.Custom)
        {
            if (!IsAdministrator)
            {
                MessageBox.Show($"An OSC connection mode of {ConnectionMode.Custom} requires VRCOSC to be ran as administrator. Please restart the app as administrator", "Permission Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Logger.Log("Connecting to VRChat using custom");

            var outgoingEndpoint = SettingsManager.GetInstance().GetValue<string>(VRCOSCSetting.OutgoingEndpoint);
            var outgoingAddress = IPAddress.Parse(outgoingEndpoint.Split(':')[0]);
            var outgoingPort = int.Parse(outgoingEndpoint.Split(':')[1]);

            var incomingEndpoint = SettingsManager.GetInstance().GetValue<string>(VRCOSCSetting.IncomingEndpoint);
            var incomingAddress = IPAddress.Parse(incomingEndpoint.Split(':')[0]);
            var incomingPort = int.Parse(incomingEndpoint.Split(':')[1]);

            initialiseOSCClient(outgoingAddress, outgoingPort, incomingAddress, incomingPort);
            await startAsync();
            return;
        }

        if (SettingsManager.GetInstance().GetValue<ConnectionMode>(VRCOSCSetting.ConnectionMode) == ConnectionMode.LAN)
        {
            if (!IsAdministrator)
            {
                MessageBox.Show($"An OSC connection mode of {ConnectionMode.LAN} requires VRCOSC to be ran as administrator. Please restart the app as administrator", "Permission Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Logger.Log("Connecting to VRChat using LAN");

            State.Value = AppManagerState.Waiting;

            if (!ConnectionManager.IsConnected)
                ConnectionManager.Start();

            await waitForConnectionManager();
            if (requestStartCancellationSource.IsCancellationRequested) return;

            initialiseOSCClient(ConnectionManager.VRChatIP!, ConnectionManager.VRChatReceivePort!.Value, ConnectionManager.VRCOSCIP!, ConnectionManager.VRCOSCReceivePort!.Value);

            await startAsync();
            return;
        }

        await Task.Run(waitForStart, requestStartCancellationSource.Token);
    }

    private async Task waitForStart()
    {
        Logger.Log("Waiting for starting conditions");
        State.Value = AppManagerState.Waiting;

        var waitingCancellationSource = new CancellationTokenSource();

        await Task.WhenAny(new[]
        {
            Task.Run(() => waitForUnity(waitingCancellationSource), requestStartCancellationSource.Token),
            Task.Run(() => waitForVRChat(waitingCancellationSource), requestStartCancellationSource.Token)
        });

        await waitingCancellationSource.CancelAsync();

        if (requestStartCancellationSource.IsCancellationRequested) return;

        if (isVRChatOpen())
        {
            Logger.Log("Found VRChat. Waiting for OSCQuery");

            if (!ConnectionManager.IsConnected)
                ConnectionManager.Start();

            await waitForConnectionManager();
            if (requestStartCancellationSource.IsCancellationRequested) return;

            initialiseOSCClient(IPAddress.Loopback, ConnectionManager.VRChatReceivePort!.Value, IPAddress.Loopback, ConnectionManager.VRCOSCReceivePort!.Value);
        }
        else
        {
            if (isUnityOpen())
            {
                Logger.Log("Found Unity");
                initialiseOSCClient(IPAddress.Loopback, 9000, IPAddress.Loopback, 9001);
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

    private async Task waitForConnectionManager()
    {
        while (!ConnectionManager.IsConnected && !requestStartCancellationSource.IsCancellationRequested)
        {
            await Task.Delay(500);
        }
    }

    private async Task startAsync()
    {
        if (ModuleManager.GetInstance().GetEnabledModulesOfType<ISpeechHandler>().Any() && SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.SpeechEnabled))
        {
            if (string.IsNullOrWhiteSpace(SettingsManager.GetInstance().GetValue<string>(VRCOSCSetting.SpeechModelPath)))
            {
                var result = MessageBox.Show("You have enabled modules that require the speech engine.\nWould you like to automatically set it up?", "Set Up Speech Engine?", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    await InstallSpeechModel();
                }
            }

            SpeechEngine.Initialise();
        }

        State.Value = AppManagerState.Starting;

        StartupManager.GetInstance().OpenFileLocations();
        await RouterManager.GetInstance().Start();
        await VRChatOscClient.EnableSend();
        ChatBoxManager.GetInstance().Start();
        await VRChatClient.Player.RetrieveAll();
        await ModuleManager.GetInstance().StartAsync();
        VRChatLogReader.Start();

        updateTask = new Repeater($"{nameof(AppManager)}-{nameof(update)}", update);
        updateTask.Start(TimeSpan.FromSeconds(1d / 60d));

        VRChatOscClient.OnParameterReceived += onParameterReceived;
        VRChatOscClient.EnableReceive();

        State.Value = AppManagerState.Started;

        sendMetadataParameters();
        sendControlParameters();
    }

    public Task InstallSpeechModel() => Application.Current.Dispatcher.Invoke(() =>
    {
        var action = new FileDownloadAction(new Uri("https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-small.bin?download=true"), Storage.GetStorageForDirectory("runtime/whisper"), "ggml-small.bin");
        action.OnComplete += () => SettingsManager.GetInstance().GetObservable<string>(VRCOSCSetting.SpeechModelPath).Value = Storage.GetStorageForDirectory("runtime/whisper").GetFullPath("ggml-small.bin");
        return MainWindow.GetInstance().ShowLoadingOverlay(action);
    });

    private void initialiseOSCClient(IPAddress sendAddress, int sendPort, IPAddress receiveAddress, int receivePort)
    {
        try
        {
            var sendEndpoint = new IPEndPoint(sendAddress, sendPort);
            var receiveEndpoint = new IPEndPoint(receiveAddress, receivePort);

            Logger.Log($"Initialising OSC with send {sendEndpoint} and receive {receiveEndpoint}");

            VRChatOscClient.Initialise(sendEndpoint, receiveEndpoint);
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"{nameof(AppManager)} experienced an exception", true);
        }
    }

    #endregion

    #region Restart

    public async Task RestartAsync()
    {
        await StopAsync();
        await Task.Delay(200);
        RequestStart();
    }

    #endregion

    #region Stop

    public async Task StopAsync()
    {
        if (State.Value is AppManagerState.Stopping or AppManagerState.Stopped) return;

        State.Value = AppManagerState.Stopping;

        await SpeechEngine.Teardown();

        await VRChatOscClient.DisableReceive();
        VRChatOscClient.OnParameterReceived -= onParameterReceived;

        if (updateTask is not null)
            await updateTask.StopAsync();

        VRChatLogReader.Stop();
        await ModuleManager.GetInstance().StopAsync();
        await ChatBoxManager.GetInstance().Stop();
        VRChatClient.Teardown();
        VRChatOscClient.DisableSend();
        RouterManager.GetInstance().Stop();

        lock (oscMessageQueueLock)
        {
            oscMessageQueue.Clear();
        }

        State.Value = AppManagerState.Stopped;
    }

    #endregion

    #region Profiles

    public void ChangeProfile(Profile newProfile) => Application.Current.Dispatcher.Invoke(async () =>
    {
        var currentProfile = ProfileManager.GetInstance().ActiveProfile.Value;
        if (currentProfile == newProfile) return;

        Debug.Assert(currentProfile is not null);
        Debug.Assert(newProfile is not null);

        Logger.Log($"Changing profile from {currentProfile.Name.Value} ({currentProfile.ID}) to {newProfile.Name.Value} ({newProfile.ID})");

        foreach (var window in Application.Current.Windows.OfType<Window>().Where(w => w != Application.Current.MainWindow))
        {
            window.Close();
        }

        var beforeState = State.Value;

        if (State.Value == AppManagerState.Started)
        {
            await StopAsync();
        }

        ChatBoxManager.GetInstance().Unload();
        ModuleManager.GetInstance().UnloadAllModules();

        ProfileManager.GetInstance().ActiveProfile.Value = newProfile;

        ModuleManager.GetInstance().LoadAllModules();
        ChatBoxManager.GetInstance().Load();
        RouterManager.GetInstance().Load();

        if (beforeState == AppManagerState.Started)
        {
            await Task.Delay(100);
            await startAsync();
        }
    });

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