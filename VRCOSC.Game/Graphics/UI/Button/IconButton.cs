// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace VRCOSC.Game.Graphics.UI.Button;

public class IconButton : BasicButton
{
    private IconUsage iconStateOff = FontAwesome.Solid.PowerOff;
    private IconUsage iconStateOn = FontAwesome.Solid.PowerOff;

    protected SpriteIcon SpriteIcon = null!;

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
        get => IconStateOn;
        set
        {
            iconStateOn = value;
            updateIcon();
        }
    }

    public int IconPadding { get; init; } = 8;

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
            Child = SpriteIcon = CreateSpriteIcon()
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
            SpriteIcon.Icon = State.Value ? iconStateOn : iconStateOff;
        else
            SpriteIcon.Icon = iconStateOff;
    }

    protected virtual SpriteIcon CreateSpriteIcon()
    {
        return new SpriteIcon
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both
        };
    }
}
