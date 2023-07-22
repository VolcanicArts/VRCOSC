// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osuTK;
using VRCOSC.Game.App;
using VRCOSC.Game.Config;
using VRCOSC.Game.Github;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Graphics.Settings;
using VRCOSC.Game.Graphics.TabBar;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.Updater;
using VRCOSC.Game.Modules;
using VRCOSC.Game.OpenVR.Metadata;

namespace VRCOSC.Game;

[Cached]
public abstract partial class VRCOSCGame : VRCOSCGameBase
{
    private const string latest_release_url = "https://github.com/volcanicarts/vrcosc/releases/latest";
    private const string discord_invite_url = "https://discord.gg/vj4brHyvT5";
    private const string kofi_url = "https://ko-fi.com/volcanicarts";

    [Resolved]
    private GameHost host { get; set; } = null!;

    [Resolved]
    private Storage storage { get; set; } = null!;

    [Cached]
    private Bindable<Tab> selectedTab = new();

    [Cached(name: "EditingModule")]
    private Bindable<Module?> editingModule = new();

    [Cached(name: "InfoModule")]
    private Bindable<Module?> infoModule = new();

    [Cached]
    private AppManager appManager = new();

    private NotificationContainer notificationContainer = null!;
    private VRCOSCUpdateManager updateManager = null!;
    private Bindable<float> uiScaleBindable = null!;

    protected VRCOSCGame(bool enableModuleDebugMode)
    {
        ModuleDebugLogger.Enabled = enableModuleDebugMode;

        Logger.Log($"Module debug mode: {enableModuleDebugMode}");
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        AddFont(Resources, @"Fonts/ArialUnicode/ArialUnicode");

        ThemeManager.VRCOSCTheme = ConfigManager.Get<VRCOSCTheme>(VRCOSCSetting.Theme);

        DependencyContainer.CacheAs(notificationContainer = new NotificationContainer());
        DependencyContainer.CacheAs(new GitHubProvider(host.Name));

        LoadComponent(notificationContainer);

        Children = new Drawable[]
        {
            appManager,
            new MainContent(),
            updateManager = CreateUpdateManager(),
            notificationContainer
        };

        uiScaleBindable = ConfigManager.GetBindable<float>(VRCOSCSetting.UIScale);
        uiScaleBindable.BindValueChanged(e => updateUiScale(e.NewValue), true);
        host.Window.Resized += () => updateUiScale(uiScaleBindable.Value);
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        copyOpenVrFiles();
        checkUpdates();
        checkVersion();

        notificationContainer.Notify(new TimedNotification
        {
            Title = "Join The Community!",
            Description = "Click to join the Discord server",
            Icon = FontAwesome.Brands.Discord,
            Colour = Colour4.FromHex(@"7289DA"),
            ClickCallback = () => host.OpenUrlExternally(discord_invite_url),
            Delay = 5000d
        });

        notificationContainer.Notify(new TimedNotification
        {
            Title = "Enjoying the app?",
            Description = "Click to buy me a coffee",
            Icon = FontAwesome.Solid.Coffee,
            Colour = Colour4.FromHex(@"ff5f5f"),
            ClickCallback = () => host.OpenUrlExternally(kofi_url),
            Delay = 5000d
        });

        appManager.State.BindValueChanged(e =>
        {
            if (e.NewValue == AppManagerState.Starting) selectedTab.Value = Tab.Run;
        }, true);

        appManager.OVRClient.OnShutdown += () =>
        {
            if (ConfigManager.Get<bool>(VRCOSCSetting.AutoStopOpenVR)) prepareForExit();
        };

        selectedTab.BindValueChanged(tab =>
        {
            if (tab.OldValue == Tab.Router) appManager.RouterManager.Serialise();
        });

        editingModule.BindValueChanged(e =>
        {
            if (e.NewValue is null && e.OldValue is not null) e.OldValue.Serialise();
        }, true);
    }

    private void updateUiScale(float scaler = 1f) => Scheduler.AddOnce(() =>
    {
        var windowSize = host.Window.ClientSize;
        DrawSizePreservingFillContainer.TargetDrawSize = new Vector2(windowSize.Width * scaler, windowSize.Height * scaler);
    });

    private void checkUpdates()
    {
        var updateMode = ConfigManager.Get<UpdateMode>(VRCOSCSetting.UpdateMode);
        if (updateMode != UpdateMode.Off) updateManager.PerformUpdateCheck();
    }

    private void checkVersion()
    {
        var lastVersion = ConfigManager.Get<string>(VRCOSCSetting.Version);

        if (Version != lastVersion && !string.IsNullOrEmpty(lastVersion))
        {
            notificationContainer.Notify(new BasicNotification
            {
                Title = "VRCOSC Updated",
                Description = "Click to see the changes",
                Icon = FontAwesome.Solid.Download,
                Colour = ThemeManager.Current[ThemeAttribute.Success],
                ClickCallback = () => host.OpenUrlExternally(latest_release_url)
            });
        }

        ConfigManager.SetValue(VRCOSCSetting.Version, Version);
    }

    private void copyOpenVrFiles()
    {
        var tempStorage = storage.GetStorageForDirectory(@"openvr");
        var tempStoragePath = tempStorage.GetFullPath(string.Empty);

        var openVrFiles = Resources.GetAvailableResources().Where(file => file.StartsWith(@"OpenVR"));

        foreach (var file in openVrFiles)
        {
            File.WriteAllBytes(Path.Combine(tempStoragePath, file.Split('/')[1]), Resources.Get(file));
        }

        var manifest = new OVRManifest();
        manifest.Applications[0].ActionManifestPath = tempStorage.GetFullPath(@"action_manifest.json");
        manifest.Applications[0].ImagePath = tempStorage.GetFullPath(@"SteamImage.png");

        File.WriteAllText(Path.Combine(tempStoragePath, @"app.vrmanifest"), JsonConvert.SerializeObject(manifest));
    }

    protected override bool OnExiting()
    {
        base.OnExiting();
        prepareForExit();
        return true;
    }

    private void prepareForExit()
    {
        // ReSharper disable once RedundantExplicitParamsArrayCreation
        Task.WhenAll(new[]
        {
            appManager.StopAsync()
        }).ContinueWith(_ => Schedule(performExit));
    }

    private void performExit()
    {
        // Force RouterManager to serialise by changing tabs
        selectedTab.Value = Tab.Modules;
        editingModule.Value = null;
        Exit();
    }

    protected abstract VRCOSCUpdateManager CreateUpdateManager();
}
