// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Pages.Run.Tabs;

public partial class AvatarParameterView : INotifyPropertyChanged
{
    public ObservableDictionary<string, object> OutgoingMessages { get; } = new();
    public ObservableDictionary<string, object> IncomingMessages { get; } = new();

    private double outgoingScrollViewerHeight;

    public double OutgoingScrollViewerHeight
    {
        get => outgoingScrollViewerHeight;
        set
        {
            outgoingScrollViewerHeight = value;
            OnPropertyChanged();
        }
    }

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

    public AvatarParameterView()
    {
        InitializeComponent();

        SizeChanged += OnSizeChanged;

        AppManager.GetInstance().VRChatOscClient.OnParameterSent += OnParameterSent;
        AppManager.GetInstance().VRChatOscClient.OnParameterReceived += OnParameterReceived;
        AppManager.GetInstance().State.Subscribe(OnAppManagerStateChange);

        DataContext = this;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        evaluateOutgoingContentHeight();
        evaluateIncomingContentHeight();
    }

    private void evaluateOutgoingContentHeight()
    {
        if (OutgoingMessages.Count == 0)
        {
            OutgoingScrollViewerHeight = 0;
            return;
        }

        var contentHeight = OutgoingMessagesList.ActualHeight;
        var targetHeight = OutgoingGridContainer.ActualHeight - 40;
        OutgoingScrollViewerHeight = contentHeight >= targetHeight ? targetHeight : double.NaN;
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
            OutgoingMessages[e.ParameterName] = e.ParameterValue;
            evaluateOutgoingContentHeight();
        });
    }

    private void OnParameterReceived(VRChatOscMessage e)
    {
        if (!e.IsAvatarParameter) return;

        Dispatcher.Invoke(() =>
        {
            IncomingMessages[e.ParameterName] = e.ParameterValue;
            evaluateIncomingContentHeight();
        });
    }

    private void OnAppManagerStateChange(AppManagerState newState) => Dispatcher.Invoke(() =>
    {
        if (newState == AppManagerState.Starting)
        {
            OutgoingMessages.Clear();
            IncomingMessages.Clear();
        }

        evaluateOutgoingContentHeight();
        evaluateIncomingContentHeight();
    });

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
