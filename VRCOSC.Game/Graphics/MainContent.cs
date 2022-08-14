// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
//using VRCOSC.Game.Graphics.About;
using VRCOSC.Game.Graphics.ModuleListing;
using VRCOSC.Game.Graphics.Settings;
using VRCOSC.Game.Graphics.Sidebar;

namespace VRCOSC.Game.Graphics;

public class MainContent : Container
{
    [Resolved]
    private VRCOSCGame game { get; set; } = null!;

    private Container screenHolder = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
        Masking = true;

        Children = new Drawable[]
        {
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
                                new SettingsScreen(),
                                //new AboutContainer()
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

        game.SelectedTab.BindValueChanged(tab =>
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
