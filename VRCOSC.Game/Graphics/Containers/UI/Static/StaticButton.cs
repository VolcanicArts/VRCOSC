// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;

namespace VRCOSC.Game.Graphics.Containers.UI.Static;

public class StaticButton : VRCOSCButton
{
    public Colour4 BackgroundColour { get; init; } = VRCOSCColour.BlueDark;

    [BackgroundDependencyLoader]
    private void load()
    {
        InternalChild = CreateBackground();
    }

    public virtual Drawable CreateBackground()
    {
        return new Box
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Colour = BackgroundColour
        };
    }
}
