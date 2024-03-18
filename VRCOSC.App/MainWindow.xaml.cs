using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Newtonsoft.Json;
using VRCOSC.App.Actions;
using VRCOSC.App.Actions.Game;
using VRCOSC.App.Modules;
using VRCOSC.App.Packages;
using VRCOSC.App.Pages;
using VRCOSC.App.Pages.Modules;
using VRCOSC.App.Pages.Packages;
using VRCOSC.App.Pages.Profiles;
using VRCOSC.App.Pages.Run;
using VRCOSC.App.Pages.Settings;
using VRCOSC.App.Profiles;
using VRCOSC.App.SDK.OVR.Metadata;
using VRCOSC.App.Settings;
using VRCOSC.App.Utils;

namespace VRCOSC.App;

public partial class MainWindow
{
    public static MainWindow GetInstance() => (MainWindow)Application.Current.MainWindow;

    private readonly HomePage homePage;
    private readonly PackagePage packagePage;
    private readonly ModulesPage modulesPage;
    private readonly RunPage runPage;
    private readonly ProfilesPage profilesPage;
    private readonly SettingsPage settingsPage;

    private readonly Storage storage = AppManager.GetInstance().Storage;

    private static Version assemblyVersion => Assembly.GetEntryAssembly()?.GetName().Version ?? new Version();
    private string version => $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";

    public MainWindow()
    {
        InitializeComponent();

        DataContext = this;

        Title = $"{AppManager.AppName} {version}";

        AppManager.GetInstance().Initialise();
        SettingsManager.GetInstance().Load();

        copyOpenVrFiles();

        homePage = new HomePage();
        packagePage = new PackagePage();
        modulesPage = new ModulesPage();
        runPage = new RunPage();
        profilesPage = new ProfilesPage();
        settingsPage = new SettingsPage();

        setPageContents(homePage, HomeButton);

        load();
    }

    private async void load()
    {
        var loadingAction = new LoadGameAction();

        loadingAction.AddAction(new DynamicProgressAction("Loading profiles", () => ProfileManager.GetInstance().Load()));
        loadingAction.AddAction(PackageManager.GetInstance().Load());
        loadingAction.AddAction(new DynamicProgressAction("Loading modules", () => ModuleManager.GetInstance().LoadAllModules()));
        //loadingAction.AddAction(new DynamicProgressAction("Loading routes", () => appManager.RouterManager.Load()));

        loadingAction.OnComplete += () =>
        {
            Dispatcher.Invoke(() =>
            {
                fadeIn(MainWindowContent, 500);
                fadeOut(LoadingOverlay, 500);
            });
        };

        await ShowLoadingOverlay("Welcome to VRCOSC", loadingAction);
    }

    private void copyOpenVrFiles()
    {
        var runtimeOVRStorage = storage.GetStorageForDirectory("runtime/openvr");
        var runtimeOVRPath = runtimeOVRStorage.GetFullPath(string.Empty);

        var ovrFiles = Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(file => file.Contains("OpenVR"));

        foreach (var file in ovrFiles)
        {
            File.WriteAllBytes(Path.Combine(runtimeOVRPath, getOriginalFileName(file)), getResourceBytes(file));
        }

        var manifest = new OVRManifest();
        manifest.Applications[0].ActionManifestPath = runtimeOVRStorage.GetFullPath("action_manifest.json");
        manifest.Applications[0].ImagePath = runtimeOVRStorage.GetFullPath("SteamImage.png");

        File.WriteAllText(Path.Combine(runtimeOVRPath, "app.vrmanifest"), JsonConvert.SerializeObject(manifest));
    }

    private static string getOriginalFileName(string fullResourceName)
    {
        var parts = fullResourceName.Split('.');
        return parts[^2] + "." + parts[^1];
    }

    private static byte[] getResourceBytes(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();

        using var stream = assembly.GetManifestResourceStream(resourceName);

        if (stream == null)
        {
            throw new InvalidOperationException($"{resourceName} does not exist");
        }

        using var memoryStream = new MemoryStream();

        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
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

        if (appManager.State.Value is AppManagerState.Started)
        {
            e.Cancel = true;
            await appManager.StopAsync();
            Close();
        }

        if (appManager.State.Value is AppManagerState.Waiting)
        {
            appManager.CancelStartRequest();
        }
    }

    private void MainWindow_OnClosed(object? sender, EventArgs e)
    {
        foreach (Window window in Application.Current.Windows)
        {
            if (window != this) window.Close();
        }
    }

    public async Task ShowLoadingOverlay(string title, ProgressAction progressAction)
    {
        Dispatcher.Invoke(() => LoadingTitle.Text = title);

        progressAction.OnComplete += HideLoadingOverlay;

        _ = Task.Run(async () =>
        {
            while (!progressAction.IsComplete)
            {
                Dispatcher.Invoke(() =>
                {
                    LoadingDescription.Text = progressAction.Title;
                    ProgressBar.Value = progressAction.GetProgress();
                });
                await Task.Delay(TimeSpan.FromSeconds(1d / 10d));
            }

            Dispatcher.Invoke(() =>
            {
                LoadingDescription.Text = "Finished!";
                ProgressBar.Value = 1;
            });
        });

        fadeIn(LoadingOverlay, 150);

        await progressAction.Execute();
    }

    public void HideLoadingOverlay() => fadeOut(LoadingOverlay, 150);

    private void fadeIn(FrameworkElement grid, double fadeInTimeMilli) => Dispatcher.Invoke(() =>
    {
        grid.Visibility = Visibility.Visible;
        grid.Opacity = 0;

        var fadeInAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(fadeInTimeMilli)
        };

        Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(OpacityProperty));

        var storyboard = new Storyboard();
        storyboard.Children.Add(fadeInAnimation);
        storyboard.Begin(grid);
    });

    private void fadeOut(FrameworkElement grid, double fadeOutTime) => Dispatcher.Invoke(() =>
    {
        grid.Opacity = 1;

        var fadeOutAnimation = new DoubleAnimation
        {
            To = 0,
            Duration = TimeSpan.FromMilliseconds(fadeOutTime)
        };

        Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(OpacityProperty));

        var storyboard = new Storyboard();
        storyboard.Children.Add(fadeOutAnimation);
        storyboard.Completed += (_, _) => grid.Visibility = Visibility.Collapsed;
        storyboard.Begin(grid);
    });

    public ICommand HomeButtonClick => new RelayCommand(_ => setPageContents(homePage, HomeButton));
    public ICommand PackagesButtonClick => new RelayCommand(_ => setPageContents(packagePage, PackagesButton));
    public ICommand ModulesButtonClick => new RelayCommand(_ => setPageContents(modulesPage, ModulesButton));
    public ICommand RunButtonClick => new RelayCommand(_ => setPageContents(runPage, RunButton));
    public ICommand ProfilesButtonClick => new RelayCommand(_ => setPageContents(profilesPage, ProfilesButton));
    public ICommand SettingsButtonClick => new RelayCommand(_ => setPageContents(settingsPage, SettingsButton));

    private void setPageContents(object page, Button button)
    {
        HomeButton.Background = Brushes.Transparent;
        PackagesButton.Background = Brushes.Transparent;
        ModulesButton.Background = Brushes.Transparent;
        RunButton.Background = Brushes.Transparent;
        ProfilesButton.Background = Brushes.Transparent;
        SettingsButton.Background = Brushes.Transparent;

        ContentFrame.Content = page;
        button.Background = (Brush)FindResource("CBackground2");
    }
}
