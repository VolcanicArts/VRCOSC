// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

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
using VRCOSC.Game.Graphics.Updater;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game;

[Cached]
public abstract class VRCOSCGame : VRCOSCGameBase
{
    [Cached]
    private ModuleManager moduleManager = new();

    [Resolved]
    private GameHost host { get; set; } = null!;

    public VRCOSCUpdateManager UpdateManager = null!;

    private NotificationContainer notificationContainer = null!;

    public Bindable<string> SearchTermFilter = new(string.Empty);
    public Bindable<ModuleType?> TypeFilter = new();

    [Cached]
    private Bindable<Tab> SelectedTab = new();

    [Cached]
    private BindableBool ModulesRunning = new();

    [Cached]
    private Bindable<Module?> EditingModule = new();

    [BackgroundDependencyLoader]
    private void load()
    {
        notificationContainer = new NotificationContainer();
        DependencyContainer.CacheAs(notificationContainer);

        Children = new Drawable[]
        {
            moduleManager,
            notificationContainer,
            new MainContent(),
            UpdateManager = CreateUpdateManager()
        };

        ChangeChildDepth(notificationContainer, float.MinValue);
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        ModulesRunning.BindValueChanged(e =>
        {
            if (e.NewValue) SelectedTab.Value = Tab.Modules;
        }, true);

        var updateMode = ConfigManager.Get<UpdateMode>(VRCOSCSetting.UpdateMode);

        if (updateMode != UpdateMode.Off) UpdateManager.CheckForUpdate(ConfigManager.Get<string>(VRCOSCSetting.UpdateRepo));

        var lastVersion = ConfigManager.Get<string>(VRCOSCSetting.Version);

        if (Version != lastVersion)
        {
            if (!string.IsNullOrEmpty(lastVersion))
            {
                notificationContainer.Notify(new BasicNotification
                {
                    Title = "VRCOSC Updated",
                    Description = "Click to see the changes",
                    Icon = FontAwesome.Solid.Download,
                    Colour = VRCOSCColour.GreenDark,
                    ClickCallback = () => host.OpenUrlExternally("https://github.com/VolcanicArts/VRCOSC/releases/latest"),
                });
            }
        }

        ConfigManager.SetValue(VRCOSCSetting.Version, Version);

        notificationContainer.Notify(new TimedNotification
        {
            Title = "Discord Server",
            Description = "Click to join the Discord server",
            Icon = FontAwesome.Brands.Discord,
            Colour = Colour4.FromHex(@"7289DA"),
            ClickCallback = () => host.OpenUrlExternally("https://discord.gg/vj4brHyvT5"),
            Delay = 5000d
        });
    }

    protected abstract VRCOSCUpdateManager CreateUpdateManager();
}
