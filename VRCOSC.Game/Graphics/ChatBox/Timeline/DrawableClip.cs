// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK.Graphics;
using osuTK.Input;
using VRCOSC.Game.ChatBox;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.ChatBox.Timeline;

[Cached]
public partial class DrawableClip : Container
{
    [Resolved]
    private Bindable<Clip?> selectedClip { get; set; } = null!;

    [Resolved]
    private TimelineEditor timelineEditor { get; set; } = null!;

    [Resolved]
    private TimelineLayer timelineLayer { get; set; } = null!;

    [Resolved]
    private ChatBoxManager chatBoxManager { get; set; } = null!;

    public readonly Clip Clip;

    private float cumulativeDrag;

    public DrawableClip(Clip clip)
    {
        Clip = clip;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;
        RelativePositionAxes = Axes.X;
        Masking = true;
        CornerRadius = 10;
        BorderColour = ThemeManager.Current[ThemeAttribute.Lighter];

        updateSizeAndPosition();

        selectedClip.BindValueChanged(e =>
        {
            BorderThickness = Clip == e.NewValue ? 4 : 2;
        }, true);

        SpriteText drawName;

        Children = new Drawable[]
        {
            new Box
            {
                Colour = ThemeManager.Current[ThemeAttribute.Light],
                RelativeSizeAxes = Axes.Both
            },
            new StartResizeDetector(Clip, v => v / Parent.DrawWidth)
            {
                RelativeSizeAxes = Axes.Y,
                Width = 20
            },
            new EndResizeDetector(Clip, v => v / Parent.DrawWidth)
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                RelativeSizeAxes = Axes.Y,
                Width = 20
            },
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(10),
                Children = new Drawable[]
                {
                    drawName = new SpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = FrameworkFont.Regular.With(size: 20)
                    }
                }
            }
        };

        Clip.Name.BindValueChanged(e => drawName.Text = e.NewValue, true);
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        if (e.Button == MouseButton.Left)
        {
            timelineEditor.HideClipMenu();
            selectedClip.Value = Clip;
        }
        else if (e.Button == MouseButton.Right)
        {
            timelineEditor.ShowClipMenu(Clip, e);
        }

        return true;
    }

    protected override bool OnDragStart(DragStartEvent e) => true;

    protected override void OnDrag(DragEvent e)
    {
        base.OnDrag(e);

        selectedClip.Value = Clip;

        e.Target = Parent;

        var deltaX = e.Delta.X / Parent.DrawWidth;
        cumulativeDrag += deltaX;

        if (Math.Abs(cumulativeDrag) >= chatBoxManager.Resolution)
        {
            var newStart = Clip.Start.Value + (float.IsNegative(cumulativeDrag) ? -1 : 1);
            var newEnd = Clip.End.Value + (float.IsNegative(cumulativeDrag) ? -1 : 1);

            var (lowerBound, _) = timelineLayer.GetBoundsNearestTo(Clip.Start.Value, false);
            var (_, upperBound) = timelineLayer.GetBoundsNearestTo(Clip.End.Value, true);

            if (newStart >= lowerBound && newEnd <= upperBound)
            {
                Clip.Start.Value = newStart;
                Clip.End.Value = newEnd;
            }

            cumulativeDrag = 0f;
        }

        updateSizeAndPosition();
    }

    private void updateSizeAndPosition()
    {
        Width = Clip.Length * chatBoxManager.Resolution;
        X = Clip.Start.Value * chatBoxManager.Resolution;
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

        [Resolved]
        private DrawableClip parentDrawableClip { get; set; } = null!;

        [Resolved]
        private TimelineLayer timelineLayer { get; set; } = null!;

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
                var newStart = Clip.Start.Value + (float.IsNegative(CumulativeDrag) ? -1 : 1);

                var (lowerBound, upperBound) = timelineLayer.GetBoundsNearestTo(float.IsNegative(CumulativeDrag) ? Clip.Start.Value : newStart, false);

                if (newStart >= lowerBound && newStart < upperBound)
                {
                    Clip.Start.Value = newStart;
                }

                CumulativeDrag = 0f;
            }

            parentDrawableClip.updateSizeAndPosition();
        }
    }

    private partial class EndResizeDetector : ResizeDetector
    {
        [Resolved]
        private ChatBoxManager chatBoxManager { get; set; } = null!;

        [Resolved]
        private DrawableClip parentDrawableClip { get; set; } = null!;

        [Resolved]
        private TimelineLayer timelineLayer { get; set; } = null!;

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
                var newEnd = Clip.End.Value + (float.IsNegative(CumulativeDrag) ? -1 : 1);

                var (lowerBound, upperBound) = timelineLayer.GetBoundsNearestTo(float.IsNegative(CumulativeDrag) ? newEnd : Clip.End.Value, true);

                if (newEnd > lowerBound && newEnd <= upperBound)
                {
                    Clip.End.Value = newEnd;
                }

                CumulativeDrag = 0f;
            }

            parentDrawableClip.updateSizeAndPosition();
        }
    }
}
