﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Screens.Main.Run;

public partial class TerminalContainer : Container<TerminalEntry>
{
    private const int terminal_entry_count = 50;
    private readonly DrawablePool<TerminalEntry> terminalEntryPool = new(terminal_entry_count);

    protected override FillFlowContainer<TerminalEntry> Content { get; }
    private readonly BasicScrollContainer scrollContainer;

    public TerminalContainer()
    {
        InternalChildren = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY7
            },
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding
                {
                    Vertical = 2,
                    Horizontal = 5
                },
                Child = scrollContainer = new BasicScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollbarVisible = false,
                    ClampExtension = 0,
                    ScrollContent =
                    {
                        Child = Content = new FillFlowContainer<TerminalEntry>
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical
                        }
                    }
                }
            }
        };

        Logger.NewEntry += onNewLogEntry;
    }

    private void onNewLogEntry(LogEntry entry)
    {
        if (entry.LoggerName != TerminalLogger.TARGET_NAME) return;

        log(entry.Message);
    }

    public void Reset()
    {
        RemoveAll(_ => true, false);
    }

    protected override void UpdateAfterChildren()
    {
        while (Count > terminal_entry_count)
        {
            var entry = this[0];
            Remove(entry, false);
        }

        scrollContainer.ScrollToEnd();
    }

    private void log(string text)
    {
        var dateTimeText = $"[{DateTime.Now:HH:mm:ss}] {text}";

        Schedule(() =>
        {
            var entry = terminalEntryPool.Get();
            entry.Text = dateTimeText;
            Add(entry);
            entry.Hide();
            entry.Show();
        });
    }
}
