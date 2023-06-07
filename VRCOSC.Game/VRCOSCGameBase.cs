// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using PInvoke;
using VRCOSC.Game.Config;
using VRCOSC.Resources;

namespace VRCOSC.Game;

public partial class VRCOSCGameBase : osu.Framework.Game
{
    [Resolved]
    private GameHost host { get; set; } = null!;

    protected DependencyContainer DependencyContainer = null!;
    protected VRCOSCConfigManager ConfigManager = null!;

    private Bindable<string> versionBindable = null!;

    private static Version assemblyVersion => Assembly.GetEntryAssembly()?.GetName().Version ?? new Version();

    protected string Version => $@"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";

    protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        => DependencyContainer = new DependencyContainer(base.CreateChildDependencies(parent));

    protected override IDictionary<FrameworkSetting, object> GetFrameworkConfigDefaults() => new Dictionary<FrameworkSetting, object>
    {
        { FrameworkSetting.FrameSync, FrameSync.VSync }
    };

    private readonly BindableBool isInTray = new();
    private readonly NotifyIcon trayIcon = new();
    private IntPtr? windowHandle;

    [BackgroundDependencyLoader]
    private void load(Storage storage)
    {
        Resources.AddStore(new DllResourceStore(typeof(VRCOSCResources).Assembly));

        DependencyContainer.CacheAs(ConfigManager = new VRCOSCConfigManager(storage));

        versionBindable = ConfigManager.GetBindable<string>(VRCOSCSetting.Version);
        versionBindable.BindValueChanged(version => host.Window.Title = $"{host.Name} {version.NewValue}", true);

        Window.WindowState = ConfigManager.Get<WindowState>(VRCOSCSetting.WindowState);

        setupTrayIcon();
    }

    protected override void Update()
    {
        if (host.Window.WindowState == WindowState.Minimised || isInTray.Value)
            host.DrawThread.InactiveHz = 1;
        else
            host.DrawThread.InactiveHz = 60;
    }

    protected override bool OnExiting()
    {
        ConfigManager.SetValue(VRCOSCSetting.WindowState, Window.WindowState);

        trayIcon.Dispose();

        return false;
    }

    private void setupTrayIcon()
    {
        trayIcon.DoubleClick += (_, _) => handleTrayTransition();

        trayIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        trayIcon.Visible = true;
        trayIcon.Text = host.Name;

        var contextMenu = new ContextMenuStrip
        {
            Items = { { "Exit", null, (_, _) => host.Exit() } }
        };

        trayIcon.ContextMenuStrip = contextMenu;
    }

    private void handleTrayTransition()
    {
        var localWindowHandle = Process.GetCurrentProcess().MainWindowHandle;

        if (localWindowHandle != IntPtr.Zero)
        {
            windowHandle ??= localWindowHandle;
        }

        if (windowHandle is not null)
        {
            isInTray.Toggle();
            User32.ShowWindow(windowHandle.Value, isInTray.Value ? User32.WindowShowStyle.SW_HIDE : User32.WindowShowStyle.SW_SHOWDEFAULT);
        }
    }
}
