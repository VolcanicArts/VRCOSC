// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.UI.Button;

public partial class IconButton : BasicButton
{
    private int iconPadding = 8;

    private SpriteIcon spriteIcon = null!;
    private Container? wrapper;

    public IconUsage? Icon
    {
        get => IconStateOff;
        set
        {
            IconStateOff = value;
            IconStateOn = value;
        }
    }

    public IconUsage? IconStateOff { get; set; } = FontAwesome.Solid.PowerOff;
    public IconUsage? IconStateOn { get; set; } = FontAwesome.Solid.PowerOff;

    public int IconPadding
    {
        get => iconPadding;
        set
        {
            iconPadding = value;
            if (wrapper is null) return;

            wrapper.Padding = new MarginPadding(iconPadding);
        }
    }

    public bool IconShadow { get; init; }

    public IconButton()
    {
        CornerRadius = 5;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(wrapper = new Container
        {
            RelativeSizeAxes = Axes.Both,
            Padding = new MarginPadding(IconPadding),
            Child = spriteIcon = new SpriteIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                Shadow = IconShadow,
                Colour = ThemeManager.Current[ThemeAttribute.Text]
            }
        });
    }

    protected override void Update()
    {
        base.Update();

        if (Stateful)
            setIcon(State.Value ? IconStateOn : IconStateOff);
        else
            setIcon(IconStateOn);
    }

    private void setIcon(IconUsage? iconUsage)
    {
        if (iconUsage is null)
        {
            spriteIcon.FadeTo(0, 150, Easing.OutQuint);
        }
        else
        {
            spriteIcon.Icon = iconUsage.Value;
            spriteIcon.FadeTo(1, 150, Easing.OutQuint);
        }
    }
}
