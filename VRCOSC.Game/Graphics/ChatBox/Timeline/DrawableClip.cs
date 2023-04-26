// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK.Input;
using VRCOSC.Game.ChatBox;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.ChatBox.Timeline;

[Cached]
public partial class DrawableClip : Container
{
    [Resolved]
    private TimelineEditor timelineEditor { get; set; } = null!;

    [Resolved]
    private TimelineLayer timelineLayer { get; set; } = null!;

    [Resolved]
    private ChatBoxManager chatBoxManager { get; set; } = null!;

    public readonly Clip Clip;

    private float cumulativeDrag;
    private SpriteText drawName = null!;
    private Box background = null!;

    public DrawableClip(Clip clip)
    {
        Clip = clip;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;
        RelativePositionAxes = Axes.X;

        Child = new Container
        {
            RelativeSizeAxes = Axes.Both,
            Masking = true,
            CornerRadius = 10,
            BorderColour = ThemeManager.Current[ThemeAttribute.Accent],
            Children = new Drawable[]
            {
                background = new Box
                {
                    Colour = ThemeManager.Current[ThemeAttribute.Light],
                    RelativeSizeAxes = Axes.Both
                },
                new StartResizeDetector(Clip)
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 15
                },
                new EndResizeDetector(Clip)
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 15
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
                            Font = FrameworkFont.Regular.With(size: 20),
                            Colour = ThemeManager.Current[ThemeAttribute.Text]
                        }
                    }
                }
            }
        };
    }

    protected override void LoadComplete()
    {
        chatBoxManager.SelectedClip.BindValueChanged(e =>
        {
            ((Container)Child).BorderThickness = Clip == e.NewValue ? 4 : 2;
            background.FadeColour(Clip == e.NewValue ? ThemeManager.Current[ThemeAttribute.Dark] : ThemeManager.Current[ThemeAttribute.Light], 300, Easing.OutQuart);
        }, true);

        Clip.Name.BindValueChanged(e => drawName.Text = e.NewValue, true);
        Clip.Enabled.BindValueChanged(e => Child.FadeTo(e.NewValue ? 1 : 0.5f), true);

        updateSizeAndPosition();
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        chatBoxManager.SelectedClip.Value = Clip;

        if (e.Button == MouseButton.Left)
        {
            timelineEditor.HideClipMenu();
        }
        else if (e.Button == MouseButton.Right)
        {
            timelineEditor.ShowClipMenu(Clip, e);
        }

        return true;
    }

    protected override bool OnDragStart(DragStartEvent e) => true;

    protected override void OnDragEnd(DragEndEvent e) => Clip.Save();

    protected override void OnDrag(DragEvent e)
    {
        base.OnDrag(e);

        chatBoxManager.SelectedClip.Value = Clip;

        e.Target = timelineLayer;
        cumulativeDrag += e.Delta.X / Parent.DrawWidth;

        if (Math.Abs(cumulativeDrag) >= chatBoxManager.TimelineResolution)
        {
            var newStart = Clip.Start.Value + Math.Sign(cumulativeDrag);
            var newEnd = Clip.End.Value + Math.Sign(cumulativeDrag);

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
        Width = Clip.Length * chatBoxManager.TimelineResolution;
        X = Clip.Start.Value * chatBoxManager.TimelineResolution;
    }

    private partial class ResizeDetector : Container
    {
        protected readonly Clip Clip;

        private Box resizeBackground = null!;

        protected ResizeDetector(Clip clip)
        {
            Clip = clip;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                resizeBackground = new Box
                {
                    Colour = ThemeManager.Current[ThemeAttribute.Mid],
                    RelativeSizeAxes = Axes.Both
                },
                new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,
                    Icon = FontAwesome.Solid.GripLinesVertical,
                    Colour = ThemeManager.Current[ThemeAttribute.SubText]
                }
            };
        }

        protected override bool OnDragStart(DragStartEvent e) => true;

        protected override void OnDragEnd(DragEndEvent e) => Clip.Save();

        protected override bool OnHover(HoverEvent e)
        {
            resizeBackground.FadeColour(ThemeManager.Current[ThemeAttribute.Darker], 100);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            resizeBackground.FadeColour(ThemeManager.Current[ThemeAttribute.Mid], 100);
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

        public StartResizeDetector(Clip clip)
            : base(clip)
        {
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;
        }

        protected override void OnDrag(DragEvent e)
        {
            base.OnDrag(e);

            e.Target = timelineLayer;

            var mousePosNormalised = e.MousePosition.X / timelineLayer.DrawWidth;
            var newStart = (int)Math.Floor(mousePosNormalised * chatBoxManager.TimelineLengthSeconds);

            if (newStart != Clip.Start.Value)
            {
                var (lowerBound, upperBound) = timelineLayer.GetBoundsNearestTo(newStart < Clip.Start.Value ? Clip.Start.Value : newStart, false);

                if (newStart >= lowerBound && newStart < upperBound)
                {
                    Clip.Start.Value = newStart;
                }
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

        public EndResizeDetector(Clip clip)
            : base(clip)
        {
            Anchor = Anchor.CentreRight;
            Origin = Anchor.CentreRight;
        }

        protected override void OnDrag(DragEvent e)
        {
            base.OnDrag(e);

            e.Target = timelineLayer;
            var mousePosNormalised = e.MousePosition.X / timelineLayer.DrawWidth;
            var newEnd = (int)Math.Ceiling(mousePosNormalised * chatBoxManager.TimelineLengthSeconds);

            if (newEnd != Clip.Start.Value)
            {
                var (lowerBound, upperBound) = timelineLayer.GetBoundsNearestTo(newEnd < Clip.End.Value ? newEnd : Clip.End.Value, true);

                if (newEnd > lowerBound && newEnd <= upperBound)
                {
                    Clip.End.Value = newEnd;
                }
            }

            parentDrawableClip.updateSizeAndPosition();
        }
    }
}
