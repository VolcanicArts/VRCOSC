// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using PInvoke;
using VRCOSC.Actions;
using VRCOSC.Actions.Game;
using VRCOSC.Config;
using VRCOSC.OVR.Metadata;
using VRCOSC.Screens.Exceptions;
using VRCOSC.Screens.Loading;
using VRCOSC.Screens.Main;
using VRCOSC.Screens.Main.Tabs;

namespace VRCOSC;

[Cached]
public abstract partial class VRCOSCGame : VRCOSCGameBase
{
    [Resolved]
    private GameHost host { get; set; } = null!;

    [Resolved]
    private Storage storage { get; set; } = null!;

    [Cached]
    private readonly AppManager appManager = new();

    private ExceptionScreen exceptionScreen = null!;
    private LoadingScreen loadingScreen = null!;
    private MainScreen? mainScreen;

    public Bindable<Tab> SelectedTab = new(Tab.Home);

    public Action? OnListingRefresh;

    [BackgroundDependencyLoader]
    private void load()
    {
        Window.Title = host.Name;
        setupTrayIcon();

        Add(loadingScreen = new LoadingScreen());
        Add(exceptionScreen = new ExceptionScreen());
        ChangeChildDepth(exceptionScreen, float.MinValue);
    }

    private bool asyncLoadComplete;

    protected override async void LoadComplete()
    {
        base.LoadComplete();

        appManager.Initialise(host, this, storage, Clock, ConfigManager);
        appManager.OVRClient.OnShutdown += () =>
        {
            if (ConfigManager.Get<bool>(VRCOSCSetting.OVRAutoClose)) prepareForExit();
        };

        copyOpenVrFiles();

        LoadingScreen.Title.Value = "Welcome to VRCOSC";
        LoadingScreen.Description.Value = "Sit tight. We're getting things ready for you!";
        var loadingAction = new LoadGameAction();

        loadingAction.AddAction(appManager.PackageManager.Load());
        loadingAction.AddAction(new DynamicProgressAction("Loading profiles", () => appManager.ProfileManager.Load()));
        loadingAction.AddAction(new DynamicProgressAction("Loading modules", () => appManager.ModuleManager.LoadAllModules()));
        loadingAction.AddAction(new DynamicProgressAction("Loading routes", () => appManager.RouterManager.Load()));
        loadingAction.AddAction(new DynamicAsyncProgressAction("Loading graphics", async () =>
        {
            mainScreen = new MainScreen();
            await LoadComponentAsync(mainScreen);
            Add(mainScreen);
            ChangeChildDepth(mainScreen, float.MaxValue);
        }));

        loadingAction.OnComplete += () =>
        {
            asyncLoadComplete = true;
            Scheduler.Add(() => loadingScreen.Hide(), false);
        };

        LoadingScreen.SetAction(loadingAction);
        await loadingAction.Execute();
    }

    protected override void Update()
    {
        if (!asyncLoadComplete) return;

        appManager.FrameworkUpdate();
        host.DrawThread.InactiveHz = inTray ? 1 : 15;

        mainScreen!.ShouldBlur = loadingScreen.State.Value == Visibility.Visible || exceptionScreen.State.Value == Visibility.Visible;

        if (!startInTrayComplete) obtainWindowHandle();
    }

    #region Exit

    protected override bool OnExiting()
    {
        base.OnExiting();

        if (ConfigManager.Get<bool>(VRCOSCSetting.TrayOnClose))
        {
            inTray = true;
            handleTrayTransition();
            return true;
        }

        prepareForExit();

        return true;
    }

    private void prepareForExit()
    {
        trayIcon.Dispose();

        Task.WhenAll(
            appManager.StopAsync()
        ).GetAwaiter().OnCompleted(Exit);
    }

    #endregion

    #region Resources

    private void copyOpenVrFiles()
    {
        var tempStorage = storage.GetStorageForDirectory("runtime/openvr");
        var tempStoragePath = tempStorage.GetFullPath(string.Empty);

        var openVrFiles = Resources.GetAvailableResources().Where(file => file.StartsWith("OpenVR"));

        foreach (var file in openVrFiles)
        {
            File.WriteAllBytes(Path.Combine(tempStoragePath, file.Split('/')[1]), Resources.Get(file));
        }

        var manifest = new OVRManifest();
        manifest.Applications[0].ActionManifestPath = tempStorage.GetFullPath("action_manifest.json");
        manifest.Applications[0].ImagePath = tempStorage.GetFullPath("SteamImage.png");

        File.WriteAllText(Path.Combine(tempStoragePath, "app.vrmanifest"), JsonConvert.SerializeObject(manifest));
    }

    #endregion

    #region Tray

    private bool startInTrayComplete;
    private bool inTray;
    private IntPtr? windowHandle;
    private NotifyIcon trayIcon = null!;

    private void obtainWindowHandle()
    {
        if (windowHandle is null)
        {
            var localWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
            if (localWindowHandle != IntPtr.Zero) windowHandle ??= localWindowHandle;
        }

        if (windowHandle is not null)
        {
            if (ConfigManager.Get<bool>(VRCOSCSetting.StartInTray))
            {
                inTray = true;
                handleTrayTransition();
            }

            startInTrayComplete = true;
        }
    }

    private void setupTrayIcon()
    {
        trayIcon = new NotifyIcon();

        trayIcon.DoubleClick += (_, _) =>
        {
            inTray = !inTray;
            handleTrayTransition();
        };

        trayIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        trayIcon.Visible = true;
        trayIcon.Text = host.Name;

        var contextMenu = new ContextMenuStrip
        {
            Items =
            {
                {
                    "VRCOSC", null, (_, _) =>
                    {
                        inTray = false;
                        handleTrayTransition();
                    }
                },
                new ToolStripSeparator(),
                {
                    "Exit", null, (_, _) => Schedule(prepareForExit)
                }
            }
        };

        trayIcon.ContextMenuStrip = contextMenu;
    }

    private void handleTrayTransition()
    {
        if (windowHandle is null) return;

        User32.ShowWindow(windowHandle.Value, inTray ? User32.WindowShowStyle.SW_HIDE : User32.WindowShowStyle.SW_SHOWDEFAULT);
        if (!inTray) User32.SetForegroundWindow(windowHandle.Value);
    }

    #endregion
}
