// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace VRCOSC.Game.Graphics.UI.List;

public abstract partial class HeightLimitedScrollableList<T> : Container<T> where T : HeightLimitedScrollableListItem
{
    private readonly FillFlowContainer flowWrapper;
    private Drawable header = null!;
    private Drawable footer = null!;
    private BasicScrollContainer scrollContainer = null!;

    protected virtual Colour4 BackgroundColourOdd => Colours.GRAY2;
    protected virtual Colour4 BackgroundColourEven => Colours.GRAY4;
    protected virtual bool AnimatePositionChange => false;

    protected override FillFlowContainer<T> Content { get; }

    protected HeightLimitedScrollableList()
    {
        InternalChild = flowWrapper = new FillFlowContainer
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Masking = true,
            CornerRadius = 5
        };

        Content = new FillFlowContainer<T>
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical
        };
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        flowWrapper.AddRange(new[]
        {
            header = CreateHeader(),
            scrollContainer = new BasicScrollContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                ClampExtension = 0,
                ScrollbarVisible = false,
                AutoSizeDuration = AnimatePositionChange ? 100 : 0,
                AutoSizeEasing = AnimatePositionChange ? Easing.OutQuint : Easing.None,
                ScrollContent =
                {
                    Child = Content
                }
            },
            footer = CreateFooter()
        });

        Content.LayoutDuration = AnimatePositionChange ? 100 : 0;
        Content.LayoutEasing = AnimatePositionChange ? Easing.OutQuint : Easing.None;
    }

    protected void ChangeListChildPosition(T child, float depth)
    {
        Content.ChangeChildDepth(child, depth);
        Content.SetLayoutPosition(child, depth);
    }

    protected virtual Drawable CreateHeader() => new Container
    {
        Anchor = Anchor.TopCentre,
        Origin = Anchor.TopCentre,
        RelativeSizeAxes = Axes.X,
        Height = 40,
        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY0
            }
        }
    };

    protected virtual Drawable CreateFooter() => new Box
    {
        Anchor = Anchor.TopCentre,
        Origin = Anchor.TopCentre,
        RelativeSizeAxes = Axes.X,
        Height = 5,
        Colour = Colours.GRAY0
    };

    protected override void UpdateAfterChildren()
    {
        var contentTargetHeight = DrawHeight - header.DrawHeight - footer.DrawHeight;

        if (Content.Height >= contentTargetHeight)
        {
            scrollContainer.AutoSizeAxes = Axes.None;
            scrollContainer.Height = contentTargetHeight;
        }
        else
        {
            scrollContainer.AutoSizeAxes = Axes.Y;
        }

        var even = false;

        foreach (var child in this)
        {
            child.SetBackground(even ? BackgroundColourEven : BackgroundColourOdd);
            even = !even;
        }
    }
}
