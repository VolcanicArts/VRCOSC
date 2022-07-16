// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.Sidebar;
using VRCOSC.Game.Graphics.Updater;

namespace VRCOSC.Game;

[Cached]
public abstract class VRCOSCGame : VRCOSCGameBase
{
    private DependencyContainer dependencies;

    public Bindable<Tabs> SelectedTab = new();

    [BackgroundDependencyLoader]
    private void load()
    {
        Child = new GridContainer
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
                    new Sidebar()
                }
            }
        };
    }

    protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
    {
        return dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
    }

    public abstract VRCOSCUpdateManager CreateUpdateManager();
}
