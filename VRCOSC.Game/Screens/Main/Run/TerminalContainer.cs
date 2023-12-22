// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using VRCOSC.Graphics;

namespace VRCOSC.Screens.Main.Run;

public partial class TerminalContainer : Container<TerminalEntry>
{
    [Resolved]
    private AppManager appManager { get; set; } = null!;

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
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Logger.NewEntry += onNewLogEntry;
        appManager.State.BindValueChanged(onAppManagerStateChange);
    }

    private void onAppManagerStateChange(ValueChangedEvent<AppManagerState> e) => Scheduler.Add(() =>
    {
        if (e.NewValue == AppManagerState.Starting) reset();
    }, false);

    private void onNewLogEntry(LogEntry entry)
    {
        if (entry.LoggerName != TerminalLogger.TARGET_NAME) return;

        log(entry.Message);
    }

    private void reset()
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

        Scheduler.Add(() =>
        {
            var entry = terminalEntryPool.Get();
            entry.Text = dateTimeText;
            Add(entry);
            entry.Hide();
            entry.Show();
        }, false);
    }
}
