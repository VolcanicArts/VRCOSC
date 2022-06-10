// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleRun;

public sealed class TerminalContainer : Container
{
    [Resolved]
    private ScreenManager ScreenManager { get; set; }

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
                    Padding = new MarginPadding(1.5f),
                    Child = terminalScroll = new BasicScrollContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        ScrollbarVisible = false,
                        ClampExtension = 0,
                        Padding = new MarginPadding
                        {
                            Horizontal = 3
                        },
                        Child = terminalFlow = new FillFlowContainer<TerminalEntry>
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
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
