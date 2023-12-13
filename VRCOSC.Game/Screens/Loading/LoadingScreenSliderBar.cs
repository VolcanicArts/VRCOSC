// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osuTK;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Screens.Loading;

public partial class LoadingScreenSliderBar : SliderBar<float>
{
    public Bindable<string> TextCurrent { get; init; } = new(string.Empty);

    private Box selectionBox = null!;

    public override bool HandlePositionalInput => false;

    [BackgroundDependencyLoader]
    private void load()
    {
        Masking = true;
        CornerRadius = 10;
        BorderThickness = 4;
        BorderColour = Colours.GRAY2;

        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY5
            },
            selectionBox = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY3,
                Scale = new Vector2(0, 1)
            },
            new SpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = FrameworkFont.Regular.With(size: 15),
                Colour = Colours.WHITE2,
                Current = TextCurrent,
                Shadow = true,
                ShadowOffset = new Vector2(0, 0.05f)
            }
        };
    }

    protected override void UpdateValue(float value)
    {
        selectionBox.ScaleTo(new Vector2(value, 1), 300, Easing.OutQuint);
    }
}
