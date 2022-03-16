// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osuTK.Input;

namespace VRCOSC.Game.Graphics.Containers.Screens.TerminalScreen;

public class TerminalContainer : Container
{
    [Resolved]
    private ScreenManager ScreenManager { get; set; }

    private BasicScrollContainer terminalScroll;
    private FillFlowContainer<SpriteText> terminalFlow;

    [BackgroundDependencyLoader]
    private void load()
    {
        Logger.NewEntry += logEntry =>
        {
            if (logEntry.LoggerName == "terminal")
                log(logEntry.Message);
        };

        Padding = new MarginPadding(40);

        InternalChild = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Masking = true,
            CornerRadius = 20,
            EdgeEffect = VRCOSCEdgeEffects.DispersedShadow,
            BorderThickness = 3,
            Children = new Drawable[]
            {
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = VRCOSCColour.Gray3
                },
                new Container
                {
                    Name = "Content",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(10),
                    Child = terminalScroll = new BasicScrollContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        ClampExtension = 20,
                        ScrollbarVisible = false,
                        Child = terminalFlow = new FillFlowContainer<SpriteText>
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

    private void log(string text)
    {
        Scheduler.Add(() =>
        {
            if (terminalFlow.Count >= 50) terminalFlow[0].RemoveAndDisposeImmediately();
            var formattedText = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {text}";
            terminalFlow.Add(new SpriteText
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Font = FrameworkFont.Regular.With(size: 20),
                Colour = VRCOSCColour.Gray8,
                Text = formattedText
            });
            Scheduler.AddDelayed(() => terminalScroll.ScrollToEnd(), 50);
        });
    }

    public void ClearTerminal()
    {
        terminalFlow.Clear();
    }

    protected override bool OnKeyDown(KeyDownEvent e)
    {
        if (e.Key != Key.Escape) return base.OnKeyDown(e);

        ScreenManager.HideTerminal();
        return true;
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        return true;
    }
}
