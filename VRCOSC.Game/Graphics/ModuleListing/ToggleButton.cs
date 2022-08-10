// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using VRCOSC.Game.Graphics.UI.Button;

namespace VRCOSC.Game.Graphics.ModuleListing;

public class ToggleButton : VRCOSCButton
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
                Colour = VRCOSCColour.Gray4
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
                    Icon = FontAwesome.Solid.Check
                }
            }
        };

        State.BindValueChanged(e => icon.Alpha = e.NewValue ? 1 : 0, true);
    }

    protected override bool OnClick(ClickEvent e)
    {
        State.Toggle();
        return base.OnClick(e);
    }
}
