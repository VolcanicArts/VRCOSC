// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Screens.Main.Tabs;

public sealed partial class DrawableTab : ClickableContainer
{
    private static readonly Colour4 default_colour = Colours.GRAY0;
    private static readonly Colour4 hover_colour = Colours.GRAY5;
    private static readonly Colour4 selected_colour = Colours.GRAY1;

    private const int onhover_duration = 250;
    private const int onhoverlost_duration = onhover_duration;

    private Box background = null!;

    public Tab Tab { get; init; }
    public IconUsage Icon { get; init; }

    [Resolved]
    private VRCOSCGame game { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;
        FillMode = FillMode.Fit;

        Children = new Drawable[]
        {
            background = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = default_colour
            },
            new SpriteIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.35f),
                FillMode = FillMode.Fit,
                Icon = Icon,
                Colour = Colours.WHITE0,
                Shadow = true,
                ShadowOffset = new Vector2(0, 1)
            }
        };
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        game.SelectedTab.BindValueChanged(tab =>
        {
            if (tab.NewValue == Tab)
            {
                background.FadeColour(IsHovered ? hover_colour : selected_colour, onhover_duration, Easing.OutQuart);
            }
            else
            {
                background.FadeColour(IsHovered ? hover_colour : default_colour, onhoverlost_duration, Easing.OutQuart);
            }
        }, true);

        Action += () => game.SelectedTab.Value = Tab;
    }

    protected override bool OnHover(HoverEvent e)
    {
        background.FadeColour(hover_colour, onhover_duration, Easing.OutQuart);
        return base.OnHover(e);
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        base.OnHoverLost(e);
        background.FadeColour(game.SelectedTab.Value == Tab ? selected_colour : default_colour, onhoverlost_duration, Easing.OutQuart);
    }
}
