// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.TabBar;

public sealed partial class DrawableTab : ClickableContainer
{
    private static readonly Colour4 default_colour = new(0, 0, 0, 0);
    private static readonly Colour4 hover_colour = ThemeManager.Current[ThemeAttribute.Light];

    private const int onhover_duration = 250;
    private const int onhoverlost_duration = onhover_duration;

    private Box background = null!;
    private SelectedIndicator indicator = null!;
    private SpriteIcon spriteIcon = null!;

    public Tab Tab { get; init; }
    public IconUsage Icon { get; init; }

    [Resolved]
    private Bindable<Tab> selectedTab { get; set; } = null!;

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
                Colour = ThemeManager.Current[ThemeAttribute.Text]
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

        selectedTab.BindValueChanged(tab =>
        {
            if (tab.NewValue == Tab)
            {
                indicator.Select();
                spriteIcon.Colour = ThemeManager.Current[ThemeAttribute.Accent];
            }
            else
            {
                indicator.DeSelect();
                background.FadeColour(default_colour, onhoverlost_duration, Easing.InOutSine);
                spriteIcon.Colour = ThemeManager.Current[ThemeAttribute.Text];
            }
        }, true);

        Action += () => selectedTab.Value = Tab;
    }

    protected override bool OnHover(HoverEvent e)
    {
        background.FadeColour(hover_colour, onhover_duration, Easing.InOutSine);

        if (selectedTab.Value == Tab) return base.OnHover(e);

        indicator.PreSelect();
        return base.OnHover(e);
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        base.OnHoverLost(e);

        background.FadeColour(default_colour, onhoverlost_duration, Easing.InOutSine);

        if (selectedTab.Value == Tab) return;

        indicator.DeSelect();
    }
}
