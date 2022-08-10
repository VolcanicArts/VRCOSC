// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using osu.Framework.Input.Events;

namespace VRCOSC.Game.Graphics.UI.Button;

public class VRCOSCButton : osu.Framework.Graphics.UserInterface.Button
{
    private const float scale_default = 1.0f;
    private const float scale_hovered = 1.05f;
    private const float scale_mousedown = 0.9f;
    private const float alpha_enabled = 1.0f;
    private const float alpha_disabled = 0.5f;

    public bool ShouldAnimate { get; init; } = true;

    public VRCOSCButton()
    {
        Masking = true;
        Enabled.Value = true;
        Enabled.BindValueChanged(_ => this.FadeTo(Enabled.Value ? alpha_enabled : alpha_disabled, 500, Easing.OutCirc), true);
    }

    protected override bool OnHover(HoverEvent e)
    {
        if (Enabled.Value && ShouldAnimate) this.ScaleTo(scale_hovered, 100, Easing.OutCirc);
        return true;
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        if (Enabled.Value && ShouldAnimate) this.ScaleTo(scale_default, 100, Easing.OutCirc);
    }

    protected override bool OnClick(ClickEvent e)
    {
        if (Enabled.Value && ShouldAnimate) this.ScaleTo(IsHovered ? scale_hovered : scale_default, 500, Easing.OutElastic);
        return base.OnClick(e);
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        if (Enabled.Value && ShouldAnimate) this.ScaleTo(scale_mousedown, 1000, Easing.OutSine);
        return true;
    }
}
