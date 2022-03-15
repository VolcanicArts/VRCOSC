using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace VRCOSC.Game.Graphics.Drawables;

public class LineSeparator : CircularContainer
{
    [BackgroundDependencyLoader]
    private void load()
    {
        Masking = true;
        InternalChild = new Box
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Colour = VRCOSCColour.Gray1,
        };
    }
}
