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
using osuTK;
using osuTK.Graphics;
using VRCOSC.Game.ChatBox;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.ChatBox.Timeline;

public partial class DrawableClip : Container
{
    [Resolved]
    private Bindable<Clip?> selectedClip { get; set; } = null!;

    [Resolved]
    private TimelineEditor timelineEditor { get; set; } = null!;

    [Resolved]
    private ChatBoxManager chatBoxManager { get; set; } = null!;

    private readonly Clip clip;

    private float cumulativeDrag;

    public DrawableClip(Clip clip)
    {
        this.clip = clip;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;
        RelativePositionAxes = Axes.X;
        Masking = true;
        CornerRadius = 10;
        BorderColour = ThemeManager.Current[ThemeAttribute.Lighter];

        // TODO - Solve unbind issue after 2nd+ selection
        clip.Start.BindValueChanged(_ => updateSizeAndPosition());
        clip.End.BindValueChanged(_ => updateSizeAndPosition());
        updateSizeAndPosition();

        selectedClip.BindValueChanged(e =>
        {
            BorderThickness = clip == e.NewValue ? 4 : 2;
        }, true);

        Children = new Drawable[]
        {
            new Box
            {
                Colour = ThemeManager.Current[ThemeAttribute.Light],
                RelativeSizeAxes = Axes.Both
            },
            new StartResizeDetector(clip, v => v / Parent.DrawWidth)
            {
                RelativeSizeAxes = Axes.Y,
                Width = 20
            },
            new EndResizeDetector(clip, v => v / Parent.DrawWidth)
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

        var deltaX = e.Delta.X / Parent.DrawWidth;
        cumulativeDrag += deltaX;

        if (Math.Abs(cumulativeDrag) >= chatBoxManager.Resolution)
        {
            var newStart = clip.Start.Value + chatBoxManager.Resolution * (float.IsNegative(cumulativeDrag) ? -1 : 1);
            var newEnd = clip.End.Value + chatBoxManager.Resolution * (float.IsNegative(cumulativeDrag) ? -1 : 1);

            if (newStart >= 0 && newEnd <= 1)
            {
                clip.Start.Value = newStart;
                clip.End.Value = newEnd;
            }

            cumulativeDrag = 0f;
        }

        // TODO - Implement delta Y for dragging between priorities
    }

    private void updateSizeAndPosition()
    {
        Width = clip.Length;
        X = clip.Start.Value;
    }

    private partial class ResizeDetector : Container
    {
        protected readonly Clip Clip;
        protected readonly Func<float, float> NormaliseFunc;

        protected float CumulativeDrag;

        public ResizeDetector(Clip clip, Func<float, float> normaliseFunc)
        {
            Clip = clip;
            NormaliseFunc = normaliseFunc;
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

    private partial class StartResizeDetector : ResizeDetector
    {
        [Resolved]
        private ChatBoxManager chatBoxManager { get; set; } = null!;

        public StartResizeDetector(Clip clip, Func<float, float> normaliseFunc)
            : base(clip, normaliseFunc)
        {
        }

        protected override void OnDrag(DragEvent e)
        {
            base.OnDrag(e);

            e.Target = Parent.Parent;
            CumulativeDrag += NormaliseFunc.Invoke(e.Delta.X);

            if (Math.Abs(CumulativeDrag) >= chatBoxManager.Resolution)
            {
                var newStart = Clip.Start.Value + chatBoxManager.Resolution * (float.IsNegative(CumulativeDrag) ? -1 : 1);

                if (newStart < Clip.End.Value && newStart >= 0)
                {
                    Clip.Start.Value = newStart;
                }

                CumulativeDrag = 0f;
            }
        }
    }

    private partial class EndResizeDetector : ResizeDetector
    {
        [Resolved]
        private ChatBoxManager chatBoxManager { get; set; } = null!;

        public EndResizeDetector(Clip clip, Func<float, float> normaliseFunc)
            : base(clip, normaliseFunc)
        {
        }

        protected override void OnDrag(DragEvent e)
        {
            base.OnDrag(e);

            e.Target = Parent.Parent;
            CumulativeDrag += NormaliseFunc.Invoke(e.Delta.X);

            if (Math.Abs(CumulativeDrag) >= chatBoxManager.Resolution)
            {
                var newEnd = Clip.End.Value + chatBoxManager.Resolution * (float.IsNegative(CumulativeDrag) ? -1 : 1);

                if (newEnd > Clip.Start.Value && newEnd <= 1)
                {
                    Clip.End.Value = newEnd;
                }

                CumulativeDrag = 0f;
            }
        }
    }
}
