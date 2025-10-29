// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Threading;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.Run.Tabs;

public partial class AvatarParameterTabView
{
    public ObservableDictionary<string, object> OutgoingMessages { get; } = new();
    public ObservableDictionary<string, object> IncomingMessages { get; } = new();

    private readonly Dictionary<string, object> outgoingLocal = new();
    private readonly Dictionary<string, object> incomingLocal = new();

    private readonly object incomingLock = new();
    private readonly object outgoingLock = new();

    private readonly DispatcherTimer timer;

    public AvatarParameterTabView()
    {
        InitializeComponent();

        AppManager.GetInstance().VRChatOscClient.OnVRChatOSCMessageSent += onVRChatOSCMessageSent;
        AppManager.GetInstance().VRChatOscClient.OnVRChatOSCMessageReceived += onVRChatOSCMessageReceived;
        AppManager.GetInstance().State.Subscribe(OnAppManagerStateChange);

        timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        timer.Tick += update;
        timer.Start();

        DataContext = this;
    }

    private void update(object? sender, EventArgs e)
    {
        lock (outgoingLock)
        {
            foreach (var pair in outgoingLocal)
            {
                OutgoingMessages[pair.Key] = pair.Value;
            }

            outgoingLocal.Clear();
        }

        lock (incomingLock)
        {
            foreach (var pair in incomingLocal)
            {
                IncomingMessages[pair.Key] = pair.Value;
            }

            incomingLocal.Clear();
        }
    }

    private void onVRChatOSCMessageSent(VRChatOSCMessage e)
    {
        if (!e.IsAvatarParameter) return;

        Dispatcher.Invoke(() =>
        {
            lock (outgoingLock)
            {
                return outgoingLocal[e.ParameterName] = e.ParameterValue;
            }
        });
    }

    private Task onVRChatOSCMessageReceived(VRChatOSCMessage e)
    {
        if (!e.IsAvatarParameter) return Task.CompletedTask;

        Dispatcher.Invoke(() =>
        {
            lock (incomingLock)
            {
                return incomingLocal[e.ParameterName] = e.ParameterValue;
            }
        });

        return Task.CompletedTask;
    }

    private void OnAppManagerStateChange(AppManagerState newState) => Dispatcher.Invoke(() =>
    {
        if (newState == AppManagerState.Starting)
        {
            OutgoingMessages.Clear();
            IncomingMessages.Clear();
        }
    });
}