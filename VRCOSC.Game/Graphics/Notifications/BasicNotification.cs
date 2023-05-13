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
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.Notifications;

public partial class BasicNotification : Notification
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
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            ColumnDimensions = new[]
            {
                new Dimension(GridSizeMode.Absolute, 55),
                new Dimension()
            },
            RowDimensions = new[]
            {
                new Dimension(GridSizeMode.AutoSize)
            },
            Content = new[]
            {
                new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
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
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
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
            s.Colour = ThemeManager.Current[ThemeAttribute.Text];
        });

        textFlow.AddParagraph(Description, s =>
        {
            s.Font = FrameworkFont.Regular.With(size: 13);
            s.Colour = ThemeManager.Current[ThemeAttribute.Text];
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
