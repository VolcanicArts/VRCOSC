// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osuTK;
using VRCOSC.Graphics;

namespace VRCOSC.Screens.Exceptions;

public partial class ExceptionScreen : VisibilityContainer
{
    private const string error_title = "An Error Has Occurred";
    private const string critical_error_title = "A Critical Error Has Occurred";

    private static ExceptionScreen instance = null!;

    protected override bool OnMouseDown(MouseDownEvent e) => State.Value == Visibility.Visible;
    protected override bool OnClick(ClickEvent e) => State.Value == Visibility.Visible;
    protected override bool OnHover(HoverEvent e) => State.Value == Visibility.Visible;
    protected override bool OnScroll(ScrollEvent e) => State.Value == Visibility.Visible;

    private SpriteText titleText = null!;
    private TextFlowContainer errorText = null!;

    public ExceptionScreen()
    {
        instance = this;
    }

    public static void HandleException(Exception e, string message = "", bool critical = false) => instance.handleException(e, message, critical);

    private void handleException(Exception e, string message, bool critical) => Scheduler.Add(() =>
    {
        var title = critical ? critical_error_title : error_title;
        var finalMessage = critical ? "This error is unrecoverable. Please restart the app and send your logs in a support thread in the Discord server" : "Please send your logs in a support thread in the Discord server";
        finalMessage += "\n\nError Message\n" + (string.IsNullOrEmpty(message) ? "" : $"{message}\n") + e.Message;

        titleText.Text = title;
        errorText.Text = finalMessage;
        Logger.Error(e, title, LoggingTarget.Runtime, true);
        Show();
    }, false);

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;
        AlwaysPresent = true;

        AddInternal(new Box
        {
            RelativeSizeAxes = Axes.Both,
            Colour = Colours.BLACK.Opacity(0.5f)
        });

        AddInternal(new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Size = new Vector2(800, 450),
            Masking = true,
            CornerRadius = 10,
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colours.GRAY2
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, 60),
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 60)
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Colours.GRAY1
                                    },
                                    titleText = new SpriteText
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Text = "An Error Has Occurred",
                                        Font = Fonts.REGULAR.With(size: 40)
                                    }
                                }
                            }
                        },
                        new Drawable[]
                        {
                            errorText = new TextFlowContainer(t =>
                            {
                                t.Font = Fonts.REGULAR.With(size: 30);
                                t.Colour = Colours.WHITE2;
                            })
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding(5),
                                TextAnchor = Anchor.TopCentre
                            }
                        },
                        new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Colours.GRAY1
                            }
                        }
                    }
                }
            }
        });
    }

    protected override void PopIn()
    {
        this.FadeInFromZero(100, Easing.OutQuint);
    }

    protected override void PopOut()
    {
        this.FadeOutFromOne(100, Easing.OutQuint);
    }
}
