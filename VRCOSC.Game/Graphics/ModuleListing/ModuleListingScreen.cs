// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleListing;

[Cached]
public sealed class ModuleListingScreen : Container
{
    public Bindable<string> SearchTermFilter = new(string.Empty);
    public Bindable<ModuleType?> TypeFilter = new();
    public Bindable<SortFilter> SortFilter = new(new SortFilter());

    public ModuleListingScreen()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = VRCOSCColour.Gray5
            },
            new GridContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 50),
                    new Dimension(),
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new Header
                        {
                            Depth = float.MinValue
                        }
                    },
                    new Drawable[]
                    {
                        new Listing()
                    }
                }
            }
        };
    }
}
