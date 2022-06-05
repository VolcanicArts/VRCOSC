// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osuTK.Graphics;

namespace VRCOSC.Game.Graphics.Containers.UI;

public class VRCOSCButton : Button
{
    protected internal Bindable<Color4> BackgroundColour { get; } = new(VRCOSCColour.BlueDark);

    [BackgroundDependencyLoader]
    private void load()
    {
        Enabled.Value = true;
        Masking = true;

        Box background;
        InternalChild = background = new Box
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both
        };

        BackgroundColour.BindValueChanged(e =>
        {
            background.Colour = e.NewValue;
        }, true);
    }

    protected override bool OnHover(HoverEvent e)
    {
        if (Enabled.Value)
        {
            this.ScaleTo(1.05f, 100, Easing.OutCirc);
        }

        return true;
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        if (Enabled.Value)
        {
            this.ScaleTo(1.0f, 100, Easing.OutCirc);
        }
    }

    protected override bool OnClick(ClickEvent e)
    {
        if (Enabled.Value)
        {
            Action?.Invoke();
            if (IsHovered) this.ScaleTo(1.05f, 100, Easing.OutCirc);
        }

        return true;
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        if (Enabled.Value)
        {
            this.ScaleTo(0.9f, 1000, Easing.OutSine);
        }

        return true;
    }

    protected override void OnMouseUp(MouseUpEvent e)
    {
        if (Enabled.Value)
        {
            this.ScaleTo(1.0f, 250, Easing.OutElastic);
        }
    }
}
