// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Managers;

namespace VRCOSC.Game.Graphics.Run;

public sealed partial class TerminalContainer : Container<TerminalEntry>
{
    private const int terminal_entry_count = 100;

    private readonly BasicScrollContainer terminalScroll;

    protected override FillFlowContainer<TerminalEntry> Content { get; }

    private readonly DrawablePool<TerminalEntry> terminalEntryPool = new(terminal_entry_count);

    [Resolved]
    private GameManager gameManager { get; set; } = null!;

    public TerminalContainer()
    {
        InternalChildren = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = ThemeManager.Current[ThemeAttribute.Darker]
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

        gameManager.State.BindValueChanged(e =>
        {
            if (e.NewValue == GameManagerState.Starting) Reset();
        });
    }

    public void Reset()
    {
        RemoveAll(_ => true, false);
    }

    protected override void UpdateAfterChildren()
    {
        base.UpdateAfterChildren();

        while (Count > terminal_entry_count)
        {
            var entry = this[0];
            Remove(entry, false);
        }

        terminalScroll.ScrollToEnd();
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
