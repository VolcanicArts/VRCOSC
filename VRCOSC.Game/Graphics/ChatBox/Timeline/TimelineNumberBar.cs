// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.ChatBox;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.ChatBox.Timeline;

public partial class TimelineNumberBar : Container
{
    [Resolved]
    private ChatBoxManager chatBoxManager { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        for (var i = 0; i < chatBoxManager.TimelineLengthSeconds; i++)
        {
            if (i != 0 && i != chatBoxManager.TimelineLengthSeconds && i % 5 == 0)
            {
                Add(new Container
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopCentre,
                    RelativePositionAxes = Axes.X,
                    X = chatBoxManager.TimelineResolution * i,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            AutoSizeAxes = Axes.Y,
                            Width = 10,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 2),
                            Children = new Drawable[]
                            {
                                new SpriteText
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Text = i.ToString(),
                                    Font = FrameworkFont.Regular.With(size: 15)
                                },
                                new Box
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Colour = ThemeManager.Current[ThemeAttribute.Lighter],
                                    Size = new Vector2(3, 10)
                                }
                            }
                        }
                    }
                });
            }
        }
    }
}
