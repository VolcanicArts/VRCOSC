// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace VRCOSC.Game.Graphics.Notifications;

public class BasicNotification : Notification
{
    public IconUsage Icon { get; init; }

    public new Colour4 Colour { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    protected override Drawable CreateForeground()
    {
        TextFlowContainer textFlow;
        var foreground = new FillFlowContainer
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Direction = FillDirection.Horizontal,
            Spacing = new Vector2(-5, 0),
            Children = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,
                    Padding = new MarginPadding(5),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = 5,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = ColourInfo.GradientVertical(Colour.Darken(0.25f), Colour)
                                },
                                new SpriteIcon
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Size = new Vector2(0.5f),
                                    Icon = Icon
                                }
                            }
                        }
                    }
                },
                textFlow = new TextFlowContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    TextAnchor = Anchor.TopLeft,
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(5)
                }
            }
        };

        textFlow.AddText(Title, s =>
        {
            s.Font = FrameworkFont.Regular.With(size: 20);
        });
        textFlow.AddText($"\n{Description}", s =>
        {
            s.Font = FrameworkFont.Regular.With(size: 15);
        });

        return foreground;
    }
}
