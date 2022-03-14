using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace VRCOSC.Game.Graphics.Drawables.Triangles;

public class TrianglesBackground : CompositeDrawable
{
    protected internal Color4 ColourLight { get; init; } = VRCOSCColour.Blue;
    protected internal Color4 ColourDark { get; init; } = VRCOSCColour.BlueDark;
    protected internal float Velocity { get; init; } = 1;
    protected internal float TriangleScale { get; init; } = 1;

    [BackgroundDependencyLoader]
    private void load()
    {
        InternalChildren = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = ColourDark
            },
            new Triangles
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ColourLight = ColourLight,
                ColourDark = ColourDark,
                Velocity = Velocity,
                TriangleScale = TriangleScale
            }
        };
    }
}
