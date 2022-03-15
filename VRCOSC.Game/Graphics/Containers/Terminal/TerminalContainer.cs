using System;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;

namespace VRCOSC.Game.Graphics.Containers.Terminal;

public class TerminalContainer : Container
{
    private BasicScrollContainer scrollContainer { get; set; }

    private FillFlowContainer<SpriteText> terminalFlow { get; set; }

    public TerminalContainer()
    {
        Logger.NewEntry += logEntry =>
        {
            if (logEntry.LoggerName == "terminal")
                log(logEntry.Message);
        };

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
                Children = new Drawable[]
                {
                    scrollContainer = new BasicScrollContainer
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

    private void log(string text, LogState state = LogState.Nominal)
    {
        Scheduler.Add(() =>
        {
            if (terminalFlow.Count >= 50) terminalFlow[0].RemoveAndDisposeImmediately();
            var formattedText = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {text}";
            terminalFlow.Add(new SpriteText()
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Text = formattedText,
                Font = FrameworkFont.Regular.With(size: 20),
                Colour = getColourOfLogState(state)
            });
            Scheduler.Add(() => scrollContainer.ScrollToEnd());
        });
    }

    public void ClearTerminal()
    {
        Scheduler.Add(() => terminalFlow.ForEach(d => d.FadeOut(250, Easing.OutCubic).Finally(_ => d.RemoveAndDisposeImmediately())));
    }

    private static Colour4 getColourOfLogState(LogState state)
    {
        return state switch
        {
            LogState.Nominal => VRCOSCColour.Gray8,
            LogState.Important => VRCOSCColour.Green,
            LogState.Error => VRCOSCColour.Red,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }
}

public enum LogState
{
    Nominal,
    Important,
    Error
}
