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
using Newtonsoft.Json;
using VRCOSC.App.Actions;
using VRCOSC.App.Actions.Game;
using VRCOSC.App.ChatBox;
using VRCOSC.App.Modules;
using VRCOSC.App.Packages;
using VRCOSC.App.Pages;
using VRCOSC.App.Pages.AppSettings;
using VRCOSC.App.Pages.ChatBox;
using VRCOSC.App.Pages.Modules;
using VRCOSC.App.Pages.Packages;
using VRCOSC.App.Pages.Profiles;
using VRCOSC.App.Pages.Run;
using VRCOSC.App.Pages.Settings;
using VRCOSC.App.Profiles;
using VRCOSC.App.SDK.OVR.Metadata;
using VRCOSC.App.Settings;
using VRCOSC.App.UI;
using VRCOSC.App.Utils;

namespace VRCOSC.App;

public partial class MainWindow
{
    public static MainWindow GetInstance() => (MainWindow)Application.Current.MainWindow;

    public readonly HomePage HomePage;
    public readonly PackagePage PackagePage;
    public readonly ModulesPage ModulesPage;
    public readonly SettingsPage SettingsPage;
    public readonly ChatBoxPage ChatBoxPage;
    public readonly RunPage RunPage;
    public readonly ProfilesPage ProfilesPage;
    public readonly AppSettingsPage AppSettingsPage;

    private readonly Storage storage = AppManager.GetInstance().Storage;

    private static Version assemblyVersion => Assembly.GetEntryAssembly()?.GetName().Version ?? new Version();
    private string version => $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";

    public MainWindow()
    {
        SettingsManager.GetInstance().Load();
        AppManager.GetInstance().Initialise();

        InitializeComponent();

        DataContext = this;

        Title = $"{AppManager.APP_NAME} {version}";

        copyOpenVrFiles();

        HomePage = new HomePage();
        PackagePage = new PackagePage();
        ModulesPage = new ModulesPage();
        SettingsPage = new SettingsPage();
        ChatBoxPage = new ChatBoxPage();
        RunPage = new RunPage();
        ProfilesPage = new ProfilesPage();
        AppSettingsPage = new AppSettingsPage();

        setPageContents(PackagePage, PackagesButton);

        load();
    }

    private async void load()
    {
        var loadingAction = new LoadGameAction();

        loadingAction.AddAction(new DynamicProgressAction("Loading profiles", () => ProfileManager.GetInstance().Load()));
        loadingAction.AddAction(PackageManager.GetInstance().Load());
        loadingAction.AddAction(new DynamicProgressAction("Loading modules", () => ModuleManager.GetInstance().LoadAllModules()));
        loadingAction.AddAction(new DynamicProgressAction("Loading chatbox", () => ChatBoxManager.GetInstance().Load()));
        //loadingAction.AddAction(new DynamicProgressAction("Loading routes", () => appManager.RouterManager.Load()));

        loadingAction.OnComplete += () =>
        {
            Dispatcher.Invoke(() =>
            {
                MainWindowContent.FadeInFromZero(500);
                LoadingOverlay.FadeOutFromOne(500);
                AppManager.GetInstance().InitialLoadComplete();
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

    public async Task ShowLoadingOverlay(string title, ProgressAction progressAction) => await Dispatcher.Invoke(async () =>
    {
        LoadingTitle.Text = title;

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

        LoadingOverlay.FadeIn(150);

        await progressAction.Execute();
    });

    public void HideLoadingOverlay() => Dispatcher.Invoke(() => LoadingOverlay.FadeOut(150));

    private void setPageContents(object page, Button button)
    {
        HomeButton.Background = Brushes.Transparent;
        PackagesButton.Background = Brushes.Transparent;
        ModulesButton.Background = Brushes.Transparent;
        SettingsButton.Background = Brushes.Transparent;
        ChatBoxButton.Background = Brushes.Transparent;
        RunButton.Background = Brushes.Transparent;
        ProfilesButton.Background = Brushes.Transparent;
        AppSettingsButton.Background = Brushes.Transparent;

        ContentFrame.Content = page;
        button.Background = (Brush)FindResource("CBackground2");
    }

    private void HomeButton_OnClick(object sender, RoutedEventArgs e)
    {
        setPageContents(HomePage, HomeButton);
    }

    private void PackagesButton_OnClick(object sender, RoutedEventArgs e)
    {
        setPageContents(PackagePage, PackagesButton);
    }

    private void ModulesButton_OnClick(object sender, RoutedEventArgs e)
    {
        setPageContents(ModulesPage, ModulesButton);
    }

    private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        setPageContents(SettingsPage, SettingsButton);
    }

    private void ChatBoxButton_OnClick(object sender, RoutedEventArgs e)
    {
        setPageContents(ChatBoxPage, ChatBoxButton);
    }

    private void RunButton_OnClick(object sender, RoutedEventArgs e)
    {
        setPageContents(RunPage, RunButton);
    }

    private void ProfilesButton_OnClick(object sender, RoutedEventArgs e)
    {
        setPageContents(ProfilesPage, ProfilesButton);
    }

    private void AppSettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        setPageContents(AppSettingsPage, AppSettingsButton);
    }
}
