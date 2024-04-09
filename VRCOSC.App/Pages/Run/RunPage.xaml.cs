// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Pages.Run;

public partial class RunPage
{
    private const int view_button_width = 160;

    private int chosenView;

    public int ChosenView
    {
        get => chosenView;
        set
        {
            chosenView = value;
            setChosenView();
        }
    }

    private readonly OSCView oscView;
    private readonly MiscView miscView;
    private readonly ModuleView moduleView;

    public RunPage()
    {
        InitializeComponent();

        DataContext = this;

        AppManager.GetInstance().State.Subscribe(onAppManagerStateChange, true);
        Logger.NewEntry += onLogEntry;

        oscView = new OSCView();
        miscView = new MiscView();
        moduleView = new ModuleView();

        setChosenView();
    }

    private void setChosenView()
    {
        ViewFrame.Content = ChosenView switch
        {
            0 => oscView,
            1 => miscView,
            2 => moduleView,
            _ => ViewFrame.Content
        };

        var moveAnimation = new DoubleAnimation(ChosenView * view_button_width, TimeSpan.FromSeconds(0.15f))
        {
            EasingFunction = new QuarticEase()
        };

        ViewSelector.RenderTransform.BeginAnimation(TranslateTransform.XProperty, moveAnimation);
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
        if (e.LoggerName != "terminal") return;

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

    public void ShowWaitingOverlay() => Dispatcher.Invoke(() => fadeIn(WaitingOverlay, 150));
    public void HideWaitingOverlay() => Dispatcher.Invoke(() => fadeOut(WaitingOverlay, 150));

    private static void fadeIn(FrameworkElement grid, double fadeInTimeMilli)
    {
        grid.Visibility = Visibility.Visible;
        grid.Opacity = 0;

        DoubleAnimation fadeInAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(fadeInTimeMilli)
        };

        Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(OpacityProperty));

        Storyboard storyboard = new Storyboard();
        storyboard.Children.Add(fadeInAnimation);
        storyboard.Begin(grid);
    }

    private static void fadeOut(FrameworkElement grid, double fadeOutTime)
    {
        grid.Opacity = 1;

        DoubleAnimation fadeOutAnimation = new DoubleAnimation
        {
            To = 0,
            Duration = TimeSpan.FromMilliseconds(fadeOutTime)
        };

        Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(OpacityProperty));

        Storyboard storyboard = new Storyboard();
        storyboard.Children.Add(fadeOutAnimation);
        storyboard.Completed += (_, _) => grid.Visibility = Visibility.Collapsed;
        storyboard.Begin(grid);
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        AppManager.GetInstance().CancelStartRequest();
    }

    private void ForceStartButton_OnClick(object sender, RoutedEventArgs e)
    {
        AppManager.GetInstance().ForceStart();
    }

    private void AvatarOSCViewButton_Click(object sender, RoutedEventArgs e)
    {
        ChosenView = 0;
    }

    private void MiscOSCViewButton_Click(object sender, RoutedEventArgs e)
    {
        ChosenView = 1;
    }

    private void ModuleViewButton_Click(object sender, RoutedEventArgs e)
    {
        ChosenView = 2;
    }
}
