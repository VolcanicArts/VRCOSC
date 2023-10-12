// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osuTK;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Screens.Loading;

public partial class LoadingScreenSliderBar : SliderBar<float>
{
    private Box box = null!;
    private Box selectionBox = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Masking = true;
        CornerRadius = 2;

        Children = new Drawable[]
        {
            box = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.Mid
            },
            selectionBox = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.Highlight,
                Scale = new Vector2(0, 1)
            }
        };
    }

    protected override void UpdateValue(float value)
    {
        selectionBox.ScaleTo(new Vector2(value, 1), 300, Easing.OutQuint);
    }
}
