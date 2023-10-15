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
    private static readonly Colour4 default_colour = Colours.Dark;
    private static readonly Colour4 hover_colour = Colours.Light;

    private const int onhover_duration = 250;
    private const int onhoverlost_duration = onhover_duration;

    private Box background = null!;
    private SelectedIndicator indicator = null!;
    private SpriteIcon spriteIcon = null!;

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
            spriteIcon = new SpriteIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.35f),
                FillMode = FillMode.Fit,
                Icon = Icon,
                Colour = Colours.Highlight
            },
            indicator = new SelectedIndicator
            {
                RelativeSizeAxes = Axes.Both
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
                indicator.Select();
                spriteIcon.Colour = Colours.Light;
            }
            else
            {
                indicator.DeSelect();
                background.FadeColour(default_colour, onhoverlost_duration, Easing.InOutSine);
                spriteIcon.Colour = Colours.OffWhite;
            }
        }, true);

        Action += () => game.SelectedTab.Value = Tab;
    }

    protected override bool OnHover(HoverEvent e)
    {
        background.FadeColour(hover_colour, onhover_duration, Easing.InOutSine);

        if (game.SelectedTab.Value == Tab) return base.OnHover(e);

        indicator.PreSelect();
        return base.OnHover(e);
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        base.OnHoverLost(e);

        background.FadeColour(default_colour, onhoverlost_duration, Easing.InOutSine);

        if (game.SelectedTab.Value == Tab) return;

        indicator.DeSelect();
    }

    private partial class SelectedIndicator : Container
    {
        public SelectedIndicator()
        {
            Child = new CircularContainer
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.1f, 0f),
                Masking = true,
                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colours.Highlight
                }
            };
        }

        public void Select()
        {
            Child.ResizeTo(new Vector2(0.1f, 0.75f), 100, Easing.OutQuad);
        }

        public void DeSelect()
        {
            Child.ResizeTo(Vector2.Zero, 200, Easing.OutQuad);
        }

        public void PreSelect()
        {
            Child.ResizeTo(new Vector2(0.1f, 0.25f), 200, Easing.OutQuad);
        }
    }
}
