// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.About;
using VRCOSC.Game.Graphics.ModuleListing;
using VRCOSC.Game.Graphics.Settings;
using VRCOSC.Game.Graphics.Sidebar;

namespace VRCOSC.Game.Graphics;

public sealed class MainContent : Container
{
    [Resolved]
    private Bindable<Tabs> selectedTab { get; set; } = null!;

    private Container screenHolder = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;
        Masking = true;

        Children = new Drawable[]
        {
            new GridContainer
            {
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
                            RelativeSizeAxes = Axes.Both
                        },
                        screenHolder = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Depth = float.MaxValue,
                            Children = new Drawable[]
                            {
                                new ModuleListingScreen(),
                                new SettingsScreen(),
                                new AboutScreen()
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

        selectedTab.BindValueChanged(tab =>
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
}
