// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Input;
using VRCOSC.Game.Graphics.Containers.UI.Button;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleRun;

public sealed class RunningPopover : Container
{
    [Resolved]
    private ScreenManager ScreenManager { get; set; }

    private TerminalContainer Terminal;
    private ParameterContainer Parameters;

    public RunningPopover()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
        RelativePositionAxes = Axes.Both;
        Position = new Vector2(0, 1);
        Padding = new MarginPadding(40);
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Child = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            CornerRadius = 20,
            EdgeEffect = VRCOSCEdgeEffects.DispersedShadow,
            BorderThickness = 3,
            Masking = true,
            Children = new Drawable[]
            {
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = VRCOSCColour.Gray3
                },
                new GridContainer
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
                            Terminal = new TerminalContainer(),
                            Parameters = new ParameterContainer()
                        }
                    }
                },
                new Container
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Size = new Vector2(60),
                    Padding = new MarginPadding(10),
                    Child = new IconButton
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        CornerRadius = 10,
                        Icon = { Value = FontAwesome.Solid.Get(0xf00d) },
                        Action = ScreenManager.HideTerminal
                    }
                }
            }
        };
    }

    public void Reset()
    {
        Terminal.ClearTerminal();
        Parameters.ClearParameters();
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

    protected override bool OnClick(ClickEvent e)
    {
        return true;
    }

    protected override bool OnHover(HoverEvent e)
    {
        return true;
    }
}
