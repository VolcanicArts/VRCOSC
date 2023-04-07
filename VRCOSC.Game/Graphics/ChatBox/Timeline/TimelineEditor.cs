// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK;
using VRCOSC.Game.ChatBox;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.ChatBox.Timeline;

[Cached]
public partial class TimelineEditor : Container
{
    [Resolved]
    private Bindable<Clip?> selectedClip { get; set; } = null!;

    [Resolved]
    private ChatBoxManager chatBoxManager { get; set; } = null!;

    private RectangularPositionSnapGrid snapping = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        TimelineLayer layer3;

        Children = new Drawable[]
        {
            new Box
            {
                Colour = ThemeManager.Current[ThemeAttribute.Mid],
                RelativeSizeAxes = Axes.Both
            },
            snapping = new RectangularPositionSnapGrid(Vector2.Zero)
            {
                RelativeSizeAxes = Axes.Both
            },
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(2),
                Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 2),
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 2),
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 2),
                        new Dimension(),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            layer3 = new TimelineLayer()
                        },
                        null,
                        new Drawable[]
                        {
                            new TimelineLayer()
                        },
                        null,
                        new Drawable[]
                        {
                            new TimelineLayer()
                        },
                        null,
                        new Drawable[]
                        {
                            new TimelineLayer()
                        }
                    }
                }
            }
        };

        chatBoxManager.Clips.ForEach(clip => layer3.Add(clip));
    }

    protected override void LoadComplete()
    {
        chatBoxManager.TimelineLength.BindValueChanged(e => snapping.Spacing = new Vector2(DrawWidth / (float)e.NewValue.TotalSeconds, 175), true);
    }

    protected override bool OnClick(ClickEvent e)
    {
        selectedClip.Value = null;
        return true;
    }

    public Vector2 GetSnappedPosition(Vector2 pos) => snapping.GetSnappedPosition(pos);
}
