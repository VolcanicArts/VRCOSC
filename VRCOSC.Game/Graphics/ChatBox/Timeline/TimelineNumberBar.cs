// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Managers;

namespace VRCOSC.Game.Graphics.ChatBox.Timeline;

public partial class TimelineNumberBar : Container
{
    [Resolved]
    private ChatBoxManager chatBoxManager { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        chatBoxManager.TimelineLength.BindValueChanged(_ =>
        {
            Clear();

            for (var i = 0; i <= chatBoxManager.TimelineLengthSeconds; i++)
            {
                if (i != 0 && i != chatBoxManager.TimelineLengthSeconds && i % 5 == 0)
                {
                    Add(new Container
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopCentre,
                        RelativePositionAxes = Axes.X,
                        RelativeSizeAxes = Axes.Y,
                        X = chatBoxManager.TimelineResolution * i,
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.Y,
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
                                        Font = FrameworkFont.Regular.With(size: 15),
                                        Colour = ThemeManager.Current[ThemeAttribute.Text]
                                    },
                                    new Box
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        Colour = ThemeManager.Current[ThemeAttribute.Lighter],
                                        Width = 2,
                                        RelativeSizeAxes = Axes.Y
                                    }
                                }
                            }
                        }
                    });
                }
            }
        }, true);
    }
}
