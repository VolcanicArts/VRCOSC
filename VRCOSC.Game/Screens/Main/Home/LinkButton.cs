// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osuTK;
using osuTK.Graphics;
using VRCOSC.Graphics.UI;

namespace VRCOSC.Screens.Main.Home;

public partial class LinkButton : VRCOSCButton
{
    [Resolved]
    private GameHost host { get; set; } = null!;

    private readonly string link;
    private readonly IconUsage icon;
    private readonly Color4 iconColour;

    public LinkButton(string link, IconUsage icon, Color4 iconColour)
    {
        this.link = link;
        this.icon = icon;
        this.iconColour = iconColour;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.CentreRight;
        Origin = Anchor.CentreRight;
        RelativeSizeAxes = Axes.Both;
        FillMode = FillMode.Fit;
        CornerRadius = 10;

        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = iconColour
            },
            new SpriteIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.7f),
                Icon = icon,
                Shadow = true
            }
        };

        Action += () => host.OpenUrlExternally(link);
    }
}
