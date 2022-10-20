// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;

namespace VRCOSC.Game.Graphics.Notifications;

public class BasicNotification : Notification
{
    public IconUsage Icon { get; init; }

    public new Colour4 Colour { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public Action? ClickCallback { get; init; }

    protected override Drawable CreateForeground()
    {
        TextFlowContainer textFlow;

        var foreground = new GridContainer
        {
            RelativeSizeAxes = Axes.Both,
            ColumnDimensions = new[]
            {
                new Dimension(GridSizeMode.Absolute, 55),
                new Dimension(),
            },
            Content = new[]
            {
                new Drawable[]
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
                                RelativeSizeAxes = Axes.Both,
                                Masking = true,
                                CornerRadius = 5,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = ColourInfo.GradientVertical(Colour.Darken(0.25f), Colour)
                                    },
                                    new SpriteIcon
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        RelativeSizeAxes = Axes.Both,
                                        Size = new Vector2(0.7f),
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
                        Padding = new MarginPadding
                        {
                            Vertical = 5,
                            Right = 5
                        }
                    }
                }
            }
        };

        textFlow.AddText(Title, s =>
        {
            s.Font = FrameworkFont.Regular.With(size: 20);
        });
        textFlow.AddText($"\n{Description}", s =>
        {
            s.Font = FrameworkFont.Regular.With(size: 13);
        });

        return foreground;
    }

    protected override bool OnClick(ClickEvent e)
    {
        base.OnClick(e);
        ClickCallback?.Invoke();
        return true;
    }
}
