// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Semver;
using VRCOSC.App.Actions;
using VRCOSC.App.ChatBox;
using VRCOSC.App.Dolly;
using VRCOSC.App.Modules;
using VRCOSC.App.Nodes;
using VRCOSC.App.Packages;
using VRCOSC.App.Profiles;
using VRCOSC.App.Router;
using VRCOSC.App.Settings;
using VRCOSC.App.Startup;
using VRCOSC.App.UI.Core;
using VRCOSC.App.UI.Views.AppSettings;
using VRCOSC.App.UI.Views.ChatBox;
using VRCOSC.App.UI.Views.Dolly;
using VRCOSC.App.UI.Views.Information;
using VRCOSC.App.UI.Views.Modules;
using VRCOSC.App.UI.Views.Nodes;
using VRCOSC.App.UI.Views.Packages;
using VRCOSC.App.UI.Views.Profiles;
using VRCOSC.App.UI.Views.Run;
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
    public ChatBoxView ChatBoxView = null!;
    public NodesView NodesView = null!;
    public DollyView DollyView = null!;
    public RunView RunView = null!;
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
        SourceInitialized += OnSourceInitialized;
        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        this.ApplyDefaultStyling();
        this.SetPositionFrom(null);

        backupV1Files();

        // load settings before anything else
        SettingsManager.GetInstance().Load();

        var installedUpdateChannel = SettingsManager.GetInstance().GetValue<UpdateChannel>(VRCOSCMetadata.InstalledUpdateChannel);
        Title = $"{AppManager.APP_NAME} {AppManager.Version}";
        if (installedUpdateChannel != UpdateChannel.Live) Title += $" {installedUpdateChannel.ToString().ToUpper()}";

        load();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.StartInTray))
        {
            // required for some windows scheduling funkiness
            await Task.Delay(100);
            transitionTray(true);
        }
    }

    private async void OnClosing(object? sender, CancelEventArgs e)
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

        foreach (Window window in Application.Current.Windows)
        {
            if (window != this)
                window.Close();
        }

        trayIcon?.Dispose();
    }

    // because this isn't returning a task and we're not doing Task.Run(), this still runs in the same
    // synchronisation context as the UI thread, meaning we don't get threading errors when initialising
    // unfortunately this does mean it also blocks the UI thread when making changes, freezing the loading screen,
    // but this method being async at least means that the window opens immediately and we can tell
    // the user what's going on
    private async void load()
    {
        // force the task to be async to open the window ASAP
        await Task.Delay(1);

        velopackUpdater = new VelopackUpdater();

        var lastUpdateCheck = SettingsManager.GetInstance().GetValue<DateTime>(VRCOSCMetadata.LastUpdateCheck);
        var updateCheckExpired = lastUpdateCheck <= DateTime.Now - TimeSpan.FromMinutes(10);

        Logger.Log($"Update check expired? {updateCheckExpired}");

        if (updateCheckExpired)
        {
            SettingsManager.GetInstance().GetObservable<DateTime>(VRCOSCMetadata.LastUpdateCheck).Value = DateTime.Now;
        }

        if (updateCheckExpired)
        {
            var isUpdating = await checkForUpdates();
            if (isUpdating) return;
        }

        createBackupIfUpdated();
        setupTrayIcon();

        AppManager.GetInstance().Initialise();

        PackagesView = new PackagesView();
        ModulesView = new ModulesView();
        ChatBoxView = new ChatBoxView();
        NodesView = new NodesView();
        DollyView = new DollyView();
        RunView = new RunView();
        ProfilesView = new ProfilesView();
        AppSettingsView = new AppSettingsView();
        InformationView = new InformationView();

        SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.EnableAppDebug).Subscribe(newValue => ShowAppDebug.Value = newValue, true);

        var isBeta = SettingsManager.GetInstance().GetValue<UpdateChannel>(VRCOSCMetadata.InstalledUpdateChannel) == UpdateChannel.Beta;

        await PackageManager.GetInstance().Load();

        var appVersionChanged = hasAppVersionChanged();

        if (appVersionChanged)
        {
            SettingsManager.GetInstance().GetObservable<string>(VRCOSCMetadata.InstalledVersion).Value = AppManager.Version;
        }

        if (appVersionChanged || (PackageManager.GetInstance().IsCacheOutdated && updateCheckExpired))
        {
            await PackageManager.GetInstance().RefreshAllSources(true);

            if (SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.AutoUpdatePackages) && PackageManager.GetInstance().AnyInstalledPackageUpdates(isBeta))
            {
                await PackageManager.GetInstance().UpdateAllInstalledPackages(isBeta);
            }
        }

        if (!SettingsManager.GetInstance().GetValue<bool>(VRCOSCMetadata.FirstTimeSetupComplete))
        {
            await PackageManager.GetInstance().InstallPackage(PackageManager.GetInstance().OfficialModulesSource, reloadAll: false, refreshBeforeInstall: false);

            var ftsWindow = new FirstTimeInstallWindow();

            ftsWindow.SourceInitialized += (_, _) =>
            {
                ftsWindow.ApplyDefaultStyling();
                ftsWindow.SetPositionFrom(this);
            };

            ftsWindow.Show();

            SettingsManager.GetInstance().GetObservable<bool>(VRCOSCMetadata.FirstTimeSetupComplete).Value = true;
        }

        ProfileManager.GetInstance().Load();
        ModuleManager.GetInstance().LoadAllModules();
        NodeManager.GetInstance().Load();
        ChatBoxManager.GetInstance().Load();
        RouterManager.GetInstance().Load();
        DollyManager.GetInstance().Load();
        StartupManager.GetInstance().Load();

        setContent(ModulesView);

        AppManager.GetInstance().InitialLoadComplete();

        LoadingSpinner.Visibility = Visibility.Collapsed;
        LoadingOverlay.FadeOut(500);
    }

    private async Task<bool> checkForUpdates()
    {
        try
        {
            if (velopackUpdater.IsInstalled())
            {
                await velopackUpdater.CheckForUpdatesAsync();

                if (velopackUpdater.IsUpdateAvailable())
                {
                    var shouldUpdate = velopackUpdater.PresentUpdate();

                    if (shouldUpdate)
                    {
                        await ShowLoadingOverlay(new CallbackProgressAction("Updating VRCOSC", () => velopackUpdater.ExecuteUpdateAsync()));
                        return true;
                    }
                }

                Logger.Log("No updates. Proceeding with loading");
            }
            else
            {
                Logger.Log("Portable app detected. Cancelling update check");
            }

            return false;
        }
        catch (HttpRequestException e)
        {
            // rate limit exceeded
            if (e.StatusCode == HttpStatusCode.Forbidden)
            {
                Logger.Error(e, "Cannot check for updates. Rate limit has occurred");
            }
            else
            {
                ExceptionHandler.Handle(e, "Cannot check for updates");
            }

            return false;
        }
    }

    private bool hasAppVersionChanged() => calculateVersionPrecedence() != 0;
    private bool hasAppUpdated() => calculateVersionPrecedence() == 1;

    private int calculateVersionPrecedence()
    {
        var installedVersionStr = SettingsManager.GetInstance().GetValue<string>(VRCOSCMetadata.InstalledVersion);
        if (string.IsNullOrEmpty(installedVersionStr)) return -1;

        var newVersion = SemVersion.Parse(AppManager.Version, SemVersionStyles.Any);
        var installedVersion = SemVersion.Parse(installedVersionStr, SemVersionStyles.Any);

        return SemVersion.ComparePrecedence(newVersion, installedVersion);
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

        if (!hasAppUpdated() || storage.GetStorageForDirectory(root_backup_directory_name).ExistsDirectory(installedVersionStr)) return;

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

        contextMenu.Items.Add("Exit", null, (_, _) => Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() => Application.Current.Shutdown())
        ));

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

    public Task ShowLoadingOverlay(ProgressAction progressAction) => Dispatcher.Invoke(() =>
    {
        LoadingTitle.Text = progressAction.Title;
        LoadingProgressBar.Visibility = progressAction.UseProgressBar ? Visibility.Visible : Visibility.Collapsed;
        LoadingSpinner.Visibility = progressAction.UseProgressBar ? Visibility.Collapsed : Visibility.Visible;

        progressAction.OnProgressChanged += p => Dispatcher.Invoke(() => LoadingProgressBar.Value = p);
        progressAction.OnComplete += () => LoadingOverlay.FadeOut(150, () => LoadingProgressBar.Value = 0);

        LoadingOverlay.FadeIn(150);

        return progressAction.Execute();
    });

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

    private void ChatBoxButton_OnClick(object sender, RoutedEventArgs e)
    {
        setContent(ChatBoxView);
    }

    private void NodesButton_OnClick(object sender, RoutedEventArgs e)
    {
        setContent(NodesView);
    }

    private void DollyButton_OnClick(object sender, RoutedEventArgs e)
    {
        setContent(DollyView);
    }

    private void RunButton_OnClick(object sender, RoutedEventArgs e)
    {
        setContent(RunView);
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