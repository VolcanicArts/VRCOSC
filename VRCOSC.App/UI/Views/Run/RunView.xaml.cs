// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VRCOSC.App.Settings;
using VRCOSC.App.UI.Core;
using VRCOSC.App.UI.Windows;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.Run;

public partial class RunView : INotifyPropertyChanged
{
    private const int view_button_width = 160;

    public int ChosenTab
    {
        get;
        set
        {
            field = value;
            setChosenView();
            OnPropertyChanged();
        }
    }

    public Observable<bool> AutoStartQuestionClicked => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCMetadata.AutoStartQuestionClicked);

    public RunView()
    {
        InitializeComponent();

        DataContext = this;

        AppManager.GetInstance().State.Subscribe(onAppManagerStateChange, true);
        Logger.NewEntry += onLogEntry;

        setChosenView();
    }

    private void setChosenView()
    {
        switch (ChosenTab)
        {
            case 0:
                RuntimeView.Visibility = Visibility.Visible;
                ParameterView.Visibility = Visibility.Collapsed;
                ChatBoxView.Visibility = Visibility.Collapsed;
                break;

            case 1:
                RuntimeView.Visibility = Visibility.Collapsed;
                ParameterView.Visibility = Visibility.Visible;
                ChatBoxView.Visibility = Visibility.Collapsed;
                break;

            case 2:
                RuntimeView.Visibility = Visibility.Collapsed;
                ParameterView.Visibility = Visibility.Collapsed;
                ChatBoxView.Visibility = Visibility.Visible;
                break;
        }

        var moveAnimation = new DoubleAnimation(ChosenTab * view_button_width, TimeSpan.FromSeconds(0.15f))
        {
            EasingFunction = new QuarticEase()
        };

        ViewSelector.RenderTransform.BeginAnimation(TranslateTransform.XProperty, moveAnimation);
    }

    private void onAppManagerStateChange(AppManagerState newState) => Dispatcher.Invoke(() =>
    {
        EndpointText.Visibility = Visibility.Collapsed;

        var cM = AppManager.GetInstance().ConnectionManager;
        var oC = AppManager.GetInstance().VRChatOscClient;
        var oscMode = SettingsManager.GetInstance().GetValue<ConnectionMode>(VRCOSCSetting.ConnectionMode);

        if (newState == AppManagerState.Starting) LogStackPanel.Children.Clear();

        switch (newState)
        {
            case AppManagerState.Waiting:
            case AppManagerState.Starting:
            case AppManagerState.Stopping:
                StartButton.IsEnabled = false;
                RestartButton.IsEnabled = false;
                StopButton.IsEnabled = false;
                break;

            case AppManagerState.Started:
                StartButton.IsEnabled = false;
                RestartButton.IsEnabled = true;
                StopButton.IsEnabled = true;
                EndpointText.Visibility = Visibility.Visible;

                EndpointText.Text = oscMode switch
                {
                    ConnectionMode.Local when cM.VRChatQueryPort is not null => $"Outgoing: {cM.VRChatIP}:{cM.VRChatReceivePort} ({cM.VRChatQueryPort}) | Incoming: {cM.VRCOSCIP}:{cM.VRCOSCReceivePort} ({cM.VRCOSCQueryPort})",
                    ConnectionMode.LAN when cM.VRCOSCQueryPort is not null => $"Outgoing: {cM.VRChatIP}:{cM.VRChatReceivePort} | Incoming: {cM.VRCOSCIP}:{cM.VRCOSCReceivePort} ({cM.VRCOSCQueryPort})",
                    ConnectionMode.Custom => $"Outgoing: {oC.SendEndpoint} | Incoming: {oC.ReceiveEndpoint}",
                    _ => $"Outgoing: {oC.SendEndpoint} | Incoming: {oC.ReceiveEndpoint}"
                };

                break;

            case AppManagerState.Stopped:
                StartButton.IsEnabled = true;
                RestartButton.IsEnabled = false;
                StopButton.IsEnabled = false;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        switch (newState)
        {
            case AppManagerState.Waiting:
                ShowWaitingOverlay();
                break;

            case AppManagerState.Starting:
            case AppManagerState.Stopping:
            case AppManagerState.Started:
            case AppManagerState.Stopped:
                HideWaitingOverlay();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    });

    private void onLogEntry(LogEntry e) => Dispatcher.Invoke(() =>
    {
        if (e.Target != LoggingTarget.Terminal || AppManager.GetInstance().State.Value == AppManagerState.Stopped || AppManager.GetInstance().State.Value == AppManagerState.Waiting) return;

        var dateTimeText = $"[{DateTime.Now:HH:mm:ss}] {e.Message}";

        LogStackPanel.Children.Add(new TextBlock
        {
            Text = dateTimeText,
            FontSize = 14,
            Foreground = (Brush)FindResource("CForeground3"),
            TextWrapping = TextWrapping.Wrap
        });

        while (LogStackPanel.Children.Count > 100)
        {
            LogStackPanel.Children.RemoveAt(0);
        }

        LogScrollViewer.ScrollToBottom();
    });

    private void PlayButtonOnClick(object sender, RoutedEventArgs e)
    {
        AppManager.GetInstance().RequestStart().Forget();
    }

    private void StopButtonOnClick(object sender, RoutedEventArgs e)
    {
        _ = AppManager.GetInstance().StopAsync();
    }

    private void RestartButtonOnClick(object sender, RoutedEventArgs e)
    {
        _ = AppManager.GetInstance().RestartAsync();
    }

    public void ShowWaitingOverlay() => Dispatcher.Invoke(() =>
    {
        WaitingOverlay.FadeInFromZero(150);
        CancelButton.IsEnabled = true;
        ForceStartButton.IsEnabled = true;
    });

    public void HideWaitingOverlay() => Dispatcher.Invoke(() => WaitingOverlay.FadeOutFromOne(150));

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        CancelButton.IsEnabled = false;
        AppManager.GetInstance().CancelStartRequest();
    }

    private void ForceStartButton_OnClick(object sender, RoutedEventArgs e)
    {
        ForceStartButton.IsEnabled = false;
        AppManager.GetInstance().ForceStart().Forget();
    }

    private void AvatarParameterViewButton_Click(object sender, RoutedEventArgs e)
    {
        ChosenTab = 1;
    }

    private void ChatBoxViewButton_Click(object sender, RoutedEventArgs e)
    {
        ChosenTab = 2;
    }

    private void RuntimeViewButton_Click(object sender, RoutedEventArgs e)
    {
        ChosenTab = 0;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void AutoStartQuestion_OnClick(object sender, RoutedEventArgs e)
    {
        SettingsManager.GetInstance().GetObservable<bool>(VRCOSCMetadata.AutoStartQuestionClicked).Value = true;
        MainWindow.GetInstance().FocusAppSettings();
        MainWindow.GetInstance().AppSettingsView.FocusBehaviourTab();
    }
}