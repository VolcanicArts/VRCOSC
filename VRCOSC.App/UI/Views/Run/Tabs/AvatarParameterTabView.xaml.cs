// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.Run.Tabs;

public partial class AvatarParameterTabView
{
    public ObservableDictionary<string, object> OutgoingMessagesStore { get; } = [];
    public ObservableDictionary<string, object> IncomingMessagesStore { get; } = [];

    public ObservableDictionary<string, object> OutgoingMessages { get; } = [];
    public ObservableDictionary<string, object> IncomingMessages { get; } = [];

    private readonly Dictionary<string, object> outgoingLocal = new();
    private readonly Dictionary<string, object> incomingLocal = new();

    private readonly Lock incomingLock = new();
    private readonly Lock outgoingLock = new();

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
                OutgoingMessagesStore[pair.Key] = pair.Value;
            }

            outgoingLocal.Clear();
            OutgoingMessages.Clear();

            foreach (var pair in OutgoingMessagesStore)
            {
                if (string.IsNullOrWhiteSpace(SearchTextBox.Text) || pair.Key.Contains(SearchTextBox.Text, StringComparison.InvariantCultureIgnoreCase))
                {
                    OutgoingMessages[pair.Key] = pair.Value;
                }
            }
        }

        lock (incomingLock)
        {
            foreach (var pair in incomingLocal)
            {
                IncomingMessagesStore[pair.Key] = pair.Value;
            }

            incomingLocal.Clear();
            IncomingMessages.Clear();

            foreach (var pair in IncomingMessagesStore)
            {
                if (string.IsNullOrWhiteSpace(SearchTextBox.Text) || pair.Key.Contains(SearchTextBox.Text, StringComparison.InvariantCultureIgnoreCase))
                {
                    IncomingMessages[pair.Key] = pair.Value;
                }
            }
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
            OutgoingMessagesStore.Clear();
            IncomingMessagesStore.Clear();
        }
    });
}