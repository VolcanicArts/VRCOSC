// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Platform;
using osu.Framework.Screens;
using PInvoke;
using VRCOSC.Game.Config;
using VRCOSC.Game.Screens.Loading;
using VRCOSC.Game.Screens.Main;
using Screen = osu.Framework.Screens.Screen;

namespace VRCOSC.Game;

[Cached]
public abstract partial class VRCOSCGame : VRCOSCGameBase
{
    [Resolved]
    private GameHost host { get; set; } = null!;

    [Resolved]
    private Storage storage { get; set; } = null!;

    private readonly AppManager appManager = new();

    private ScreenStack baseScreenStack = null!;

    private Screen mainScreen = null!;
    private Screen loadingScreen = null!;

    /// <summary>
    /// For use when <see cref="LoadingScreen"/> is pushed
    /// </summary>
    public Bindable<string> LoadingAction = new(string.Empty);

    /// <summary>
    /// For use when <see cref="LoadingScreen"/> is pushed
    /// </summary>
    public Bindable<float> LoadingProgress = new();

    [BackgroundDependencyLoader]
    private void load()
    {
        Window.Title = host.Name;
        setupTrayIcon();

        Add(baseScreenStack = new ScreenStack());

        loadingScreen = new LoadingScreen();
        mainScreen = new MainScreen();

        LoadComponent(loadingScreen);
        baseScreenStack.Push(loadingScreen);
    }

    protected override async void LoadComplete()
    {
        base.LoadComplete();

        LoadingAction.Value = "Loading graphics";
        LoadingProgress.Value = 0f;
        await LoadComponentAsync(mainScreen);
        LoadingProgress.Value = 1f;

        LoadingAction.Value = "Loading managers";
        LoadingProgress.Value = 0f;
        appManager.Initialise(storage);
        LoadingProgress.Value = 1f;

        LoadingAction.Value = "Loading modules";
        LoadingProgress.Value = 0f;
        appManager.ModuleManager.LoadAllModules();
        LoadingProgress.Value = 1f;

        LoadingAction.Value = "Complete!";
        LoadingProgress.Value = 1f;
        Scheduler.Add(() => baseScreenStack.Push(mainScreen), false);
    }

    protected override void Update()
    {
        if (!startInTrayComplete) obtainWindowHandle();
    }

    protected override bool OnExiting()
    {
        trayIcon.Dispose();

        return base.OnExiting();
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
