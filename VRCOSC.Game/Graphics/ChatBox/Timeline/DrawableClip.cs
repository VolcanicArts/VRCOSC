// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK.Graphics;
using VRCOSC.Game.ChatBox.Clips;

namespace VRCOSC.Game.Graphics.ChatBox.Timeline;

public partial class DrawableClip : Container
{
    [Resolved]
    private Bindable<Clip?> selectedClip { get; set; } = null!;

    private readonly Clip clip;

    public DrawableClip(Clip clip)
    {
        this.clip = clip;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Height = 120;
        RelativeSizeAxes = Axes.X;
        RelativePositionAxes = Axes.X;
        Masking = true;
        CornerRadius = 5;
        BorderColour = Colour4.Aqua.Darken(0.5f);

        clip.Start.BindValueChanged(_ => updateSizeAndPosition());
        clip.End.BindValueChanged(_ => updateSizeAndPosition());
        updateSizeAndPosition();

        selectedClip.BindValueChanged(e =>
        {
            BorderThickness = clip == e.NewValue ? 3 : 0;
        }, true);

        Children = new Drawable[]
        {
            new Box
            {
                Colour = Color4.OrangeRed,
                RelativeSizeAxes = Axes.Both,
            },
            new ResizeDetector(clip.Start, v => v / Parent.DrawWidth)
            {
                RelativeSizeAxes = Axes.Y,
                Width = 20
            },
            new ResizeDetector(clip.End, v => v / Parent.DrawWidth)
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                RelativeSizeAxes = Axes.Y,
                Width = 20
            }
        };
    }

    protected override bool OnClick(ClickEvent e)
    {
        selectedClip.Value = clip;
        return true;
    }

    protected override bool OnDragStart(DragStartEvent e) => true;

    protected override void OnDrag(DragEvent e)
    {
        base.OnDrag(e);

        selectedClip.Value = clip;

        e.Target = Parent;
        var delta = e.Delta.X / Parent.DrawWidth;
        // TODO - Apply snapping
        // TODO - Apply limits
        clip.Start.Value += delta;
        clip.End.Value += delta;
    }

    private void updateSizeAndPosition()
    {
        Width = clip.Length;
        X = clip.Start.Value;
    }

    private partial class ResizeDetector : Container
    {
        private readonly Bindable<float> value;
        private readonly Func<float, float> normaliseFunc;

        public ResizeDetector(Bindable<float> value, Func<float, float> normaliseFunc)
        {
            this.value = value;
            this.normaliseFunc = normaliseFunc;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new Box
            {
                Colour = Color4.Black.Opacity(0f),
                RelativeSizeAxes = Axes.Both
            };
        }

        protected override bool OnDragStart(DragStartEvent e) => true;

        protected override void OnDrag(DragEvent e)
        {
            base.OnDrag(e);

            e.Target = Parent.Parent;
            value.Value += normaliseFunc.Invoke(e.Delta.X);
        }

        protected override bool OnHover(HoverEvent e)
        {
            Child.FadeColour(Colour4.Black.Opacity(0.5f), 100);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            Child.FadeColour(Colour4.Black.Opacity(0f), 100);
        }
    }
}
