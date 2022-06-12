// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace VRCOSC.Game.Graphics.Containers.UI.Button;

public class IconButton : VRCOSCButton
{
    protected internal Bindable<IconUsage> Icon { get; set; } = new(FontAwesome.Solid.Check);
    public int IconPadding { get; init; } = 8;

    [BackgroundDependencyLoader]
    private void load()
    {
        SpriteIcon spriteIcon;
        AddInternal(new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            FillMode = FillMode.Fit,
            Padding = new MarginPadding(IconPadding),
            Child = spriteIcon = new SpriteIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Shadow = true
            }
        });

        Icon.BindValueChanged(e =>
        {
            spriteIcon.Icon = e.NewValue;
        }, true);

        Enabled.BindValueChanged(_ => updateAlpha(), true);
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        updateAlpha();
    }

    private void updateAlpha()
    {
        if (Enabled.Value)
        {
            this.FadeTo(1, 500, Easing.OutCirc);
        }
        else
        {
            this.FadeTo(0.5f, 500, Easing.OutCirc);
        }
    }
}
