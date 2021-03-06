// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osuTK;
using VRCOSC.Game.Graphics.Drawables;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleRun;

public sealed class TerminalContainer : Container
{
    private BasicScrollContainer terminalScroll;
    private FillFlowContainer<TerminalEntry> terminalFlow;

    public TerminalContainer()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Child = new GridContainer
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            RowDimensions = new[]
            {
                new Dimension(GridSizeMode.Absolute, 60),
                new Dimension()
            },
            Content = new[]
            {
                new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new SpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Terminal",
                                Font = FrameworkFont.Regular.With(size: 40),
                            },
                            new LineSeparator
                            {
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Size = new Vector2(0.975f, 0.075f)
                            }
                        }
                    }
                },
                new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding(15),
                        Child = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            BorderThickness = 3,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = VRCOSCColour.Gray2,
                                },
                                new Container
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding(3f),
                                    Child = terminalScroll = new BasicScrollContainer
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        RelativeSizeAxes = Axes.Both,
                                        ScrollbarVisible = false,
                                        ClampExtension = 0,
                                        Child = terminalFlow = new FillFlowContainer<TerminalEntry>
                                        {
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FillDirection.Vertical,
                                            Padding = new MarginPadding
                                            {
                                                Horizontal = 3
                                            },
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        Logger.NewEntry += logEntry =>
        {
            if (logEntry.LoggerName == "terminal")
                log(logEntry.Message);
        };
    }

    protected override void UpdateAfterChildren()
    {
        base.UpdateAfterChildren();

        while (terminalFlow.Count > 50) terminalFlow[0].RemoveAndDisposeImmediately();
        terminalScroll.ScrollToEnd();
    }

    private void log(string text)
    {
        Scheduler.Add(() =>
        {
            var formattedText = $"[{DateTime.Now:HH:mm:ss}] {text}";
            terminalFlow.Add(new TerminalEntry(formattedText));
        });
    }

    public void ClearTerminal()
    {
        terminalFlow.Clear(true);
    }
}
