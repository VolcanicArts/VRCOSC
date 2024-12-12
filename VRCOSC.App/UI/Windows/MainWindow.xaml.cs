// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Newtonsoft.Json;
using PInvoke;
using Semver;
using VRCOSC.App.Actions;
using VRCOSC.App.ChatBox;
using VRCOSC.App.Modules;
using VRCOSC.App.OVR;
using VRCOSC.App.Packages;
using VRCOSC.App.Profiles;
using VRCOSC.App.Router;
using VRCOSC.App.SDK.OVR.Metadata;
using VRCOSC.App.Settings;
using VRCOSC.App.Startup;
using VRCOSC.App.UI.Core;
using VRCOSC.App.UI.Views.AppDebug;
using VRCOSC.App.UI.Views.AppSettings;
using VRCOSC.App.UI.Views.ChatBox;
using VRCOSC.App.UI.Views.Information;
using VRCOSC.App.UI.Views.Modules;
using VRCOSC.App.UI.Views.Packages;
using VRCOSC.App.UI.Views.Profiles;
using VRCOSC.App.UI.Views.Router;
using VRCOSC.App.UI.Views.Run;
using VRCOSC.App.UI.Views.Startup;
using VRCOSC.App.Updater;
using VRCOSC.App.Utils;
using Application = System.Windows.Application;
using ButtonBase = System.Windows.Controls.Primitives.ButtonBase;

#if !DEBUG
using Velopack.Locators;
#endif

namespace VRCOSC.App.UI.Windows;

public partial class MainWindow
{
    public static MainWindow GetInstance() => (MainWindow)Application.Current.MainWindow;

    public PackagesView PackagesView = null!;
    public ModulesView ModulesView = null!;
    public RouterView RouterView = null!;
    public ChatBoxView ChatBoxView = null!;
    public StartupView StartupView = null!;
    public RunView RunView = null!;
    public AppDebugView AppDebugView = null!;
    public ProfilesView ProfilesView = null!;
    public AppSettingsView AppSettingsView = null!;
    public InformationView InformationView = null!;

    private readonly Storage storage = AppManager.GetInstance().Storage;
    private VelopackUpdater velopackUpdater = null!;

    public Observable<bool> ShowAppDebug { get; } = new();
    public Observable<bool> ShowRouter { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        backupV1Files();
        setupTrayIcon();
        copyOpenVrFiles();
        startApp();
    }

    private async void startApp()
    {
        SettingsManager.GetInstance().Load();

        velopackUpdater = new VelopackUpdater();

        if (!velopackUpdater.IsInstalled())
        {
            Logger.Log("Portable app detected. Cancelling update check");
        }
        else
        {
            await velopackUpdater.CheckForUpdatesAsync();

            if (velopackUpdater.IsUpdateAvailable())
            {
                await velopackUpdater.ExecuteUpdate();
                return;
            }

            Logger.Log("No updates. Proceeding with loading");
        }

        AppManager.GetInstance().Initialise();

        var installedUpdateChannel = SettingsManager.GetInstance().GetValue<UpdateChannel>(VRCOSCMetadata.InstalledUpdateChannel);
        Title = installedUpdateChannel == UpdateChannel.Beta ? $"{AppManager.APP_NAME} {AppManager.Version} BETA" : $"{AppManager.APP_NAME} {AppManager.Version}";

        PackagesView = new PackagesView();
        ModulesView = new ModulesView();
        RouterView = new RouterView();
        ChatBoxView = new ChatBoxView();
        StartupView = new StartupView();
        RunView = new RunView();
        AppDebugView = new AppDebugView();
        ProfilesView = new ProfilesView();
        AppSettingsView = new AppSettingsView();
        InformationView = new InformationView();

        setContent(ModulesView);

        SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.EnableAppDebug).Subscribe(newValue => ShowAppDebug.Value = newValue, true);
        SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.EnableRouter).Subscribe(newValue => ShowRouter.Value = newValue, true);

        await load();
    }

    private void backupV1Files()
    {
        try
        {
            if (!storage.Exists("framework.ini")) return;

            var sourceDir = storage.GetFullPath(string.Empty);
            var destinationDir = storage.GetStorageForDirectory("v1-backup").GetFullPath(string.Empty);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var fileName = Path.GetFileName(file);
                var destFile = Path.Combine(destinationDir, fileName);
                File.Move(file, destFile, false);
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var dirName = Path.GetFileName(dir);
                if (dirName == "v1-backup") continue;

                var destDir = Path.Combine(destinationDir, dirName);
                Directory.Move(dir, destDir);
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Could not backup V1 files");
        }
    }

    private async Task load()
    {
        var loadingAction = new CompositeProgressAction();
        loadingAction.AddAction(PackageManager.GetInstance().Load());

        var installedVersion = SettingsManager.GetInstance().GetValue<string>(VRCOSCMetadata.InstalledVersion);

        if (!string.IsNullOrEmpty(installedVersion))
        {
            var installedVersionParsed = SemVersion.Parse(installedVersion, SemVersionStyles.Any);
            var currentVersion = SemVersion.Parse(AppManager.Version, SemVersionStyles.Any);

            // refresh packages if we've upgraded or downgraded version
            if (SemVersion.ComparePrecedence(installedVersionParsed, currentVersion) != 0)
            {
                loadingAction.AddAction(PackageManager.GetInstance().RefreshAllSources(true));
            }
        }

        SettingsManager.GetInstance().GetObservable<string>(VRCOSCMetadata.InstalledVersion).Value = AppManager.Version;

        if (SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.AutoUpdatePackages))
        {
            loadingAction.AddAction(new DynamicChildProgressAction(() => PackageManager.GetInstance().UpdateAllInstalledPackages()));
        }

        loadingAction.AddAction(new DynamicProgressAction("Loading Managers", () =>
        {
            ProfileManager.GetInstance().Load();
            ModuleManager.GetInstance().LoadAllModules();
            ChatBoxManager.GetInstance().Load();
            RouterManager.GetInstance().Load();
            StartupManager.GetInstance().Load();
        }));

        if (!SettingsManager.GetInstance().GetValue<bool>(VRCOSCMetadata.FirstTimeSetupComplete))
        {
            loadingAction.AddAction(new DynamicChildProgressAction(() => PackageManager.GetInstance().InstallPackage(PackageManager.GetInstance().OfficialModulesSource)));
        }

        loadingAction.OnComplete += () => Dispatcher.Invoke(() =>
        {
            if (!SettingsManager.GetInstance().GetValue<bool>(VRCOSCMetadata.FirstTimeSetupComplete))
            {
                new FirstTimeInstallWindow().Show();
                SettingsManager.GetInstance().GetObservable<bool>(VRCOSCMetadata.FirstTimeSetupComplete).Value = true;
            }

            MainWindowContent.FadeInFromZero(500);
            AppManager.GetInstance().InitialLoadComplete();
        });

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
#if DEBUG
        manifest.Applications[0].BinaryPathWindows = Environment.ProcessPath!;
#else
        manifest.Applications[0].BinaryPathWindows = Path.Join(VelopackLocator.GetDefault(null).RootAppDir, "current", "VRCOSC.exe");
#endif
        manifest.Applications[0].ActionManifestPath = runtimeOVRStorage.GetFullPath("action_manifest.json");
        manifest.Applications[0].ImagePath = runtimeOVRStorage.GetFullPath("SteamImage.png");

        File.WriteAllText(Path.Join(runtimeOVRPath, "app.vrmanifest"), JsonConvert.SerializeObject(manifest, Formatting.Indented));
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

        OVRDeviceManager.GetInstance().Serialise();
        RouterManager.GetInstance().Serialise();
        StartupManager.GetInstance().Serialise();
        SettingsManager.GetInstance().Serialise();
    }

    private void MainWindow_OnClosed(object? sender, EventArgs e)
    {
        trayIcon.Dispose();

        foreach (Window window in Application.Current.Windows)
        {
            if (window != Application.Current.MainWindow) window.Close();
        }
    }

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        this.SetPosition(null, ScreenChoice.Primary, HorizontalPosition.Center, VerticalPosition.Center);

        if (SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.StartInTray))
        {
            await Task.Delay(100); // just in case
            inTray = true;
            handleTrayTransition();
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
        try
        {
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow is null) throw new InvalidOperationException("Main window is null");

            if (inTray)
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window == mainWindow)
                    {
                        User32.ShowWindow(new WindowInteropHelper(mainWindow).Handle, User32.WindowShowStyle.SW_HIDE);
                    }
                    else
                    {
                        window.Close();
                    }
                }
            }
            else
            {
                User32.ShowWindow(new WindowInteropHelper(mainWindow).Handle, User32.WindowShowStyle.SW_SHOWDEFAULT);
                mainWindow.Activate();
            }
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, "Main window has experienced an exception");
        }
    }

    #endregion

    public Task ShowLoadingOverlay(string title, ProgressAction progressAction) => Dispatcher.Invoke(() =>
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

        return progressAction.Execute();
    });

    public void HideLoadingOverlay() => Dispatcher.Invoke(() => LoadingOverlay.FadeOut(150));

    private void setContent(object userControl)
    {
        ContentControl.Content = userControl;
    }

    private void PackagesButton_OnClick(object sender, RoutedEventArgs e)
    {
        setContent(PackagesView);
    }

    private void ModulesButton_OnClick(object sender, RoutedEventArgs e)
    {
        setContent(ModulesView);
    }

    private void RouterButton_OnClick(object sender, RoutedEventArgs e)
    {
        setContent(RouterView);
    }

    private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
    }

    private void ChatBoxButton_OnClick(object sender, RoutedEventArgs e)
    {
        setContent(ChatBoxView);
    }

    private void StartupButton_OnClick(object sender, RoutedEventArgs e)
    {
        setContent(StartupView);
    }

    private void RunButton_OnClick(object sender, RoutedEventArgs e)
    {
        setContent(RunView);
    }

    private void DebugButton_OnClick(object sender, RoutedEventArgs e)
    {
        setContent(AppDebugView);
    }

    private void ProfilesButton_OnClick(object sender, RoutedEventArgs e)
    {
        setContent(ProfilesView);
    }

    private void AppSettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        setContent(AppSettingsView);
    }

    private void InformationButton_OnClick(object sender, RoutedEventArgs e)
    {
        setContent(InformationView);
    }

    public void FocusAppSettings()
    {
        AppSettingsTabButton.IsChecked = true;
        AppSettingsTabButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
    }
}