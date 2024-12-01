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
using VRCOSC.App.UI.Views.Settings;
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

    public readonly PackagesView PackagesView;
    public readonly ModulesView ModulesView;
    public readonly RouterView RouterView;
    public readonly SettingsView SettingsView;
    public readonly ChatBoxView ChatBoxView;
    public readonly RunView RunView;
    public readonly AppDebugView AppDebugView;
    public readonly ProfilesView ProfilesView;
    public readonly AppSettingsView AppSettingsView;
    public readonly InformationView InformationView;

    private readonly Storage storage = AppManager.GetInstance().Storage;

    public Observable<bool> ShowAppDebug { get; } = new();
    public Observable<bool> ShowRouter { get; } = new();

    public MainWindow()
    {
        backupV1Files();

        SettingsManager.GetInstance().Load();
        SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.EnableAppDebug).Subscribe(newValue => ShowAppDebug.Value = newValue, true);
        SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.EnableRouter).Subscribe(newValue => ShowRouter.Value = newValue, true);

        AppManager.GetInstance().Initialise();

        InitializeComponent();
        DataContext = this;

        var installedUpdateChannel = SettingsManager.GetInstance().GetValue<UpdateChannel>(VRCOSCMetadata.InstalledUpdateChannel);
        Title = installedUpdateChannel == UpdateChannel.Beta ? $"{AppManager.APP_NAME} {AppManager.Version} BETA" : $"{AppManager.APP_NAME} {AppManager.Version}";

        setupTrayIcon();
        copyOpenVrFiles();

        PackagesView = new PackagesView();
        ModulesView = new ModulesView();
        RouterView = new RouterView();
        SettingsView = new SettingsView();
        ChatBoxView = new ChatBoxView();
        RunView = new RunView();
        AppDebugView = new AppDebugView();
        ProfilesView = new ProfilesView();
        AppSettingsView = new AppSettingsView();
        InformationView = new InformationView();

        setContent(ModulesView);

        load();
    }

    private void backupV1Files()
    {
        if (!storage.Exists("framework.ini")) return;

        var sourceDir = storage.GetFullPath(string.Empty);
        var destinationDir = storage.GetStorageForDirectory("v1-backup").GetFullPath(string.Empty);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var fileName = Path.GetFileName(file);
            var destFile = Path.Combine(destinationDir, fileName);
            File.Move(file, destFile);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var dirName = Path.GetFileName(dir);
            if (dirName == "v1-backup") continue;

            var destDir = Path.Combine(destinationDir, dirName);
            Directory.Move(dir, destDir);
        }
    }

    private void load()
    {
        var loadingAction = new CompositeProgressAction();

        loadingAction.AddAction(new DynamicAsyncProgressAction("Checking for updates", () => AppManager.GetInstance().VelopackUpdater.CheckForUpdatesAsync()));
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

        loadingAction.AddAction(new DynamicProgressAction("Loading Profiles", () => ProfileManager.GetInstance().Load()));
        loadingAction.AddAction(new DynamicProgressAction("Loading Modules", () => ModuleManager.GetInstance().LoadAllModules()));
        loadingAction.AddAction(new DynamicProgressAction("Loading ChatBox", () => ChatBoxManager.GetInstance().Load()));
        loadingAction.AddAction(new DynamicProgressAction("Loading Router", () => RouterManager.GetInstance().Load()));

        if (!SettingsManager.GetInstance().GetValue<bool>(VRCOSCMetadata.FirstTimeSetupComplete))
        {
            loadingAction.AddAction(new DynamicChildProgressAction(() => PackageManager.GetInstance().InstallPackage(PackageManager.GetInstance().OfficialModulesSource)));
            loadingAction.AddAction(new DynamicProgressAction(string.Empty, () => new FirstTimeInstallWindow().Show()));
        }

        loadingAction.OnComplete += () =>
        {
            Dispatcher.Invoke(() =>
            {
                SettingsManager.GetInstance().GetObservable<bool>(VRCOSCMetadata.FirstTimeSetupComplete).Value = true;
                MainWindowContent.FadeInFromZero(500);
                AppManager.GetInstance().InitialLoadComplete();
            });
        };

        _ = ShowLoadingOverlay("Welcome to VRCOSC", loadingAction);
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
    }

    private void MainWindow_OnClosed(object? sender, EventArgs e)
    {
        trayIcon.Dispose();

        foreach (Window window in Application.Current.Windows)
        {
            if (window != this) window.Close();
        }
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        this.SetPosition(null, ScreenChoice.Primary, HorizontalPosition.Center, VerticalPosition.Center);

        if (SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.StartInTray))
        {
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
        setContent(SettingsView);
    }

    private void ChatBoxButton_OnClick(object sender, RoutedEventArgs e)
    {
        setContent(ChatBoxView);
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