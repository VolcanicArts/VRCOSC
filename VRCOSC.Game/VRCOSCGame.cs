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
using VRCOSC.Game.Graphics.Sidebar;
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

    public Bindable<Tabs> SelectedTab = new();
    public Bindable<string> SearchTermFilter = new(string.Empty);
    public Bindable<ModuleType?> TypeFilter = new();
    public Bindable<Module?> EditingModule = new();
    public BindableBool ModulesRunning = new();

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
            if (e.NewValue) SelectedTab.Value = Tabs.Modules;
        }, true);

        var updateMode = ConfigManager.Get<UpdateMode>(VRCOSCSetting.UpdateMode);

        if (updateMode != UpdateMode.Off) UpdateManager.CheckForUpdate();

        notificationContainer.Notify(new TimedNotification
        {
            Title = "Discord Server",
            Description = "Click to join the Discord server",
            Icon = FontAwesome.Brands.Discord,
            Colour = Colour4.FromHex(@"7289DA"),
            ClickCallback = () => host.OpenUrlExternally("https://discord.gg/vj4brHyvT5"),
            Delay = 7500d
        });

        notificationContainer.Notify(new TimedNotification
        {
            Title = "Prefab Updates",
            Description = "Remember to check for prefab updates!",
            Icon = FontAwesome.Solid.ExclamationTriangle,
            Colour = VRCOSCColour.YellowDark,
            ClickCallback = () => host.OpenUrlExternally("https://github.com/VolcanicArts/VRCOSC/releases/latest"),
            Delay = 7500d
        });
    }

    protected abstract VRCOSCUpdateManager CreateUpdateManager();
}
