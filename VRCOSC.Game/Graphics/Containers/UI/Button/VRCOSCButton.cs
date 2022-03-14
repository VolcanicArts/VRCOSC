using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osuTK.Graphics;
using VRCOSC.Game.Graphics.Drawables.Triangles;

namespace VRCOSC.Game.Graphics.Containers.UI;

public class VRCOSCButton : Button
{
    protected internal Color4 BackgroundColour { get; init; } = VRCOSCColour.BlueDark;

    [BackgroundDependencyLoader]
    private void load()
    {
        Masking = true;
        EdgeEffect = VRCOSCEdgeEffects.NoShadow;

        InternalChild = new TrianglesBackground
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            ColourDark = BackgroundColour,
            ColourLight = BackgroundColour.Lighten(0.2f),
            Velocity = 0.2f,
            TriangleScale = 0.5f
        };
    }

    protected override bool OnClick(ClickEvent e)
    {
        Action?.Invoke();
        if (!IsHovered) return true;

        this.MoveToY(-1.5f, 100, Easing.InCubic);
        TweenEdgeEffectTo(VRCOSCEdgeEffects.BasicShadow, 100, Easing.InCubic);
        return true;
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        this.MoveToY(0, 100, Easing.OutCubic);
        TweenEdgeEffectTo(VRCOSCEdgeEffects.NoShadow, 100, Easing.OutCubic);
        return true;
    }

    protected override bool OnHover(HoverEvent e)
    {
        this.MoveToY(-1.5f, 100, Easing.InCubic);
        TweenEdgeEffectTo(VRCOSCEdgeEffects.BasicShadow, 100, Easing.InCubic);
        return true;
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        this.MoveToY(0, 100, Easing.OutCubic);
        TweenEdgeEffectTo(VRCOSCEdgeEffects.NoShadow, 100, Easing.OutCubic);
        base.OnHoverLost(e);
    }
}
