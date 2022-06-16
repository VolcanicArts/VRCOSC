// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;

namespace VRCOSC.Game.Graphics.Containers.UI.Dynamic;

public class DropdownButton : StatefulIconButton
{
    private SpriteIcon spriteIcon;

    public DropdownButton()
    {
        IconStateTrue = FontAwesome.Solid.ArrowCircleRight;
        IconStateFalse = FontAwesome.Solid.ArrowCircleRight;
        BackgroundColourStateTrue = VRCOSCColour.Invisible;
        BackgroundColourStateFalse = VRCOSCColour.Invisible;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        State.BindValueChanged(_ => spriteIcon.RotateTo(State.Value ? 90 : 0, 400, Easing.OutElastic), true);
    }

    protected override SpriteIcon CreateSpriteIcon()
    {
        return spriteIcon = new SpriteIcon
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both
        };
    }
}
