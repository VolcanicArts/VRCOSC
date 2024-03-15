// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Pages.Run;

public partial class RunPage
{
    public RunPage()
    {
        InitializeComponent();

        AppManager.GetInstance().State.Subscribe(onAppManagerStateChange, true);
        Logger.NewEntry += onLogEntry;
    }

    private void onAppManagerStateChange(AppManagerState newState) => Dispatcher.Invoke(() =>
    {
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
                break;

            case AppManagerState.Stopped:
                StartButton.IsEnabled = true;
                RestartButton.IsEnabled = false;
                StopButton.IsEnabled = false;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    });

    private void onLogEntry(LogEntry e) => Dispatcher.Invoke(() =>
    {
        if (e.LoggerName != "terminal") return;

        var dateTimeText = $"[{DateTime.Now:HH:mm:ss}] {e.Message}";

        LogStackPanel.Children.Add(new TextBlock
        {
            Text = dateTimeText,
            FontSize = 15,
            Foreground = (Brush)FindResource("CForeground3"),
            TextWrapping = TextWrapping.Wrap
        });

        if (LogStackPanel.Children.Count >= 50)
        {
            LogStackPanel.Children.RemoveAt(0);
        }

        LogScrollViewer.ScrollToBottom();
    });

    private void PlayButtonOnClick(object sender, RoutedEventArgs e)
    {
        AppManager.GetInstance().RequestStart();
    }

    private void StopButtonOnClick(object sender, RoutedEventArgs e)
    {
        AppManager.GetInstance().Stop();
    }

    private void RestartButtonOnClick(object sender, RoutedEventArgs e)
    {
        AppManager.GetInstance().Restart();
    }
}
