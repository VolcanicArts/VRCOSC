using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using VRCOSC.Game.Graphics.EdgeEffects;

namespace VRCOSC.Game.Graphics.Drawables.UI.Buttons;

public class IconButton : Button
{
    public new Colour4 Colour { get; init; }
    public IconUsage Icon { get; init; }

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
                Colour = Colour
            },
            new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(8),
                Child = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Icon = Icon,
                    Colour = Colour4.Black
                }
            }
        };
    }

    protected override bool OnClick(ClickEvent e)
    {
        Action?.Invoke();
        if (!IsHovered) return true;

        this.MoveToY(-1.5f, 100, Easing.InCubic);
        TweenEdgeEffectTo(BasicEdgeEffects.BasicShadow, 100, Easing.InCubic);
        return true;
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        this.MoveToY(0, 100, Easing.OutCubic);
        TweenEdgeEffectTo(BasicEdgeEffects.NoShadow, 100, Easing.OutCubic);
        return true;
    }

    protected override bool OnHover(HoverEvent e)
    {
        this.MoveToY(-1.5f, 100, Easing.InCubic);
        TweenEdgeEffectTo(BasicEdgeEffects.BasicShadow, 100, Easing.InCubic);
        return true;
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        this.MoveToY(0, 100, Easing.OutCubic);
        TweenEdgeEffectTo(BasicEdgeEffects.NoShadow, 100, Easing.OutCubic);
        base.OnHoverLost(e);
    }
}
