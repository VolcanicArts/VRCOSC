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
using osu.Framework.Platform;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Graphics.Settings;
using VRCOSC.Game.Graphics.TabBar;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.Updater;
using VRCOSC.Game.Managers;
using VRCOSC.Game.Modules;
using VRCOSC.Game.OpenVR.Metadata;

namespace VRCOSC.Game;

[Cached]
public abstract partial class VRCOSCGame : VRCOSCGameBase
{
    private const string latest_release_url = "https://github.com/volcanicarts/vrcosc/releases/latest";
    private const string discord_invite_url = "https://discord.gg/vj4brHyvT5";

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
    private Bindable<Module.ModuleType?> typeFilter = new();

    [Cached]
    public GameManager GameManager = new();

    [Cached]
    public ChatBoxManager ChatBoxManager = new();

    private NotificationContainer notificationContainer = null!;
    private VRCOSCUpdateManager updateManager = null!;
    private RouterManager routerManager = null!;
    private StartupManager startupManager = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        ThemeManager.Theme = ConfigManager.Get<ColourTheme>(VRCOSCSetting.Theme);

        DependencyContainer.CacheAs(notificationContainer = new NotificationContainer());
        DependencyContainer.CacheAs(typeof(IVRCOSCSecrets), GetSecrets());
        DependencyContainer.CacheAs(routerManager = new RouterManager(storage, notificationContainer));
        DependencyContainer.CacheAs(startupManager = new StartupManager(storage, notificationContainer));

        LoadComponent(notificationContainer);

        routerManager.Load();
        startupManager.Load();

        Children = new Drawable[]
        {
            GameManager,
            new MainContent(),
            updateManager = CreateUpdateManager(),
            notificationContainer
        };
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

        GameManager.State.BindValueChanged(e =>
        {
            if (e.NewValue == GameManagerState.Starting) selectedTab.Value = Tab.Run;
        }, true);

        GameManager.OVRClient.OnShutdown += () =>
        {
            if (ConfigManager.Get<bool>(VRCOSCSetting.AutoStopOpenVR)) prepareForExit();
        };

        selectedTab.BindValueChanged(tab =>
        {
            if (tab.OldValue == Tab.Router) routerManager.Serialise();
        });
    }

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
            GameManager.StopAsync()
        }).ContinueWith(_ => Schedule(performExit));
    }

    private void performExit()
    {
        editingModule.Value = null;
        infoModule.Value = null;
        Exit();
    }

    protected abstract IVRCOSCSecrets GetSecrets();
    protected abstract VRCOSCUpdateManager CreateUpdateManager();
}
