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
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Remote;
using VRCOSC.Game.Modules.SDK.VRChat;
using VRCOSC.Game.OSC.VRChat;
using VRCOSC.Game.Profiles;

namespace VRCOSC.Game;

public class AppManager
{
    public ProfileManager ProfileManager { get; private set; } = null!;
    public ModuleManager ModuleManager { get; private set; } = null!;
    public RemoteModuleSourceManager RemoteModuleSourceManager { get; private set; } = null!;
    public VRChatOscClient VRChatOscClient { get; private set; } = null!;
    public VRChatClient VRChatClient { get; private set; } = null!;

    private Scheduler scheduler = null!;

    private readonly Queue<VRChatOscMessage> oscMessageQueue = new();

    public readonly Bindable<AppManagerState> State = new(AppManagerState.Stopped);

    public void Initialise(Storage storage, IClock clock)
    {
        scheduler = new Scheduler(() => ThreadSafety.IsUpdateThread, clock);

        ProfileManager = new ProfileManager(storage);
        ModuleManager = new ModuleManager(storage, clock, this);
        RemoteModuleSourceManager = new RemoteModuleSourceManager(storage);
        VRChatOscClient = new VRChatOscClient();
        VRChatClient = new VRChatClient(VRChatOscClient);
    }

    public void FrameworkUpdate()
    {
        if (State.Value != AppManagerState.Started) return;

        scheduler.Update();
        processOscMessageQueue();
        ModuleManager.FrameworkUpdate();
    }

    #region OSC

    private void processOscMessageQueue()
    {
        while (oscMessageQueue.TryDequeue(out var message))
        {
            if (message.IsAvatarChangeEvent)
            {
                if (ProfileManager.AvatarChange((string)message.ParameterValue)) continue;

                ModuleManager.AvatarChange();
                continue;
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

        if (!initialiseOSCClient()) return;
        if (!VRChatOscClient.EnableSend() || !VRChatOscClient.EnableReceive()) return;

        State.Value = AppManagerState.Starting;

        await ModuleManager.StartAsync();

        State.Value = AppManagerState.Started;
    }

    private bool initialiseOSCClient()
    {
        try
        {
            var sendEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9000);
            var receiveEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9001);

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
        ModuleManager.Stop();
        oscMessageQueue.Clear();

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
