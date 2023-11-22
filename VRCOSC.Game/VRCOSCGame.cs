// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Platform;
using PInvoke;
using VRCOSC.Game.Config;
using VRCOSC.Game.Screens.Loading;
using VRCOSC.Game.Screens.Main;
using VRCOSC.Game.Screens.Main.Tabs;

namespace VRCOSC.Game;

[Cached]
public abstract partial class VRCOSCGame : VRCOSCGameBase
{
    [Resolved]
    private GameHost host { get; set; } = null!;

    [Resolved]
    private Storage storage { get; set; } = null!;

    [Cached]
    private readonly AppManager appManager = new();

    public LoadingScreen LoadingScreen { get; private set; } = null!;

    public Bindable<Tab> SelectedTab = new(Tab.Home);

    public Action? OnListingRefresh;

    [BackgroundDependencyLoader]
    private void load()
    {
        Window.Title = host.Name;
        setupTrayIcon();

        Add(LoadingScreen = new LoadingScreen());
    }

    protected override async void LoadComplete()
    {
        base.LoadComplete();

        LoadingScreen.Title.Value = "Welcome to VRCOSC";
        LoadingScreen.Description.Value = "Sit tight. We're getting things ready for you!";

        LoadingScreen.Action.Value = "Loading managers";
        appManager.Initialise(this, host, storage, Clock);

        LoadingScreen.Action.Value = "Loading packages";

        void onRemoteModulesActionProgress(LoadingInfo loadingInfo)
        {
            LoadingScreen.Action.Value = loadingInfo.Action;
            LoadingScreen.Progress.Value = loadingInfo.Progress;
        }

        appManager.PackageManager.Progress = onRemoteModulesActionProgress;
        await appManager.PackageManager.Load();

        LoadingScreen.Action.Value = "Loading profiles";
        appManager.ProfileManager.Load();

        LoadingScreen.Action.Value = "Loading modules";
        appManager.ModuleManager.LoadAllModules();

        LoadingScreen.Action.Value = "Loading graphics";
        var mainScreen = new MainScreen
        {
            EnableBlur = LoadingScreen.State.GetBoundCopy()
        };

        await LoadComponentAsync(mainScreen);
        Add(mainScreen);
        ChangeChildDepth(mainScreen, float.MaxValue);

        LoadingScreen.Action.Value = "Complete!";
        LoadingScreen.Progress.Value = 1f;
        Scheduler.Add(() => LoadingScreen.Hide(), false);
    }

    protected override void Update()
    {
        appManager.FrameworkUpdate();
        host.DrawThread.InactiveHz = inTray ? 1 : 15;

        if (!startInTrayComplete) obtainWindowHandle();
    }

    protected override bool OnExiting()
    {
        trayIcon.Dispose();
        Task.WhenAll(
            appManager.StopAsync()
        ).GetAwaiter().OnCompleted(Exit);

        return true;
    }

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
                    "Exit", null, (_, _) => Schedule(Exit)
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
