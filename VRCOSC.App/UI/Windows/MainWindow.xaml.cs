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
using Newtonsoft.Json;
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
    private const string root_backup_directory_name = "backups";

    public static MainWindow GetInstance() => Application.Current.Dispatcher.Invoke(() => (MainWindow)Application.Current.MainWindow!);

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

    private void startApp()
    {
        SettingsManager.GetInstance().Load();

        createBackupIfUpdated();

        velopackUpdater = new VelopackUpdater();

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

        load();
    }

    private void backupV1Files()
    {
        try
        {
            if (storage.Exists("framework.ini"))
            {
                var sourceDir = storage.GetFullPath(string.Empty);
                var destinationDir = storage.GetStorageForDirectory(root_backup_directory_name).GetStorageForDirectory("v1").GetFullPath(string.Empty);

                foreach (var file in Directory.GetFiles(sourceDir))
                {
                    var fileName = Path.GetFileName(file);
                    var destFile = Path.Combine(destinationDir, fileName);
                    File.Move(file, destFile, false);
                }

                foreach (var dir in Directory.GetDirectories(sourceDir))
                {
                    var dirName = Path.GetFileName(dir);
                    if (dirName == root_backup_directory_name) continue;

                    var destDir = Path.Combine(destinationDir, dirName);
                    Directory.Move(dir, destDir);
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Could not backup V1 files");
        }

        // move v1-backup into backups/v1

        try
        {
            if (storage.ExistsDirectory("v1-backup"))
            {
                var sourceDir = storage.GetFullPath("v1-backup");
                var destinationDir = storage.GetStorageForDirectory(root_backup_directory_name).GetFullPath("v1");

                Directory.Move(sourceDir, destinationDir);
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "An error has occured moving the V1 backup folder");
        }
    }

    private void createBackupIfUpdated()
    {
        var installedVersionStr = SettingsManager.GetInstance().GetValue<string>(VRCOSCMetadata.InstalledVersion);
        if (string.IsNullOrEmpty(installedVersionStr) || storage.GetStorageForDirectory(root_backup_directory_name).ExistsDirectory(installedVersionStr)) return;

        var newVersion = SemVersion.Parse(AppManager.Version, SemVersionStyles.Any);
        var installedVersion = SemVersion.Parse(installedVersionStr, SemVersionStyles.Any);

        var hasUpdated = SemVersion.ComparePrecedence(newVersion, installedVersion) == 1;
        if (!hasUpdated) return;

        Logger.Log("App has updated. Creating a backup for previous version");

        var sourceDir = storage.GetFullPath(string.Empty);
        var destDir = storage.GetStorageForDirectory(root_backup_directory_name).GetStorageForDirectory(installedVersionStr).GetFullPath(string.Empty);

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var dirName = Path.GetFileName(dir);
            // don't want `backups`. Don't need `logs` or `runtime` directories
            if (dirName is root_backup_directory_name or "logs" or "runtime") continue;

            storage.GetStorageForDirectory(dirName).CopyTo(Path.Combine(destDir, dirName));
        }
    }

    private async void load()
    {
        var doFirstTimeSetup = !SettingsManager.GetInstance().GetValue<bool>(VRCOSCMetadata.FirstTimeSetupComplete);

        await PackageManager.GetInstance().Load();

        if (doFirstTimeSetup)
        {
            await PackageManager.GetInstance().InstallPackage(PackageManager.GetInstance().OfficialModulesSource, reloadAll: false, refreshBeforeInstall: false);
        }

        ProfileManager.GetInstance().Load();
        ModuleManager.GetInstance().LoadAllModules();
        ChatBoxManager.GetInstance().Load();
        RouterManager.GetInstance().Load();
        StartupManager.GetInstance().Load();

        MainWindowContent.FadeInFromZero(500);
        AppManager.GetInstance().InitialLoadComplete();

        if (doFirstTimeSetup)
        {
            var ftsWindow = new FirstTimeInstallWindow();

            ftsWindow.SourceInitialized += (_, _) =>
            {
                ftsWindow.ApplyDefaultStyling();
                ftsWindow.SetPositionFrom(this);
            };

            ftsWindow.Show();

            SettingsManager.GetInstance().GetObservable<bool>(VRCOSCMetadata.FirstTimeSetupComplete).Value = true;
        }

        var appUpdated = false;

        var installedVersion = SettingsManager.GetInstance().GetValue<string>(VRCOSCMetadata.InstalledVersion);

        if (!string.IsNullOrEmpty(installedVersion))
        {
            var cachedInstalledVersion = SemVersion.Parse(installedVersion, SemVersionStyles.Any);
            var newInstalledVersion = SemVersion.Parse(AppManager.Version, SemVersionStyles.Any);

            if (SemVersion.ComparePrecedence(cachedInstalledVersion, newInstalledVersion) != 0)
            {
                appUpdated = true;
            }
        }

        if (appUpdated || string.IsNullOrEmpty(installedVersion))
        {
            SettingsManager.GetInstance().GetObservable<string>(VRCOSCMetadata.InstalledVersion).Value = AppManager.Version;
        }

        var cacheOutdated = PackageManager.GetInstance().IsCacheOutdated;

        if (!appUpdated && !cacheOutdated)
        {
            WelcomeOverlay.FadeOutFromOne(1000);
        }

        if (velopackUpdater.IsInstalled())
        {
            // only check for an update if we haven't just updated (to save time checking again)
            if (!appUpdated)
            {
                await velopackUpdater.CheckForUpdatesAsync();

                if (velopackUpdater.IsUpdateAvailable())
                {
                    var isUpdating = await velopackUpdater.ExecuteUpdate();
                    if (isUpdating) return;
                }

                Logger.Log("No updates. Proceeding with loading");
            }
            else
            {
                Logger.Log("App has just updated. Skipping update check");
            }
        }
        else
        {
            Logger.Log("Portable app detected. Cancelling update check");
        }

        if (appUpdated || cacheOutdated)
        {
            await PackageManager.GetInstance().RefreshAllSources(true);

            if (SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.AutoUpdatePackages) && PackageManager.GetInstance().AnyInstalledPackageUpdates())
            {
                await PackageManager.GetInstance().UpdateAllInstalledPackages();
                await ModuleManager.GetInstance().ReloadAllModules();
            }

            WelcomeOverlay.FadeOutFromOne(500);
        }
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
            transitionTray(true);
            return;
        }

        var appManager = AppManager.GetInstance();

        if (appManager.State.Value is AppManagerState.Started)
        {
            e.Cancel = true;
            await appManager.StopAsync();
            Close();
            return;
        }

        OVRDeviceManager.GetInstance().Serialise();
        RouterManager.GetInstance().Serialise();
        StartupManager.GetInstance().Serialise();
        SettingsManager.GetInstance().Serialise();

        trayIcon?.Dispose();
    }

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.StartInTray))
        {
            // required for some windows scheduling funkiness
            await Task.Delay(100);
            transitionTray(true);
        }
    }

    private void MainWindow_OnSourceInitialized(object? _, EventArgs _2)
    {
        this.ApplyDefaultStyling();
        this.SetPositionFrom(null);
    }

    #region Tray

    private bool currentlyInTray;
    private NotifyIcon? trayIcon;

    private void setupTrayIcon()
    {
        trayIcon = new NotifyIcon();
        trayIcon.DoubleClick += (_, _) => transitionTray(!currentlyInTray);

        trayIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly()!.Location)!;
        trayIcon.Visible = true;
        trayIcon.Text = AppManager.APP_NAME;

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add(AppManager.APP_NAME, null, (_, _) => transitionTray(false));
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add("Exit", null, (_, _) => Dispatcher.Invoke(() => Application.Current.Shutdown()));

        trayIcon.ContextMenuStrip = contextMenu;
    }

    private void transitionTray(bool inTray)
    {
        try
        {
            if (!currentlyInTray && inTray)
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window == this)
                        window.Hide();
                    else
                        window.Close();
                }

                currentlyInTray = true;
                return;
            }

            if (currentlyInTray && !inTray)
            {
                Show();
                Activate();
                currentlyInTray = false;
                return;
            }
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, "Main window has experienced an exception");
        }
    }

    #endregion

    public Task ShowLoadingOverlay(string title, ProgressAction progressAction)
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
    }

    public void HideLoadingOverlay() => LoadingOverlay.FadeOut(150);

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