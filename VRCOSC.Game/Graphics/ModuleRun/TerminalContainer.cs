// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osuTK;

namespace VRCOSC.Game.Graphics.ModuleRun;

public sealed class TerminalContainer : Container<TerminalEntry>
{
    private readonly BasicScrollContainer terminalScroll;

    protected override FillFlowContainer<TerminalEntry> Content { get; }

    private readonly DrawablePool<TerminalEntry> terminalEntryPool = new(50);

    public TerminalContainer()
    {
        RelativeSizeAxes = Axes.Both;

        InternalChild = new GridContainer
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
                    new TerminalHeader()
                },
                new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding(15),
                        Child = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            BorderThickness = 3,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = VRCOSCColour.Gray2,
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding(3f),
                                    Child = terminalScroll = new BasicScrollContainer
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        ScrollbarVisible = false,
                                        ClampExtension = 0,
                                        Child = Content = new FillFlowContainer<TerminalEntry>
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
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        Logger.NewEntry += logEntry =>
        {
            if (logEntry.LoggerName == "terminal")
                log(logEntry.Message);
        };
    }

    public void Reset()
    {
        RemoveAll(_ => true, false);
    }

    protected override void UpdateAfterChildren()
    {
        base.UpdateAfterChildren();

        while (Count > 50)
        {
            var entry = this[0];
            Remove(entry, false);
        }

        terminalScroll.ScrollToEnd();
    }

    private void log(string text) => Schedule(() =>
    {
        var entry = terminalEntryPool.Get();
        entry.Text = $"[{DateTime.Now:HH:mm:ss}] {text}";
        Add(entry);
        entry.Hide();
        entry.Show();
    });

    private sealed class TerminalHeader : Container
    {
        public TerminalHeader()
        {
            RelativeSizeAxes = Axes.Both;

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
            };
        }
    }
}
