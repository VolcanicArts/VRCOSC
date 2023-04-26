// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Graphics.ChatBox.SelectedClip;
using VRCOSC.Game.Graphics.ChatBox.Timeline;
using VRCOSC.Game.Graphics.ChatBox.Timeline.Menu.Clip;
using VRCOSC.Game.Graphics.ChatBox.Timeline.Menu.Layer;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.ChatBox;

[Cached]
public partial class ChatBoxScreen : Container
{
    [Cached]
    private TimelineLayerMenu layerMenu = new();

    [Cached]
    private TimelineClipMenu clipMenu = new();

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;

        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = ThemeManager.Current[ThemeAttribute.Light]
            },
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(10),
                Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 5),
                        new Dimension(GridSizeMode.Absolute, 40),
                        new Dimension(GridSizeMode.Absolute, 5),
                        new Dimension()
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new SelectedClipEditorWrapper
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.Both
                            },
                        },
                        null,
                        new Drawable[]
                        {
                            new TimelineLengthContainer
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Width = 0.2f,
                                Masking = true,
                                CornerRadius = 10
                            }
                        },
                        null,
                        new Drawable[]
                        {
                            new TimelineWrapper
                            {
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                RelativeSizeAxes = Axes.Both
                            }
                        }
                    }
                }
            },
            layerMenu,
            clipMenu
        };
    }
}
