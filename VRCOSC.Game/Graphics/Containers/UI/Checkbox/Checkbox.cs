// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;

namespace VRCOSC.Game.Graphics.Containers.UI.Checkbox;

public class Checkbox : Button
{
    public BindableBool State { get; init; } = new();

    [BackgroundDependencyLoader]
    private void load()
    {
        Masking = true;

        Box background;
        Children = new Drawable[]
        {
            background = new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both
            },
            new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(8),
                Child = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Icon = FontAwesome.Solid.Check,
                    Shadow = true
                }
            }
        };

        State.BindValueChanged(e =>
        {
            background.Colour = e.NewValue ? VRCOSCColour.Green : VRCOSCColour.Red;
        }, true);
    }

    protected override bool OnHover(HoverEvent e)
    {
        this.ScaleTo(1.05f, 100, Easing.OutCirc);
        return true;
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        this.ScaleTo(1.0f, 100, Easing.OutCirc);
    }

    protected override bool OnClick(ClickEvent e)
    {
        Action?.Invoke();
        State.Toggle();
        if (IsHovered) this.ScaleTo(1.05f, 100, Easing.OutCirc);
        return true;
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        this.ScaleTo(0.9f, 1000, Easing.OutSine);
        return true;
    }

    protected override void OnMouseUp(MouseUpEvent e)
    {
        this.ScaleTo(1.0f, 250, Easing.OutElastic);
    }
}
