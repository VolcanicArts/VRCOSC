using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VRCOSC.App.Actions;
using VRCOSC.App.Actions.Game;
using VRCOSC.App.Modules;
using VRCOSC.App.Packages;
using VRCOSC.App.Pages;
using VRCOSC.App.Pages.Modules;
using VRCOSC.App.Pages.Packages;
using VRCOSC.App.Pages.Profiles;
using VRCOSC.App.Pages.Run;
using VRCOSC.App.Profiles;

namespace VRCOSC.App;

public partial class MainWindow
{
    private readonly HomePage homePage;
    private readonly PackagePage packagePage;
    private readonly ModulesPage modulesPage;
    private readonly RunPage runPage;
    private readonly ProfilesPage profilesPage;

    public MainWindow()
    {
        InitializeComponent();

        DataContext = this;

        Title = "VRCOSC 2024.209.0";

        homePage = new HomePage();
        packagePage = new PackagePage();
        modulesPage = new ModulesPage();
        runPage = new RunPage();
        profilesPage = new ProfilesPage();

        setPageContents(homePage, HomeButton);

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

    private async void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        var appManager = AppManager.GetInstance();

        if (appManager.State.Value is AppManagerState.Started or AppManagerState.Waiting)
        {
            e.Cancel = true;
            await appManager.StopAsync();
            Close();
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

    public ICommand HomeButtonClick => new RelayCommand(_ => setPageContents(homePage, HomeButton));
    public ICommand PackagesButtonClick => new RelayCommand(_ => setPageContents(packagePage, PackagesButton));
    public ICommand ModulesButtonClick => new RelayCommand(_ => setPageContents(modulesPage, ModulesButton));
    public ICommand RunButtonClick => new RelayCommand(_ => setPageContents(runPage, RunButton));
    public ICommand ProfilesButtonClick => new RelayCommand(_ => setPageContents(profilesPage, ProfilesButton));

    private void setPageContents(object page, Button button)
    {
        HomeButton.Background = Brushes.Transparent;
        PackagesButton.Background = Brushes.Transparent;
        ModulesButton.Background = Brushes.Transparent;
        RunButton.Background = Brushes.Transparent;
        ProfilesButton.Background = Brushes.Transparent;

        ContentFrame.Content = page;
        button.Background = (Brush)FindResource("CBackground2");
    }
}
