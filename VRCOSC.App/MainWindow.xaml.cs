using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using Newtonsoft.Json;
using PInvoke;
using VRCOSC.App.Actions;
using VRCOSC.App.Actions.Game;
using VRCOSC.App.ChatBox;
using VRCOSC.App.Modules;
using VRCOSC.App.Packages;
using VRCOSC.App.Pages;
using VRCOSC.App.Pages.AppDebug;
using VRCOSC.App.Pages.AppSettings;
using VRCOSC.App.Pages.ChatBox;
using VRCOSC.App.Pages.Modules;
using VRCOSC.App.Pages.Packages;
using VRCOSC.App.Pages.Profiles;
using VRCOSC.App.Pages.Router;
using VRCOSC.App.Pages.Run;
using VRCOSC.App.Pages.Settings;
using VRCOSC.App.Profiles;
using VRCOSC.App.Router;
using VRCOSC.App.SDK.OVR.Metadata;
using VRCOSC.App.Settings;
using VRCOSC.App.UI;
using VRCOSC.App.Utils;
using Application = System.Windows.Application;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;
using TextBox = System.Windows.Controls.TextBox;

namespace VRCOSC.App;

public partial class MainWindow
{
    public static MainWindow GetInstance() => (MainWindow)Application.Current.MainWindow;

    public readonly HomePage HomePage;
    public readonly PackagePage PackagePage;
    public readonly ModulesPage ModulesPage;
    public readonly RouterPage RouterPage;
    public readonly SettingsPage SettingsPage;
    public readonly ChatBoxPage ChatBoxPage;
    public readonly RunPage RunPage;
    public readonly AppDebugPage DebugPage;
    public readonly ProfilesPage ProfilesPage;
    public readonly AppSettingsPage AppSettingsPage;

    private readonly Storage storage = AppManager.GetInstance().Storage;

    public Observable<bool> ShowAppDebug { get; } = new();

    public MainWindow()
    {
        SettingsManager.GetInstance().Load();
        SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.EnableAppDebug).Subscribe(newValue => ShowAppDebug.Value = newValue, true);

        AppManager.GetInstance().Initialise();

        InitializeComponent();
        DataContext = this;

        Title = $"{AppManager.APP_NAME} {AppManager.GetInstance().Version}";

        setupTrayIcon();
        copyOpenVrFiles();

        HomePage = new HomePage();
        PackagePage = new PackagePage();
        ModulesPage = new ModulesPage();
        RouterPage = new RouterPage();
        SettingsPage = new SettingsPage();
        ChatBoxPage = new ChatBoxPage();
        RunPage = new RunPage();
        DebugPage = new AppDebugPage();
        ProfilesPage = new ProfilesPage();
        AppSettingsPage = new AppSettingsPage();

        setPageContents(PackagePage, PackagesButton);

        load();
    }

    private async void load()
    {
        var loadingAction = new LoadGameAction();

        loadingAction.AddAction(new DynamicAsyncProgressAction("Checking for updates", () => AppManager.GetInstance().VelopackUpdater.CheckForUpdatesAsync()));
        loadingAction.AddAction(new DynamicProgressAction("Loading profiles", () => ProfileManager.GetInstance().Load()));
        loadingAction.AddAction(PackageManager.GetInstance().Load());
        loadingAction.AddAction(new DynamicProgressAction("Loading modules", () => ModuleManager.GetInstance().LoadAllModules()));
        loadingAction.AddAction(new DynamicProgressAction("Loading chatbox", () => ChatBoxManager.GetInstance().Load()));
        loadingAction.AddAction(new DynamicProgressAction("Loading routes", () => RouterManager.GetInstance().Load()));

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
        if (SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.TrayOnClose))
        {
            e.Cancel = true;
            inTray = true;
            handleTrayTransition();
            return;
        }

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
        trayIcon.Dispose();

        foreach (Window window in Application.Current.Windows)
        {
            if (window != this) window.Close();
        }
    }

    #region Tray

    private bool inTray;
    private readonly NotifyIcon trayIcon = new();

    private void setupTrayIcon()
    {
        trayIcon.DoubleClick += (_, _) =>
        {
            inTray = !inTray;
            handleTrayTransition();
        };

        trayIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly()!.Location)!;
        trayIcon.Visible = true;
        trayIcon.Text = AppManager.APP_NAME;

        var contextMenu = new ContextMenuStrip
        {
            Items =
            {
                {
                    AppManager.APP_NAME, null, (_, _) =>
                    {
                        inTray = false;
                        handleTrayTransition();
                    }
                },
                new ToolStripSeparator(),
                {
                    "Exit", null, (_, _) => Dispatcher.Invoke(() => Application.Current.Shutdown())
                }
            }
        };

        trayIcon.ContextMenuStrip = contextMenu;
    }

    private void handleTrayTransition()
    {
        if (inTray)
        {
            foreach (Window window in Application.Current.Windows)
            {
                User32.ShowWindow(new WindowInteropHelper(window).Handle, User32.WindowShowStyle.SW_HIDE);
            }
        }
        else
        {
            foreach (Window window in Application.Current.Windows)
            {
                User32.ShowWindow(new WindowInteropHelper(window).Handle, User32.WindowShowStyle.SW_SHOWDEFAULT);
                window.Show();
                window.Activate();
            }
        }
    }

    #endregion

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
        RouterButton.Background = Brushes.Transparent;
        SettingsButton.Background = Brushes.Transparent;
        ChatBoxButton.Background = Brushes.Transparent;
        RunButton.Background = Brushes.Transparent;
        DebugButton.Background = Brushes.Transparent;
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

    private void RouterButton_OnClick(object sender, RoutedEventArgs e)
    {
        setPageContents(RouterPage, RouterButton);
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

    private void DebugButton_OnClick(object sender, RoutedEventArgs e)
    {
        setPageContents(DebugPage, DebugButton);
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
