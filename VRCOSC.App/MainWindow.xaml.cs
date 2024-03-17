using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using VRCOSC.App.Actions;
using VRCOSC.App.Actions.Game;
using VRCOSC.App.Modules;
using VRCOSC.App.Packages;
using VRCOSC.App.Profiles;

namespace VRCOSC.App;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();

        Title = "VRCOSC 2024.209.0";

        AppManager.GetInstance().Initialise();

        load();
    }

    private async void load()
    {
        var loadingAction = new LoadGameAction();

        loadingAction.AddAction(new DynamicProgressAction("Loading profiles", () => ProfileManager.GetInstance().Load()));
        loadingAction.AddAction(PackageManager.GetInstance().Load());
        loadingAction.AddAction(new DynamicProgressAction("Loading modules", () => ModuleManager.GetInstance().LoadAllModules()));
        //loadingAction.AddAction(new DynamicProgressAction("Loading routes", () => appManager.RouterManager.Load()));

        loadingAction.OnComplete += HideLoadingOverlay;
        ShowLoadingOverlay(loadingAction);
        await loadingAction.Execute();
    }

    private void MainWindow_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        var focusedElement = FocusManager.GetFocusedElement(this) as FrameworkElement;

        if (e.OriginalSource is not TextBox && focusedElement is TextBox)
        {
            Keyboard.ClearFocus();
        }
    }

    private void MainWindow_OnClosed(object? sender, EventArgs e)
    {
        foreach (Window window in Application.Current.Windows)
        {
            if (window != this) window.Close();
        }
    }

    public void ShowLoadingOverlay(ProgressAction progressAction) => Dispatcher.Invoke(() =>
    {
        _ = Task.Run(async () =>
        {
            while (!progressAction.IsComplete)
            {
                Dispatcher.Invoke(() => { ProgressBar.Value = progressAction.GetProgress(); });
                await Task.Delay(TimeSpan.FromSeconds(1d / 30d));
            }
        });

        fadeIn(LoadingOverlay, 150);
    });

    public void HideLoadingOverlay() => Dispatcher.Invoke(() => { fadeOut(LoadingOverlay, 150); });

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
}
