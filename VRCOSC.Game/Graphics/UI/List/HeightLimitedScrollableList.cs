// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace VRCOSC.Game.Graphics.UI.List;

public abstract partial class HeightLimitedScrollableList<T> : Container where T : HeightLimitedScrollableListItem
{
    private Drawable header = null!;
    private Drawable footer = null!;
    private BasicScrollContainer scrollContainer = null!;
    private FillFlowContainer<T> listingFlow = null!;

    protected virtual Colour4 BackgroundColourOdd => Colours.GRAY2;
    protected virtual Colour4 BackgroundColourEven => Colours.GRAY1;

    [BackgroundDependencyLoader]
    private void load()
    {
        Child = new FillFlowContainer
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Masking = true,
            CornerRadius = 5,
            Children = new[]
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
                    AutoSizeEasing = Easing.OutQuint,
                    AutoSizeDuration = 100,
                    ScrollContent =
                    {
                        Child = listingFlow = new FillFlowContainer<T>
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            LayoutDuration = 100,
                            LayoutEasing = Easing.OutQuint
                        }
                    }
                },
                footer = CreateFooter(),
            }
        };
    }

    public void AddList(T drawable)
    {
        listingFlow.Add(drawable);
    }

    public void ClearList()
    {
        listingFlow.Clear(true);
    }

    public void ChangeListChildPosition(T child, float depth)
    {
        listingFlow.ChangeChildDepth(child, depth);
        listingFlow.SetLayoutPosition(child, depth);
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

        if (listingFlow.Height >= contentTargetHeight)
        {
            scrollContainer.AutoSizeAxes = Axes.None;
            scrollContainer.Height = contentTargetHeight;
        }
        else
        {
            scrollContainer.AutoSizeAxes = Axes.Y;
        }

        var even = false;

        foreach (var child in listingFlow)
        {
            child.SetBackground(even ? BackgroundColourEven : BackgroundColourOdd);
            even = !even;
        }
    }
}
