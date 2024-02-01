// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Graphics;
using VRCOSC.Graphics.UI.List;

namespace VRCOSC.Screens.Main.Run;

public partial class DrawableParameter : HeightLimitedScrollableListItem
{
    public readonly string ParameterAddress;
    private readonly object parameterInitialValue;

    private SpriteText valueSpriteText = null!;
    private Box flashBackground = null!;
    private object? currentValue;

    public DrawableParameter(string parameterAddress, object parameterInitialValue)
    {
        ParameterAddress = parameterAddress;
        this.parameterInitialValue = parameterInitialValue;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            flashBackground = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.Transparent
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
                        Text = ParameterAddress,
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
        currentValue = value;
    }

    protected override void Update()
    {
        if (!IsLoaded || currentValue is null || valueSpriteText.Text == (currentValue?.ToString() ?? "INVALID")) return;

        valueSpriteText.Text = currentValue?.ToString() ?? "INVALID";
        flashBackground.FlashColour(Colours.WHITE0.Opacity(0.5f), 1000, Easing.OutQuad);
    }
}
