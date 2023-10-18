// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Screens.Main.Repo;

public partial class LoadingContainer : Container
{
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
                Colour = Colours.Black.Opacity(0.75f)
            },
            new LoadingCircle()
        };
    }
}

public partial class LoadingCircle : CircularContainer
{
    [BackgroundDependencyLoader]
    private void load()
    {
        Masking = true;
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativePositionAxes = Axes.X;
        Size = new Vector2(10);
        X = -0.5f;

        Child = new Box
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Colour = Colours.OffWhite
        };

        this.MoveToX(0.5f, 2000, Easing.InOutQuart).Then().MoveToX(-0.5f, 2000, Easing.InOutQuart).Loop();
    }
}
