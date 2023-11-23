// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Graphics;

namespace VRCOSC.Game.Graphics.UI;

public partial class CheckBox : Button
{
    public Bindable<bool> State = new();

    public Color4 BackgroundColour { get; init; } = Colours.GRAY2;
    public IconUsage Icon { get; init; } = FontAwesome.Solid.Check;
    public float IconSize = 25;

    private readonly Color4 iconColour = Color4.White;
    private Box background = null!;
    private SpriteIcon icon = null!;

    public CheckBox()
    {
        BorderColour = Colours.GRAY5;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Masking = true;
        CornerRadius = 5;
        BorderThickness = 4;
        BorderColour = BorderColour;

        Children = new Drawable[]
        {
            background = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = BackgroundColour,
                AlwaysPresent = true
            },
            icon = new SpriteIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Icon = Icon,
                Size = new Vector2(IconSize),
                Colour = iconColour,
                Shadow = true,
                ShadowColour = Colours.BLACK.Opacity(0.25f),
                ShadowOffset = new Vector2(0, 1)
            }
        };

        State.BindValueChanged(stateOnValueChanged, true);
    }

    protected override bool OnClick(ClickEvent e)
    {
        State.Value = !State.Value;
        return true;
    }

    private void stateOnValueChanged(ValueChangedEvent<bool> e)
    {
        if (e.NewValue)
            enable();
        else
            disable();
    }

    private void enable()
    {
        background.FadeInFromZero(100, Easing.OutQuart);
        icon.FadeInFromZero(100, Easing.OutQuart);
        this.TransformTo(nameof(BorderThickness), 0f, 100, Easing.OutQuart);
    }

    private void disable()
    {
        background.FadeOutFromOne(100, Easing.OutQuart);
        icon.FadeOutFromOne(100, Easing.OutQuart);
        this.TransformTo(nameof(BorderThickness), 4f, 100, Easing.OutQuart);
    }
}
