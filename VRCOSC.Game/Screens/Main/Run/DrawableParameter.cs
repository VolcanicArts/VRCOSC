// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
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

    public Bindable<bool> Even = new();

    private Box background = null!;
    private SpriteText valueSpriteText = null!;

    public DrawableParameter(string parameterName, object parameterInitialValue)
    {
        this.parameterName = parameterName;
        this.parameterInitialValue = parameterInitialValue;
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
            background = new Box
            {
                RelativeSizeAxes = Axes.Both
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

        Even.BindValueChanged(e => background.Colour = e.NewValue ? Colours.GRAY1 : Colours.GRAY2, true);
    }

    public void UpdateValue(object value)
    {
        valueSpriteText.Text = value.ToString() ?? "INVALID";
        background.FlashColour(Colours.WHITE0.Opacity(0.5f), 500, Easing.OutQuint);
    }
}
