// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osuTK;
using PInvoke;
using VRCOSC.Game.Config;
using VRCOSC.Resources;
using Icon = System.Drawing.Icon;
using WindowState = osu.Framework.Platform.WindowState;

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

    private readonly Bindable<bool> inTray = new();
    private readonly NotifyIcon trayIcon = new();
    private IntPtr? windowHandle;

    protected override Container<Drawable> Content { get; }
    protected readonly DrawSizePreservingFillContainer DrawSizePreservingFillContainer;

    protected VRCOSCGameBase()
    {
        base.Content.Add(Content = DrawSizePreservingFillContainer = new DrawSizePreservingFillContainer
        {
            TargetDrawSize = Vector2.One
        });
    }

    [BackgroundDependencyLoader]
    private void load(Storage storage)
    {
        Resources.AddStore(new DllResourceStore(typeof(VRCOSCResources).Assembly));

        DependencyContainer.CacheAs(ConfigManager = new VRCOSCConfigManager(storage));

        versionBindable = ConfigManager.GetBindable<string>(VRCOSCSetting.Version);
        versionBindable.BindValueChanged(version => host.Window.Title = $"{host.Name} {version.NewValue}", true);

        Window.WindowState = ConfigManager.Get<WindowState>(VRCOSCSetting.WindowState);

        inTray.Value = ConfigManager.Get<bool>(VRCOSCSetting.StartInTray);
        setupTrayIcon();
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        Task.Run(async () =>
        {
            await Task.Delay(500);
            handleTrayTransition(false);
        });
    }

    protected override void Update()
    {
        if (host.Window.WindowState == WindowState.Minimised || inTray.Value)
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
        trayIcon.DoubleClick += (_, _) => handleTrayTransition(true);

        trayIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        trayIcon.Visible = true;
        trayIcon.Text = host.Name;

        var contextMenu = new ContextMenuStrip
        {
            Items = { { "Exit", null, (_, _) => host.Exit() } }
        };

        trayIcon.ContextMenuStrip = contextMenu;
    }

    private void handleTrayTransition(bool toggle)
    {
        var localWindowHandle = Process.GetCurrentProcess().MainWindowHandle;

        if (localWindowHandle != IntPtr.Zero)
        {
            windowHandle ??= localWindowHandle;
        }

        if (windowHandle is not null)
        {
            if (toggle) inTray.Value = !inTray.Value;
            User32.ShowWindow(windowHandle.Value, inTray.Value ? User32.WindowShowStyle.SW_HIDE : User32.WindowShowStyle.SW_SHOWDEFAULT);
        }
    }
}
