// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace VRCOSC.Game.Graphics.Containers.UI.Dynamic;

public class StatefulIconButton : StatefulButton
{
    public IconUsage IconStateTrue { get; init; } = FontAwesome.Solid.Check;
    public IconUsage IconStateFalse { get; init; } = FontAwesome.Solid.Get(0xf00d);

    public int IconPadding { get; init; } = 8;

    [BackgroundDependencyLoader]
    private void load()
    {
        SpriteIcon spriteIcon;

        Add(new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            FillMode = FillMode.Fit,
            Padding = new MarginPadding(IconPadding),
            Child = spriteIcon = CreateSpriteIcon()
        });

        State.BindValueChanged(e => spriteIcon.Icon = State.Value ? IconStateTrue : IconStateFalse, true);
    }

    protected virtual SpriteIcon CreateSpriteIcon()
    {
        return new SpriteIcon
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Shadow = true
        };
    }
}
