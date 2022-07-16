// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;

namespace VRCOSC.Game.Graphics.Sidebar;

public sealed class Sidebar : Container
{
    private FillFlowContainer<Tab> tabsFlow = null!;

    public Sidebar()
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
                Colour = VRCOSCColour.Gray3
            },
            tabsFlow = new FillFlowContainer<Tab>
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Children = new[]
                {
                    new Tab()
                    {
                        Icon = FontAwesome.Solid.List
                    },
                    new Tab()
                    {
                        Icon = FontAwesome.Solid.Cog
                    }
                }
            }
        };
    }
}
