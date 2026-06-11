// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using org.mariuszgromada.math.mxparser;
using VRCOSC.App.Actions;
using VRCOSC.App.Audio;
using VRCOSC.App.Audio.Whisper;
using VRCOSC.App.ChatBox;
using VRCOSC.App.Dolly;
using VRCOSC.App.Modules;
using VRCOSC.App.Nodes;
using VRCOSC.App.OpenVR;
using VRCOSC.App.OSC;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.Profiles;
using VRCOSC.App.Router;
using VRCOSC.App.SDK.Handlers;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.SDK.VRChat;
using VRCOSC.App.SDK.VRChat.Logs;
using VRCOSC.App.SDK.VRChat.Logs.Handlers;
using VRCOSC.App.Settings;
using VRCOSC.App.Startup;
using VRCOSC.App.SteamVR;
using VRCOSC.App.UI.Themes;
using VRCOSC.App.UI.Windows;
using VRCOSC.App.Utils;
using Module = VRCOSC.App.SDK.Modules.Module;

namespace VRCOSC.App;

internal class AppManager : IVRCClientEventHandler
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
    public Observable<DateTime> LastStartedTime { get; } = new();
    public Observable<Theme> ProxyTheme { get; } = new(Theme.Dark);

    public ConnectionManager ConnectionManager = null!;
    public VRChatOSCClient VRChatOscClient = null!;
    public VRChatClient VRChatClient = null!;
    public ChatBoxWorldBlacklist ChatBoxWorldBlacklist = null!;
    public WhisperSpeechEngine SpeechEngine = null!;
    public GlobalKeyboardHook GlobalKeyboardHook { get; } = new();
    public OpenVRManager OpenVRManager { get; private set; }
    public SteamVRManager SteamVRManager { get; private set; }

    private Repeater vrchatCheckTask = null!;

    private ConcurrentDictionary<ParameterDefinition, (DateTime Timestamp, VRChatParameter Parameter)> parameterCache { get; } = [];

    public AppManager()
    {
        License.iConfirmNonCommercialUse("VolcanicArts");

        State.Subscribe(newState => Logger.Log("AppManager changed state to " + newState));
    }

    public void Initialise()
    {
        SettingsManager.GetInstance().GetObservable<Theme>(VRCOSCSetting.Theme).Subscribe(theme => ProxyTheme.Value = theme, true);

        ConnectionManager = new ConnectionManager();
        VRChatOscClient = new VRChatOSCClient();
        VRChatClient = new VRChatClient(VRChatOscClient);
        ChatBoxWorldBlacklist = new ChatBoxWorldBlacklist();

        SpeechEngine = new WhisperSpeechEngine();

        SpeechEngine.OnPartialResult += result =>
        {
            ModuleManager.GetInstance().GetRunningModulesOfType<ISpeechHandler>().ForEach(module =>
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

            NodeManager.GetInstance().OnPartialSpeechResult(result);
        };

        SpeechEngine.OnFinalResult += result =>
        {
            ModuleManager.GetInstance().GetRunningModulesOfType<ISpeechHandler>().ForEach(module =>
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

            NodeManager.GetInstance().OnFinalSpeechResult(result);
        };

        var chosenInputDeviceSetting = SettingsManager.GetInstance().GetObservable<string>(VRCOSCSetting.SelectedMicrophoneID);

        if (!string.IsNullOrEmpty(chosenInputDeviceSetting.Value) && AudioDeviceHelper.GetDeviceByID(chosenInputDeviceSetting.Value) is null)
        {
            chosenInputDeviceSetting.Value = string.Empty;
        }
    }

    public void InitialLoadComplete()
    {
        VRChatOscClient.Init(ConnectionManager);

        vrchatCheckTask = new Repeater($"{nameof(AppManager)}-{nameof(checkForVRChatAutoStart)}", checkForVRChatAutoStart);
        vrchatCheckTask.Start(TimeSpan.FromSeconds(2));

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

        OpenVRManager = new OpenVRManager();
        SteamVRManager = new SteamVRManager();
    }

    public async void HandleClientEvent(IVRChatClientEvent @event)
    {
        switch (@event)
        {
            case UserAuthenticatedClientEvent userAuthenticatedClientEvent:
                VRChatClient.UpdateUser(userAuthenticatedClientEvent.User);
                break;

            case InstanceJoinedClientEvent instanceJoinedClientEvent:
                VRChatClient.UpdateInstance(instanceJoinedClientEvent.Instance);

                // we only want to update client data on instance events if it was recent
                if (@event.Timestamp >= DateTime.Now - TimeSpan.FromSeconds(1))
                    await updateClientData();
                break;

            case InstanceLeftClientEvent:
                VRChatClient.UpdateInstance(null);

                // we only want to update client data on instance events if it was recent
                if (@event.Timestamp >= DateTime.Now - TimeSpan.FromSeconds(1))
                    await updateClientData();
                break;

            case UserJoinedClientEvent userJoinedClientEvent:
            {
                if (!VRChatClient.IsInInstance) return;

                VRChatClient.Instance.Users.Add(userJoinedClientEvent.User);
                break;
            }

            case UserLeftClientEvent userLeftClientEvent:
            {
                if (!VRChatClient.IsInInstance) return;

                VRChatClient.Instance.Users.RemoveIf(user => user == userLeftClientEvent.User);
                break;
            }

            case AvatarPreChangeClientEvent:
                // we only want to avatar data on avatar events if it was recent
                if (@event.Timestamp >= DateTime.Now - TimeSpan.FromSeconds(1))
                    VRChatClient.UpdateAvatar(null);

                break;
        }
    }

    public VRChatParameter? GetParameter<T>(string name) => parameterCache.GetValueOrDefault(new ParameterDefinition(name, ParameterTypeFactory.CreateFrom<T>())).Parameter;
    public VRChatParameter? GetParameter(string name) => parameterCache.SingleOrDefault(p => p.Value.Parameter.Name == name).Value.Parameter;

    public TemplatedVRChatParameter? GetParameter<T>(Regex pattern)
    {
        var type = ParameterTypeFactory.CreateFrom<T>();
        var parameter = parameterCache.Where(p => p.Key.Type == type).OrderByDescending(p => p.Value.Timestamp).FirstOrDefault(p => pattern.IsMatch(p.Value.Parameter.Name)).Value.Parameter;
        return parameter is not null ? new TemplatedVRChatParameter(pattern, parameter) : null;
    }

    public void SendToAllParameter<T>(string pattern, T value)
    {
        if (!VRChatClient.IsInAvatar) return;

        var regex = new Regex($"^(?:{Regex.Escape(pattern).Replace(@"\*", @"(\S*?)")})$");

        foreach (var def in VRChatClient.Avatar.Parameters.Where(def => regex.IsMatch(def.Name)))
        {
            VRChatOscClient.Send($"{VRChatOSCConstants.ADDRESS_AVATAR_PARAMETERS}/{def.Name}", value);
        }
    }

    /// <summary>
    /// Attempts to get the first parameter that matches <paramref name="pattern"/>. Extracts the value, otherwise default <typeparamref name="T"/>
    /// </summary>
    public T GetParameterValue<T>(Regex pattern) where T : unmanaged => GetParameter<T>(pattern)?.GetValue<T>() ?? default;

    public static bool IsAdministrator => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

    private async Task checkForVRChatAutoStart()
    {
        if (!VRChatClient.CheckIfOpenChanged()) return;

        await ConnectionManager.Stop();

        if (VRChatClient.IsOpen && State.Value == AppManagerState.Stopped && SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.VRCAutoStart)) await RequestStart();
        if (!VRChatClient.IsOpen && State.Value == AppManagerState.Started && SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.VRCAutoStop)) await StopAsync();
    }

    #region OSC

    private async Task onVRChatOSCMessageReceived(VRChatOSCMessage message)
    {
        try
        {
            if (string.IsNullOrEmpty(message.Address)) return;

            if (message.IsAvatarChangeEvent)
            {
                if (ProfileManager.GetInstance().AvatarChange((string)message.ParameterValue)) return;

                await updateClientData();
                ModuleManager.GetInstance().AvatarChange(VRChatClient.Avatar);

                sendMetadataParameters();
                sendControlParameters();
            }

            if (message.IsDollyEvent)
            {
                DollyManager.GetInstance().HandleDollyEvent(message);
            }

            if (message.IsUserCamera)
            {
                VRChatClient.UserCamera.HandleMessage(message);
            }

            if (message.IsAvatarEyeHeight && VRChatClient.IsInAvatar)
            {
                VRChatClient.Avatar.HandleOSCMessage(message);
            }

            if (message.IsAvatarParameter)
            {
                var parameter = new VRChatParameter(message);
                parameterCache[parameter.GetDefinition()] = (DateTime.Now, parameter);

                if (Enum.TryParse<VRChatAvatarParameter>(parameter.Name, out _))
                    ModuleManager.GetInstance().PlayerUpdate();

                if (parameter.Name.StartsWith("VRCOSC/Controls"))
                {
                    handleControlParameter(parameter);
                }

                ModuleManager.GetInstance().OnParameterReceived(parameter);
            }
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e);
        }
    }

    private async Task updateClientData()
    {
        Logger.Log("Updating client data");

        var avatarId = await VRChatOscClient.RequestCurrentAvatar();
        var avatarConfig = avatarId is null || avatarId.StartsWith("local") ? null : AvatarConfigLoader.LoadConfigFor(avatarId);
        var parameters = (await VRChatOscClient.RequestAllParameters()).ToList();

        parameterCache.Clear();

        if (avatarId is not null && avatarConfig is not null)
        {
            Logger.Log($"Found avatar {avatarId} ({avatarConfig.Name}) with {parameters.Count} parameters");

            foreach (var parameter in parameters)
            {
                parameterCache[parameter.GetDefinition()] = (DateTime.Now, parameter);
            }

            VRChatClient.UpdateAvatar(new Avatar(avatarId, avatarConfig.Name, parameters.Select(x => x.GetDefinition()).ToArray()));
        }
        else
        {
            VRChatClient.UpdateAvatar(null);
        }

        if (VRChatClient.IsInAvatar)
        {
            var avatarHeight = await VRChatOscClient.RequestAvatarHeight();
            VRChatClient.Avatar.EyeHeight = avatarHeight.EyeHeight;
            VRChatClient.Avatar.EyeHeightMin = avatarHeight.EyeHeightMin;
            VRChatClient.Avatar.EyeHeightMax = avatarHeight.EyeHeightMax;
            VRChatClient.Avatar.EyeHeightScalingAllowed = avatarHeight.EyeHeightScalingAllowed;
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

    private void handleControlParameter(VRChatParameter parameter)
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
        VRChatOscClient.Send($"{VRChatOSCConstants.ADDRESS_AVATAR_PARAMETERS}/{parameterName}", value);
    }

    #endregion

    #region Start

    private CancellationTokenSource requestStartCancellationSource = null!;

    public async Task ForceStart()
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

    public async Task RequestStart()
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
            if (SettingsManager.GetInstance().GetValue<SpeechModel>(VRCOSCSetting.SpeechModel) == SpeechModel.Custom && string.IsNullOrWhiteSpace(SettingsManager.GetInstance().GetValue<string>(VRCOSCSetting.SpeechModelPath)))
            {
                var result = MessageBox.Show("You have enabled modules that require the speech engine.\nWould you like to automatically set it up?", "Set Up Speech Engine?", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    await InstallSpeechModel(SpeechModel.Small);
                }
                else
                {
                    SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.SpeechEnabled).Value = false;
                }
            }
        }

        if (ModuleManager.GetInstance().GetEnabledModulesOfType<ISpeechHandler>().Any() && SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.SpeechEnabled))
        {
            SpeechEngine.Initialise();
        }

        State.Value = AppManagerState.Starting;
        VRChatLogReader.Register(this);

        await updateClientData();

        await AudioManager.GetInstance().Init();
        StartupManager.GetInstance().OpenFileLocations();
        await RouterManager.GetInstance().Start();
        await VRChatOscClient.EnableSend();
        ChatBoxManager.GetInstance().Start();
        await VRChatClient.UserCamera.RetrieveAllData();
        await ModuleManager.GetInstance().StartAsync();
        await NodeManager.GetInstance().Start();
        VRChatLogReader.Start();

        VRChatOscClient.OnVRChatOSCMessageReceived += onVRChatOSCMessageReceived;
        VRChatOscClient.EnableReceive();

        if (SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.GlobalKeyboardHook))
        {
            Logger.Log("Global keyboard hook has been enabled!");
            GlobalKeyboardHook.Enable();
        }

        State.Value = AppManagerState.Started;
        LastStartedTime.Value = DateTime.Now;

        sendMetadataParameters();
        sendControlParameters();
    }

    public Task InstallSpeechModel(SpeechModel model) => Application.Current.Dispatcher.Invoke(() =>
    {
        var modelName = model switch
        {
            SpeechModel.Tiny => "ggml-tiny.bin",
            SpeechModel.Small => "ggml-small.bin",
            _ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
        };

        var action = new FileDownloadAction(new Uri($"https://huggingface.co/ggerganov/whisper.cpp/resolve/main/{modelName}?download=true"), Storage.GetStorageForDirectory("runtime/whisper"), modelName);

        action.OnComplete += () => SettingsManager.GetInstance().GetObservable<SpeechModel>(VRCOSCSetting.SpeechModel).Value = model;

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
        await RequestStart();
    }

    #endregion

    #region Stop

    public async Task StopAsync()
    {
        if (State.Value is AppManagerState.Stopping or AppManagerState.Stopped) return;

        State.Value = AppManagerState.Stopping;

        if (GlobalKeyboardHook.IsEnabled)
        {
            GlobalKeyboardHook.Disable();
            Logger.Log("Global keyboard hook is disabled!");
        }

        await SpeechEngine.Teardown();
        ProcessFPS.DisposeAll();

        await VRChatOscClient.DisableReceive();
        VRChatOscClient.OnVRChatOSCMessageReceived -= onVRChatOSCMessageReceived;

        await VRChatLogReader.Stop();
        await NodeManager.GetInstance().Stop();
        await ModuleManager.GetInstance().StopAsync();
        await ChatBoxManager.GetInstance().Stop();
        VRChatOscClient.DisableSend();
        await RouterManager.GetInstance().Stop();
        await AudioManager.GetInstance().Stop();

        parameterCache.Clear();
        VRChatLogReader.DeRegister(this);

        VRChatClient.UpdateAvatar(null);
        VRChatClient.UpdateInstance(null);
        VRChatClient.UpdateUser(null);

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

        NodeManager.GetInstance().Unload();
        ChatBoxManager.GetInstance().Unload();
        ModuleManager.GetInstance().UnloadAllModules();
        DollyManager.GetInstance().Unload();

        ProfileManager.GetInstance().ActiveProfile.Value = newProfile;

        DollyManager.GetInstance().Load();
        ModuleManager.GetInstance().LoadAllModules();
        ChatBoxManager.GetInstance().Load();
        NodeManager.GetInstance().Load();
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