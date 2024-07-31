// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.Run.Tabs;

public partial class AvatarParameterView : INotifyPropertyChanged
{
    public ObservableDictionary<string, object> OutgoingMessages { get; } = new();
    public ObservableDictionary<string, object> IncomingMessages { get; } = new();

    private readonly Dictionary<string, object> outgoingLocal = new();
    private readonly Dictionary<string, object> incomingLocal = new();

    private readonly object incomingLock = new();
    private readonly object outgoingLock = new();

    private double incomingScrollViewerHeight;

    public double IncomingScrollViewerHeight
    {
        get => incomingScrollViewerHeight;
        set
        {
            incomingScrollViewerHeight = value;
            OnPropertyChanged();
        }
    }

    private readonly DispatcherTimer timer;

    public AvatarParameterView()
    {
        InitializeComponent();

        SizeChanged += OnSizeChanged;

        AppManager.GetInstance().VRChatOscClient.OnParameterSent += OnParameterSent;
        AppManager.GetInstance().VRChatOscClient.OnParameterReceived += OnParameterReceived;
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

        evaluateIncomingContentHeight();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        evaluateIncomingContentHeight();
    }

    private void evaluateIncomingContentHeight()
    {
        if (IncomingMessages.Count == 0)
        {
            IncomingScrollViewerHeight = 0;
            return;
        }

        var contentHeight = IncomingMessagesList.ActualHeight;
        var targetHeight = IncomingGridContainer.ActualHeight - 40;
        IncomingScrollViewerHeight = contentHeight >= targetHeight ? targetHeight : double.NaN;
    }

    private void OnParameterSent(VRChatOscMessage e)
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

    private void OnParameterReceived(VRChatOscMessage e)
    {
        if (!e.IsAvatarParameter) return;

        Dispatcher.Invoke(() =>
        {
            lock (incomingLock)
            {
                return incomingLocal[e.ParameterName] = e.ParameterValue;
            }
        });
    }

    private void OnAppManagerStateChange(AppManagerState newState) => Dispatcher.Invoke(() =>
    {
        if (newState == AppManagerState.Starting)
        {
            OutgoingMessages.Clear();
            IncomingMessages.Clear();
        }

        evaluateIncomingContentHeight();
    });

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
