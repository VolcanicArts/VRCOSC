// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Input;

namespace VRCOSC.Game.Graphics.UI.Button;

public partial class VRCOSCButton : osu.Framework.Graphics.UserInterface.Button
{
    private const float alpha_enabled = 1.0f;
    private const float alpha_disabled = 0.5f;

    public bool ShouldAnimate { get; init; } = true;
    public bool Circular { get; init; }
    public Vector2 HoverOffset { get; init; } = new(0, -2);

    public override bool HandlePositionalInput => Enabled.Value;

    protected VRCOSCButton()
    {
        Masking = true;
        Enabled.Value = true;
        Enabled.BindValueChanged(_ => this.FadeTo(Enabled.Value ? alpha_enabled : alpha_disabled, 500, Easing.OutCirc), true);
        EdgeEffect = VRCOSCEdgeEffects.NoShadow;
    }

    protected override void UpdateAfterAutoSize()
    {
        if (Circular) CornerRadius = Math.Min(DrawSize.X, DrawSize.Y) / 2f;
        base.UpdateAfterAutoSize();
    }

    protected override bool OnHover(HoverEvent e)
    {
        if (ShouldAnimate)
        {
            this.TransformTo(nameof(EdgeEffect), VRCOSCEdgeEffects.HoverShadow, 100, Easing.OutCirc);
            this.MoveTo(HoverOffset, 100, Easing.OutCirc);
        }

        return true;
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        if (ShouldAnimate)
        {
            this.TransformTo(nameof(EdgeEffect), VRCOSCEdgeEffects.NoShadow, 100, Easing.OutCirc);
            this.MoveTo(Vector2.Zero, 100, Easing.OutCirc);
        }
    }

    protected override bool OnClick(ClickEvent e)
    {
        if (ShouldAnimate && IsHovered)
        {
            this.TransformTo(nameof(EdgeEffect), VRCOSCEdgeEffects.HoverShadow, 100, Easing.OutCirc);
            this.MoveTo(HoverOffset, 100, Easing.OutCirc);
        }

        return base.OnClick(e);
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        if (e.Button != MouseButton.Left) return base.OnMouseDown(e);

        if (ShouldAnimate)
        {
            this.TransformTo(nameof(EdgeEffect), VRCOSCEdgeEffects.NoShadow, 100, Easing.OutCirc);
            this.MoveTo(Vector2.Zero, 100, Easing.OutCirc);
        }

        return true;
    }
}
