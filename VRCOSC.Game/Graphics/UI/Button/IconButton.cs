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
    private IconUsage iconStateOff = FontAwesome.Solid.PowerOff;
    private IconUsage iconStateOn = FontAwesome.Solid.PowerOff;

    private SpriteIcon spriteIcon = null!;

    public IconUsage Icon
    {
        get => iconStateOff;
        set
        {
            iconStateOff = value;
            iconStateOn = value;
            updateIcon();
        }
    }

    public IconUsage IconStateOff
    {
        get => iconStateOff;
        set
        {
            iconStateOff = value;
            updateIcon();
        }
    }

    public IconUsage IconStateOn
    {
        get => iconStateOn;
        set
        {
            iconStateOn = value;
            updateIcon();
        }
    }

    public int IconPadding { get; init; } = 8;

    public bool IconShadow { get; init; }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            FillMode = FillMode.Fit,
            Padding = new MarginPadding(IconPadding),
            Child = spriteIcon = createSpriteIcon()
        });

        State.BindValueChanged(_ => updateIcon(), true);
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        updateIcon();
    }

    private void updateIcon()
    {
        if (!IsLoaded) return;

        if (Stateful)
            spriteIcon.Icon = State.Value ? iconStateOn : iconStateOff;
        else
            spriteIcon.Icon = iconStateOff;
    }

    private SpriteIcon createSpriteIcon() => new()
    {
        Anchor = Anchor.Centre,
        Origin = Anchor.Centre,
        RelativeSizeAxes = Axes.Both,
        Shadow = IconShadow,
        Colour = ThemeManager.Current[ThemeAttribute.Text]
    };
}
