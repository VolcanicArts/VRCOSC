// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.UI.Button;

public sealed partial class ToggleButton : VRCOSCButton
{
    public BindableBool State { get; init; } = new();

    public ToggleButton()
    {
        Masking = true;
        CornerRadius = 5;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        SpriteIcon icon;
        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = ThemeManager.Current[ThemeAttribute.Mid]
            },
            new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(5),
                Child = icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Icon = FontAwesome.Solid.Check,
                    Colour = ThemeManager.Current[ThemeAttribute.Text],
                    Alpha = State.Value ? 1 : 0
                }
            }
        };

        State.BindValueChanged(e => icon.FadeTo(e.NewValue ? 1 : 0, 200, Easing.OutQuart));
    }

    protected override bool OnClick(ClickEvent e)
    {
        State.Toggle();
        return base.OnClick(e);
    }
}
