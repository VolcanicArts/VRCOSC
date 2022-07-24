// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.ModuleListing;
using VRCOSC.Game.Graphics.Settings;
using VRCOSC.Game.Graphics.Sidebar;
using VRCOSC.Game.Graphics.Updater;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game;

[Cached]
public abstract class VRCOSCGame : VRCOSCGameBase
{
    private Container screenHolder = null!;

    [Cached]
    private ModuleManager moduleManager = new();

    public Bindable<Tabs> SelectedTab = new();

    public Bindable<string> SearchTermFilter = new(string.Empty);
    public Bindable<ModuleType?> TypeFilter = new();
    public Bindable<Module?> EditingModule = new();

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            moduleManager,
            new GridContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 100),
                    new Dimension(),
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new TabBar
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both
                        },
                        screenHolder = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Depth = float.MaxValue,
                            Children = new Drawable[]
                            {
                                new ModuleListingScreen(),
                                new SettingsScreen()
                            }
                        }
                    }
                }
            }
        };
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        SelectedTab.BindValueChanged(tab =>
        {
            var id = (int)tab.NewValue;

            for (int i = 0; i < screenHolder.Count; i++)
            {
                if (id == i)
                    screenHolder[i].Show();
                else
                    screenHolder[i].Hide();
            }
        }, true);
    }

    public abstract VRCOSCUpdateManager CreateUpdateManager();
}
