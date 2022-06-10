// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osuTK.Input;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleRun;

public class RunningPopoverContent : Container
{
    [Resolved]
    private ScreenManager ScreenManager { get; set; }

    public TerminalContainer Terminal;
    public ParameterContainer Parameters;

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
        Child = new GridContainer
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            ColumnDimensions = new[]
            {
                new Dimension(),
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
                        Padding = new MarginPadding
                        {
                            Bottom = 15,
                            Left = 15,
                            Right = 15 / 2f
                        },
                        Child = Terminal = new TerminalContainer()
                    },
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding
                        {
                            Bottom = 15,
                            Right = 15,
                            Left = 15 / 2f
                        },
                        Child = Parameters = new ParameterContainer()
                    }
                }
            }
        };
    }

    protected override bool OnKeyDown(KeyDownEvent e)
    {
        if (e.Key != Key.Escape) return base.OnKeyDown(e);

        ScreenManager.HideTerminal();
        return true;
    }
}
