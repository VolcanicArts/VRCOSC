// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace VRCOSC.Game.Graphics.Containers.Screens.TerminalScreen;

public class TerminalEntry : Container
{
    public string Text { get; init; }

    [BackgroundDependencyLoader]
    private void load()
    {
        Alpha = 0;

        InternalChild = new SpriteText
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            Font = FrameworkFont.Regular.With(size: 20),
            Colour = VRCOSCColour.Gray8,
            Text = Text
        };
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        this.FadeInFromZero(500, Easing.OutCirc);
    }
}
