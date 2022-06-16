// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace VRCOSC.Game.Graphics.Containers.UI.Static;

public class IconButton : StaticButton
{
    public IconUsage Icon { get; init; }
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
            Child = CreateSpriteIcon()
        });
    }

    protected virtual SpriteIcon CreateSpriteIcon()
    {
        return new SpriteIcon
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Shadow = true,
            Icon = Icon
        };
    }
}
