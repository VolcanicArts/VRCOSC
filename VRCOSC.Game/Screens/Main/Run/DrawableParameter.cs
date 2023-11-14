// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Screens.Main.Run;

public partial class DrawableParameter : Container
{
    private readonly string parameterName;
    private readonly object parameterInitialValue;
    private readonly bool even;

    private SpriteText valueSpriteText = null!;

    public DrawableParameter(string parameterName, object parameterInitialValue, bool even)
    {
        this.parameterName = parameterName;
        this.parameterInitialValue = parameterInitialValue;
        this.even = even;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = even ? Colours.GRAY1 : Colours.GRAY2
            },
            new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding
                {
                    Vertical = 2,
                    Horizontal = 5
                },
                Children = new Drawable[]
                {
                    new SpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Text = parameterName,
                        Font = Fonts.REGULAR.With(size: 20),
                        Colour = Colours.WHITE2
                    },
                    valueSpriteText = new SpriteText
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Font = Fonts.REGULAR.With(size: 20),
                        Colour = Colours.WHITE2,
                        Text = parameterInitialValue.ToString() ?? "INVALID"
                    }
                }
            }
        };
    }

    public void UpdateValue(object value)
    {
        valueSpriteText.Text = value.ToString() ?? "INVALID";
    }
}
