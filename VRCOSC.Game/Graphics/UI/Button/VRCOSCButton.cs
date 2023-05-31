// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Input;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.UI.Button;

public partial class VRCOSCButton : ClickableContainer
{
    private const float alpha_enabled = 1.0f;
    private const float alpha_disabled = 0.5f;
    private static readonly Vector2 hover_offset = new(0, -2);

    public bool ShouldAnimate { get; init; } = true;
    public bool Circular { get; init; }

    protected override Container Content { get; }

    public override bool HandlePositionalInput => Enabled.Value;

    public new float CornerRadius
    {
        set => Content.CornerRadius = value;
    }

    public new float BorderThickness
    {
        set => Content.BorderThickness = value;
    }

    protected VRCOSCButton()
    {
        InternalChild = Content = new Container
        {
            RelativeSizeAxes = Axes.Both,
            Masking = true,
            EdgeEffect = VRCOSCEdgeEffects.NoShadow,
            BorderColour = ThemeManager.Current[ThemeAttribute.Border]
        };

        Enabled.Value = true;
        Enabled.BindValueChanged(_ => Content.FadeTo(Enabled.Value ? alpha_enabled : alpha_disabled, 500, Easing.OutCirc), true);
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
            Content.TransformTo(nameof(EdgeEffect), VRCOSCEdgeEffects.HoverShadow, 100, Easing.OutCirc);
            Content.MoveTo(hover_offset, 100, Easing.OutCirc);
        }

        return true;
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        if (ShouldAnimate)
        {
            Content.TransformTo(nameof(EdgeEffect), VRCOSCEdgeEffects.NoShadow, 100, Easing.OutCirc);
            Content.MoveTo(Vector2.Zero, 100, Easing.OutCirc);
        }
    }

    protected override bool OnClick(ClickEvent e)
    {
        if (ShouldAnimate && IsHovered)
        {
            Content.TransformTo(nameof(EdgeEffect), VRCOSCEdgeEffects.HoverShadow, 100, Easing.OutCirc);
            Content.MoveTo(hover_offset, 100, Easing.OutCirc);
        }

        return base.OnClick(e);
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        if (e.Button != MouseButton.Left) return base.OnMouseDown(e);

        if (ShouldAnimate)
        {
            Content.TransformTo(nameof(EdgeEffect), VRCOSCEdgeEffects.NoShadow, 100, Easing.OutCirc);
            Content.MoveTo(Vector2.Zero, 100, Easing.OutCirc);
        }

        return true;
    }
}
